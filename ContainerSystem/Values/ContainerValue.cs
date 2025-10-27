/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using System.Text;
using System.Text.Json;
using ContainerSystem.Core;

namespace ContainerSystem.Values;

/// <summary>
/// Nested container value for hierarchical data structures.
/// Equivalent to C++ container_value class.
/// </summary>
public class ContainerValue : Value
{
    public ContainerValue(string name) : base(name, new List<Value>())
    {
    }

    public ContainerValue(string name, List<Value> children) : base(name, children)
    {
        // Set parent references for all children
        foreach (var child in children)
        {
            child.SetParent(this);
        }
    }

    /// <summary>
    /// Adds a value to this container.
    /// </summary>
    /// <param name="value">The value to add</param>
    /// <returns>The added value for fluent API</returns>
    public Value Add(Value value)
    {
        AddChild(value);
        return value;
    }

    /// <summary>
    /// Removes a value by name.
    /// </summary>
    /// <param name="name">Name of the value to remove</param>
    public void Remove(string name)
    {
        var toRemove = Children().Where(c => c.Name == name).ToList();
        foreach (var child in toRemove)
        {
            Children().Remove(child);
        }
    }

    /// <summary>
    /// Removes all child values.
    /// </summary>
    public void RemoveAll()
    {
        Children().Clear();
    }

    /// <summary>
    /// Gets a value by name.
    /// </summary>
    /// <param name="name">Name to search for</param>
    /// <param name="index">Index if multiple matches exist (default: 0)</param>
    /// <returns>Matching value or null</returns>
    public Value? GetValue(string name, int index = 0)
    {
        var matches = ValueArray(name);
        return index < matches.Count ? matches[index] : null;
    }

    public override string Data()
    {
        return $"Container({ChildCount} values)";
    }

    public override int Size()
    {
        // Calculate total size: header + all children
        int totalSize = sizeof(int); // Child count
        foreach (var child in Children())
        {
            totalSize += child.Size();
        }
        return totalSize;
    }

    public override byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Write child count
        writer.Write(ChildCount);

        // Write each child's serialized data
        foreach (var child in Children())
        {
            var childData = child.Serialize();
            writer.Write(childData.Length);
            writer.Write(childData);
        }

        return ms.ToArray();
    }

    public override string ToJson()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var data = new
        {
            name = Name,
            type = (int)Type,
            type_name = "container",
            child_count = ChildCount,
            children = Children().Select(c =>
            {
                // Parse child's JSON to include in this object
                try
                {
                    return JsonDocument.Parse(c.ToJson()).RootElement;
                }
                catch
                {
                    // Fallback if child JSON is malformed
                    return JsonDocument.Parse("{}").RootElement;
                }
            }).ToList()
        };

        return JsonSerializer.Serialize(data, options);
    }

    public override string ToXml()
    {
        var sb = new StringBuilder();
        sb.Append($"<container name=\"{Name}\" child_count=\"{ChildCount}\">");

        foreach (var child in Children())
        {
            sb.Append(child.ToXml());
        }

        sb.Append("</container>");
        return sb.ToString();
    }

    public override string ToString()
    {
        return Data();
    }

    public override bool ToBoolean()
    {
        return ChildCount > 0;
    }
}
