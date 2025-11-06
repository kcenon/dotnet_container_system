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
/// Array/list value for storing collections of values.
/// ArrayValue (type 15) is an extension to support homogeneous or heterogeneous
/// collections of values, similar to JSON arrays.
///
/// Wire format (binary):
/// [type:1=15][name_len:4 LE][name:UTF-8][value_size:4 LE][count:4 LE][values...]
///
/// Text format:
/// [name,15,count];[element1][element2]...
///
/// Equivalent to C++ array_value class.
/// </summary>
public class ArrayValue : Value
{
    private readonly List<Value> _elements;

    /// <summary>
    /// Creates an empty array.
    /// </summary>
    /// <param name="name">The name/key of this array</param>
    public ArrayValue(string name) : base(name, new List<Value>())
    {
        _elements = Children();
    }

    /// <summary>
    /// Creates an array with initial elements.
    /// </summary>
    /// <param name="name">The name/key of this array</param>
    /// <param name="elements">Initial elements</param>
    public ArrayValue(string name, List<Value> elements) : base(name, elements)
    {
        _elements = Children();

        // Set parent references for all elements
        foreach (var element in _elements)
        {
            element.SetParent(this);
        }
    }

    /// <summary>
    /// Gets the type discriminator.
    /// </summary>
    public override ValueTypes Type => ValueTypes.ArrayValue;

    /// <summary>
    /// Gets the number of elements in the array.
    /// </summary>
    public int Count => _elements.Count;

    /// <summary>
    /// Checks if the array is empty.
    /// </summary>
    public bool IsEmpty => _elements.Count == 0;

    /// <summary>
    /// Adds an element to the end of the array.
    /// </summary>
    /// <param name="value">The value to add</param>
    /// <returns>The added value for fluent API</returns>
    public Value Append(Value value)
    {
        AddChild(value);
        return value;
    }

    /// <summary>
    /// Adds an element (C++ compatibility name).
    /// </summary>
    /// <param name="value">The value to add</param>
    public void PushBack(Value value)
    {
        AddChild(value);
    }

    /// <summary>
    /// Gets element at index.
    /// </summary>
    /// <param name="index">The index</param>
    /// <returns>The element at index</returns>
    /// <exception cref="IndexOutOfRangeException">If index is invalid</exception>
    public Value At(int index)
    {
        if (index < 0 || index >= _elements.Count)
        {
            throw new IndexOutOfRangeException($"ArrayValue index {index} out of range (size: {_elements.Count})");
        }
        return _elements[index];
    }

    /// <summary>
    /// Indexer for C# array syntax.
    /// </summary>
    /// <param name="index">The index</param>
    /// <returns>The element at index</returns>
    public Value this[int index]
    {
        get => At(index);
        set
        {
            if (index < 0 || index >= _elements.Count)
            {
                throw new IndexOutOfRangeException($"ArrayValue index {index} out of range (size: {_elements.Count})");
            }
            value.SetParent(this);
            _elements[index] = value;
        }
    }

    /// <summary>
    /// Gets all elements.
    /// </summary>
    /// <returns>Read-only list of elements</returns>
    public IReadOnlyList<Value> Elements => _elements.AsReadOnly();

    /// <summary>
    /// Clears all elements.
    /// </summary>
    public void Clear()
    {
        _elements.Clear();
    }

    public override string Data()
    {
        return $"Array({_elements.Count} elements)";
    }

    public override int Size()
    {
        // Calculate total size: count (4 bytes) + all elements
        int totalSize = sizeof(int);
        foreach (var element in _elements)
        {
            totalSize += element.Size();
        }
        return totalSize;
    }

    public override byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Write element count
        writer.Write(_elements.Count);

        // Write all elements with full wire format (type, name, size, value)
        foreach (var element in _elements)
        {
            // Serialize element with header for proper deserialization
            var elementData = ValueFactory.SerializeWithHeader(element);
            writer.Write(elementData);
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Deserialize from binary data.
    /// </summary>
    /// <param name="name">The name for the array</param>
    /// <param name="data">Binary data</param>
    /// <returns>Deserialized ArrayValue</returns>
    public static ArrayValue Deserialize(string name, byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);

        // Read count
        int count = reader.ReadInt32();

        var elements = new List<Value>(count);

        // Deserialize all elements using ValueFactory
        for (int i = 0; i < count; i++)
        {
            var element = ValueFactory.DeserializeFromReader(reader);
            elements.Add(element);
        }

        return new ArrayValue(name, elements);
    }

    public override string ToString()
    {
        var type = Type.ToTypeString();
        var sb = new StringBuilder();

        // Format: [name,type,count];[element1][element2]...
        sb.Append($"[{Name},{type},{_elements.Count}];");

        foreach (var element in _elements)
        {
            sb.Append(element.ToString());
        }

        return sb.ToString();
    }

    /// <summary>
    /// Convert to JSON format.
    /// </summary>
    public string ToJson()
    {
        var jsonElements = _elements.Select(e =>
        {
            if (e is ContainerValue cv)
                return cv.ToJson();
            else if (e is ArrayValue av)
                return av.ToJson();
            else
                return JsonSerializer.Serialize(new
                {
                    name = e.Name,
                    type = e.Type.ToTypeString(),
                    data = e.Data()
                });
        }).ToList();

        var result = new
        {
            name = Name,
            type = "array",
            elements = jsonElements
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// LINQ support: Get enumerator for iteration.
    /// </summary>
    public IEnumerator<Value> GetEnumerator() => _elements.GetEnumerator();
}
