/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using System.Text;
using System.Text.RegularExpressions;
using ContainerSystem.Values;

namespace ContainerSystem.Core;

/// <summary>
/// Implements C++ Wire Protocol serialization/deserialization for cross-language compatibility.
/// Equivalent to C++ value_container wire format.
///
/// Wire Protocol Format:
/// @header={{[field_id,value];...}}@data={{[name,type,value];...}};
///
/// Header Field IDs:
/// - 1: target_id
/// - 2: target_sub_id
/// - 3: source_id
/// - 4: source_sub_id
/// - 5: message_type
/// - 6: version
/// </summary>
public static class WireProtocol
{
    // Header field ID constants (matching C++ implementation)
    private const int HeaderFieldTargetId = 1;
    private const int HeaderFieldTargetSubId = 2;
    private const int HeaderFieldSourceId = 3;
    private const int HeaderFieldSourceSubId = 4;
    private const int HeaderFieldMessageType = 5;
    private const int HeaderFieldVersion = 6;

    // Wire protocol markers
    private const string HeaderPrefix = "@header={{";
    private const string DataPrefix = "@data={{";
    private const string BlockSuffix = "}}";
    private const string ProtocolSuffix = ";";

    // Regex patterns for parsing
    private static readonly Regex HeaderPattern = new(@"@header=\{\{(.*?)\}\}", RegexOptions.Compiled);
    private static readonly Regex DataPattern = new(@"@data=\{\{(.*?)\}\}", RegexOptions.Compiled);
    private static readonly Regex EntryPattern = new(@"\[([^\]]*)\]", RegexOptions.Compiled);

    /// <summary>
    /// Serializes a ValueContainer to C++ Wire Protocol format.
    /// </summary>
    /// <param name="container">The container to serialize</param>
    /// <returns>Wire protocol string</returns>
    public static string Serialize(ValueContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);

        var sb = new StringBuilder();

        // Build header section
        sb.Append(HeaderPrefix);
        AppendHeaderField(sb, HeaderFieldTargetId, container.TargetId);
        AppendHeaderField(sb, HeaderFieldTargetSubId, container.TargetSubId);
        AppendHeaderField(sb, HeaderFieldSourceId, container.SourceId);
        AppendHeaderField(sb, HeaderFieldSourceSubId, container.SourceSubId);
        AppendHeaderField(sb, HeaderFieldMessageType, container.MessageType);
        AppendHeaderField(sb, HeaderFieldVersion, container.Version);
        sb.Append(BlockSuffix);

        // Build data section
        sb.Append(DataPrefix);
        var values = container.Values();
        for (int i = 0; i < values.Count; i++)
        {
            AppendDataEntry(sb, values[i]);
        }
        sb.Append(BlockSuffix);
        sb.Append(ProtocolSuffix);

        return sb.ToString();
    }

    /// <summary>
    /// Serializes a ValueContainer to C++ Wire Protocol format as byte array.
    /// </summary>
    /// <param name="container">The container to serialize</param>
    /// <returns>Wire protocol bytes (UTF-8)</returns>
    public static byte[] SerializeToBytes(ValueContainer container)
    {
        return Encoding.UTF8.GetBytes(Serialize(container));
    }

    /// <summary>
    /// Deserializes a C++ Wire Protocol string to ValueContainer.
    /// </summary>
    /// <param name="wireData">The wire protocol string</param>
    /// <returns>Deserialized container</returns>
    /// <exception cref="FormatException">If wire data is malformed</exception>
    public static ValueContainer Deserialize(string wireData)
    {
        if (string.IsNullOrEmpty(wireData))
        {
            throw new ArgumentException("Wire data cannot be null or empty", nameof(wireData));
        }

        var container = new ValueContainer();

        // Parse header section
        var headerMatch = HeaderPattern.Match(wireData);
        if (headerMatch.Success)
        {
            ParseHeader(container, headerMatch.Groups[1].Value);
        }

        // Parse data section
        var dataMatch = DataPattern.Match(wireData);
        if (dataMatch.Success)
        {
            ParseData(container, dataMatch.Groups[1].Value);
        }

        return container;
    }

    /// <summary>
    /// Deserializes a C++ Wire Protocol byte array to ValueContainer.
    /// </summary>
    /// <param name="wireData">The wire protocol bytes (UTF-8)</param>
    /// <returns>Deserialized container</returns>
    public static ValueContainer Deserialize(byte[] wireData)
    {
        ArgumentNullException.ThrowIfNull(wireData);
        return Deserialize(Encoding.UTF8.GetString(wireData));
    }

    /// <summary>
    /// Attempts to deserialize wire data without throwing exceptions.
    /// </summary>
    /// <param name="wireData">The wire protocol string</param>
    /// <param name="container">The deserialized container if successful</param>
    /// <returns>True if deserialization succeeded and wire data is valid format</returns>
    public static bool TryDeserialize(string wireData, out ValueContainer? container)
    {
        container = null;

        if (string.IsNullOrEmpty(wireData))
            return false;

        // Check for valid wire protocol format markers
        if (!wireData.Contains("@header={{") || !wireData.Contains("@data={{"))
            return false;

        try
        {
            container = Deserialize(wireData);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #region Private Serialization Helpers

    private static void AppendHeaderField(StringBuilder sb, int fieldId, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            sb.Append('[');
            sb.Append(fieldId);
            sb.Append(',');
            sb.Append(EscapeValue(value));
            sb.Append(']');
            sb.Append(';');
        }
    }

    private static void AppendDataEntry(StringBuilder sb, Value value)
    {
        sb.Append('[');
        sb.Append(EscapeValue(value.Name));
        sb.Append(',');
        sb.Append((int)value.Type);
        sb.Append(',');
        sb.Append(SerializeValueData(value));
        sb.Append(']');
        sb.Append(';');
    }

    private static string SerializeValueData(Value value)
    {
        return value.Type switch
        {
            ValueTypes.NullValue => string.Empty,
            ValueTypes.BoolValue => value.ToBoolean() ? "1" : "0",
            ValueTypes.BytesValue => Convert.ToBase64String(value.ToBytes()),
            ValueTypes.ContainerValue => SerializeNestedContainer(value),
            ValueTypes.ArrayValue => SerializeArrayValue(value),
            _ => EscapeValue(value.Data())
        };
    }

    private static string SerializeNestedContainer(Value value)
    {
        // For nested containers, recursively serialize children
        var children = value.Children();
        if (children.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append('{');
        for (int i = 0; i < children.Count; i++)
        {
            if (i > 0) sb.Append('|');
            sb.Append(children[i].Name);
            sb.Append(':');
            sb.Append((int)children[i].Type);
            sb.Append(':');
            sb.Append(SerializeValueData(children[i]));
        }
        sb.Append('}');
        return sb.ToString();
    }

    private static string SerializeArrayValue(Value value)
    {
        var children = value.Children();
        if (children.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append('<');
        for (int i = 0; i < children.Count; i++)
        {
            if (i > 0) sb.Append('|');
            sb.Append((int)children[i].Type);
            sb.Append(':');
            sb.Append(SerializeValueData(children[i]));
        }
        sb.Append('>');
        return sb.ToString();
    }

    private static string EscapeValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        // Escape special characters for wire protocol
        return value
            .Replace("\\", "\\\\")
            .Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace(",", "\\,")
            .Replace(";", "\\;")
            .Replace("{", "\\{")
            .Replace("}", "\\}")
            .Replace("|", "\\|")
            .Replace(":", "\\:");
    }

    #endregion

    #region Private Deserialization Helpers

    /// <summary>
    /// Extracts entries from content, properly handling escaped brackets.
    /// Returns the content inside each [...] without the brackets.
    /// </summary>
    private static List<string> ExtractEntries(string content)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool escaped = false;
        bool inEntry = false;

        foreach (char c in content)
        {
            if (escaped)
            {
                if (inEntry)
                {
                    current.Append('\\');
                    current.Append(c);
                }
                escaped = false;
            }
            else if (c == '\\')
            {
                escaped = true;
            }
            else if (c == '[' && !inEntry)
            {
                inEntry = true;
                current.Clear();
            }
            else if (c == ']' && inEntry)
            {
                result.Add(current.ToString());
                current.Clear();
                inEntry = false;
            }
            else if (inEntry)
            {
                current.Append(c);
            }
        }

        return result;
    }

    private static void ParseHeader(ValueContainer container, string headerContent)
    {
        var entries = ExtractEntries(headerContent);
        foreach (var entryContent in entries)
        {
            var parts = SplitEntry(entryContent, 2);
            if (parts.Length >= 2 && int.TryParse(parts[0], out var fieldId))
            {
                var value = UnescapeValue(parts[1]);
                ApplyHeaderField(container, fieldId, value);
            }
        }
    }

    private static void ApplyHeaderField(ValueContainer container, int fieldId, string value)
    {
        switch (fieldId)
        {
            case HeaderFieldTargetId:
                container.SetTarget(value, container.TargetSubId);
                break;
            case HeaderFieldTargetSubId:
                container.SetTarget(container.TargetId, value);
                break;
            case HeaderFieldSourceId:
                container.SetSource(value, container.SourceSubId);
                break;
            case HeaderFieldSourceSubId:
                container.SetSource(container.SourceId, value);
                break;
            case HeaderFieldMessageType:
                container.MessageType = value;
                break;
            case HeaderFieldVersion:
                // Version is read-only in current implementation
                // This would require adding a SetVersion method
                break;
        }
    }

    private static void ParseData(ValueContainer container, string dataContent)
    {
        var entries = ExtractEntries(dataContent);
        foreach (var entryContent in entries)
        {
            var parts = SplitEntry(entryContent, 3);
            if (parts.Length >= 3)
            {
                var name = UnescapeValue(parts[0]);
                if (int.TryParse(parts[1], out var typeId))
                {
                    var valueType = (ValueTypes)typeId;
                    var data = UnescapeValue(parts[2]);
                    var value = CreateValue(name, valueType, data);
                    if (value != null)
                    {
                        container.Add(value);
                    }
                }
            }
        }
    }

    private static Value? CreateValue(string name, ValueTypes type, string data)
    {
        try
        {
            return type switch
            {
                ValueTypes.NullValue => null,
                ValueTypes.BoolValue => new BoolValue(name, data == "1" || data.Equals("true", StringComparison.OrdinalIgnoreCase)),
                ValueTypes.ShortValue => new ShortValue(name, short.Parse(data)),
                ValueTypes.UShortValue => new UShortValue(name, ushort.Parse(data)),
                ValueTypes.IntValue => new IntValue(name, int.Parse(data)),
                ValueTypes.UIntValue => new UIntValue(name, uint.Parse(data)),
                ValueTypes.LongValue => new LongValue(name, long.Parse(data)),
                ValueTypes.ULongValue => new ULongValue(name, ulong.Parse(data)),
                ValueTypes.LLongValue => new LLongValue(name, long.Parse(data)),
                ValueTypes.ULLongValue => new ULLongValue(name, ulong.Parse(data)),
                ValueTypes.FloatValue => new FloatValue(name, float.Parse(data)),
                ValueTypes.DoubleValue => new DoubleValue(name, double.Parse(data)),
                ValueTypes.StringValue => new StringValue(name, data),
                ValueTypes.BytesValue => new BytesValue(name, Convert.FromBase64String(data)),
                ValueTypes.ContainerValue => ParseNestedContainer(name, data),
                ValueTypes.ArrayValue => ParseArrayValue(name, data),
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    private static ContainerValue ParseNestedContainer(string name, string data)
    {
        var container = new ContainerValue(name);

        if (string.IsNullOrEmpty(data) || data.Length < 2)
        {
            return container;
        }

        // Remove surrounding braces
        var content = data.Substring(1, data.Length - 2);
        var parts = SplitByDelimiter(content, '|');

        foreach (var part in parts)
        {
            var segments = SplitByDelimiter(part, ':');
            if (segments.Length >= 3)
            {
                var childName = segments[0];
                if (int.TryParse(segments[1], out var typeId))
                {
                    var childData = string.Join(":", segments.Skip(2));
                    var child = CreateValue(childName, (ValueTypes)typeId, childData);
                    if (child != null)
                    {
                        container.Add(child);
                    }
                }
            }
        }

        return container;
    }

    private static ArrayValue ParseArrayValue(string name, string data)
    {
        var array = new ArrayValue(name);

        if (string.IsNullOrEmpty(data) || data.Length < 2)
        {
            return array;
        }

        // Remove surrounding angle brackets
        var content = data.Substring(1, data.Length - 2);
        var parts = SplitByDelimiter(content, '|');
        int index = 0;

        foreach (var part in parts)
        {
            var segments = SplitByDelimiter(part, ':');
            if (segments.Length >= 2)
            {
                if (int.TryParse(segments[0], out var typeId))
                {
                    var itemData = string.Join(":", segments.Skip(1));
                    var item = CreateValue($"item_{index++}", (ValueTypes)typeId, itemData);
                    if (item != null)
                    {
                        array.AddChild(item);
                    }
                }
            }
        }

        return array;
    }

    private static string[] SplitEntry(string content, int maxParts)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool escaped = false;

        foreach (char c in content)
        {
            if (escaped)
            {
                current.Append(c);
                escaped = false;
            }
            else if (c == '\\')
            {
                escaped = true;
            }
            else if (c == ',' && result.Count < maxParts - 1)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());
        return result.ToArray();
    }

    private static string[] SplitByDelimiter(string content, char delimiter)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool escaped = false;
        int braceDepth = 0;
        int angleDepth = 0;

        foreach (char c in content)
        {
            if (escaped)
            {
                current.Append(c);
                escaped = false;
            }
            else if (c == '\\')
            {
                escaped = true;
                current.Append(c);
            }
            else if (c == '{')
            {
                braceDepth++;
                current.Append(c);
            }
            else if (c == '}')
            {
                braceDepth--;
                current.Append(c);
            }
            else if (c == '<')
            {
                angleDepth++;
                current.Append(c);
            }
            else if (c == '>')
            {
                angleDepth--;
                current.Append(c);
            }
            else if (c == delimiter && braceDepth == 0 && angleDepth == 0)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
        {
            result.Add(current.ToString());
        }

        return result.ToArray();
    }

    private static string UnescapeValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var result = new StringBuilder();
        bool escaped = false;

        foreach (char c in value)
        {
            if (escaped)
            {
                result.Append(c);
                escaped = false;
            }
            else if (c == '\\')
            {
                escaped = true;
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }

    #endregion
}
