/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using System.Text;
using System.Text.Json;
using ContainerSystem.Values;

namespace ContainerSystem.Core;

/// <summary>
/// A high-level container for messages, including source/target IDs, message type,
/// and a list of values.
/// Equivalent to C++ value_container class.
/// </summary>
public class ValueContainer
{
    private string _messageType;
    private string _sourceId;
    private string _sourceSubId;
    private string _targetId;
    private string _targetSubId;
    private string _version;
    private readonly List<Value> _values;
    private readonly object _lock = new();

    /// <summary>
    /// Default constructor: sets up a "data_container" type with version "1.0.0.0".
    /// </summary>
    public ValueContainer()
    {
        _messageType = "data_container";
        _sourceId = string.Empty;
        _sourceSubId = string.Empty;
        _targetId = string.Empty;
        _targetSubId = string.Empty;
        _version = "1.0.0.0";
        _values = new List<Value>();
    }

    /// <summary>
    /// Construct from a serialized JSON string.
    /// </summary>
    /// <param name="dataString">Serialized container data</param>
    public ValueContainer(string dataString)
    {
        _values = new List<Value>();
        Deserialize(dataString);
        _messageType ??= "data_container";
        _version ??= "1.0.0.0";
        _sourceId ??= string.Empty;
        _sourceSubId ??= string.Empty;
        _targetId ??= string.Empty;
        _targetSubId ??= string.Empty;
    }

    /// <summary>
    /// Construct from a byte array.
    /// </summary>
    /// <param name="dataArray">Byte array containing serialized data</param>
    public ValueContainer(byte[] dataArray)
    {
        _values = new List<Value>();
        var dataString = Encoding.UTF8.GetString(dataArray);
        Deserialize(dataString);
        _messageType ??= "data_container";
        _version ??= "1.0.0.0";
        _sourceId ??= string.Empty;
        _sourceSubId ??= string.Empty;
        _targetId ??= string.Empty;
        _targetSubId ??= string.Empty;
    }

    /// <summary>
    /// Construct with full metadata specification.
    /// </summary>
    /// <param name="sourceId">Source ID</param>
    /// <param name="sourceSubId">Source sub ID</param>
    /// <param name="targetId">Target ID</param>
    /// <param name="targetSubId">Target sub ID</param>
    /// <param name="messageType">Message type</param>
    /// <param name="version">Protocol version</param>
    public ValueContainer(
        string sourceId,
        string sourceSubId,
        string targetId,
        string targetSubId,
        string messageType,
        string version = "1.0.0.0")
    {
        _sourceId = sourceId;
        _sourceSubId = sourceSubId;
        _targetId = targetId;
        _targetSubId = targetSubId;
        _messageType = messageType;
        _version = version;
        _values = new List<Value>();
    }

    /// <summary>
    /// Gets or sets the message type.
    /// </summary>
    public string MessageType
    {
        get => _messageType;
        set => _messageType = value;
    }

    /// <summary>
    /// Gets the source ID.
    /// </summary>
    public string SourceId => _sourceId;

    /// <summary>
    /// Gets the source sub ID.
    /// </summary>
    public string SourceSubId => _sourceSubId;

    /// <summary>
    /// Gets the target ID.
    /// </summary>
    public string TargetId => _targetId;

    /// <summary>
    /// Gets the target sub ID.
    /// </summary>
    public string TargetSubId => _targetSubId;

    /// <summary>
    /// Gets the version.
    /// </summary>
    public string Version => _version;

    /// <summary>
    /// Gets all units/values in the container (read-only access).
    /// </summary>
    public IReadOnlyList<Value> Units
    {
        get
        {
            lock (_lock)
            {
                return _values.AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Sets the source IDs.
    /// </summary>
    /// <param name="sourceId">Main source ID</param>
    /// <param name="sourceSubId">Source sub ID</param>
    public void SetSource(string sourceId, string sourceSubId = "")
    {
        _sourceId = sourceId;
        _sourceSubId = sourceSubId;
    }

    /// <summary>
    /// Sets the target IDs.
    /// </summary>
    /// <param name="targetId">Main target ID</param>
    /// <param name="targetSubId">Target sub ID</param>
    public void SetTarget(string targetId, string targetSubId = "")
    {
        _targetId = targetId;
        _targetSubId = targetSubId;
    }

    /// <summary>
    /// Adds a value to the container.
    /// </summary>
    /// <param name="value">Value to add</param>
    public void Add(Value value)
    {
        lock (_lock)
        {
            _values.Add(value);
        }
    }

    /// <summary>
    /// Gets a single value by name.
    /// </summary>
    /// <param name="key">Name of the value to find</param>
    /// <returns>The value if found, null otherwise</returns>
    public Value? GetValue(string key)
    {
        lock (_lock)
        {
            return _values.FirstOrDefault(v => v.Name == key);
        }
    }

    /// <summary>
    /// Gets all values with the specified name.
    /// </summary>
    /// <param name="key">Name to search for</param>
    /// <returns>List of matching values</returns>
    public List<Value> ValueArray(string key)
    {
        lock (_lock)
        {
            return _values.Where(v => v.Name == key).ToList();
        }
    }

    /// <summary>
    /// Gets all values in the container.
    /// </summary>
    /// <returns>List of all values</returns>
    public List<Value> Values()
    {
        lock (_lock)
        {
            return new List<Value>(_values);
        }
    }

    /// <summary>
    /// Serializes the container to a JSON string.
    /// </summary>
    /// <returns>JSON string representation</returns>
    public string Serialize() => ToJson();

    /// <summary>
    /// Converts the container to JSON format (flat Python/.NET format).
    /// </summary>
    /// <returns>JSON string representation</returns>
    public string ToJson()
    {
        lock (_lock)
        {
            var sb = new StringBuilder();
            sb.Append('{');
            sb.Append($"\"message_type\":\"{_messageType}\",");
            sb.Append($"\"version\":\"{_version}\",");
            sb.Append($"\"source_id\":\"{_sourceId}\",");
            sb.Append($"\"source_sub_id\":\"{_sourceSubId}\",");
            sb.Append($"\"target_id\":\"{_targetId}\",");
            sb.Append($"\"target_sub_id\":\"{_targetSubId}\",");
            sb.Append("\"values\":[");

            for (int i = 0; i < _values.Count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(_values[i].ToJson());
            }

            sb.Append("]}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Deserializes JSON string into this container.
    /// </summary>
    /// <param name="dataString">JSON string to deserialize</param>
    private void Deserialize(string dataString)
    {
        try
        {
            using var doc = JsonDocument.Parse(dataString);
            var root = doc.RootElement;

            if (root.TryGetProperty("message_type", out var msgType))
                _messageType = msgType.GetString() ?? "data_container";

            if (root.TryGetProperty("version", out var ver))
                _version = ver.GetString() ?? "1.0.0.0";

            if (root.TryGetProperty("source_id", out var srcId))
                _sourceId = srcId.GetString() ?? string.Empty;

            if (root.TryGetProperty("source_sub_id", out var srcSubId))
                _sourceSubId = srcSubId.GetString() ?? string.Empty;

            if (root.TryGetProperty("target_id", out var tgtId))
                _targetId = tgtId.GetString() ?? string.Empty;

            if (root.TryGetProperty("target_sub_id", out var tgtSubId))
                _targetSubId = tgtSubId.GetString() ?? string.Empty;

            // Parse values array
            if (root.TryGetProperty("values", out var valuesArray) && valuesArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var valueElement in valuesArray.EnumerateArray())
                {
                    var value = ParseValueFromJson(valueElement);
                    if (value != null)
                        _values.Add(value);
                }
            }
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize container: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Parses a single value from a JSON element.
    /// </summary>
    /// <param name="element">JSON element containing value data</param>
    /// <returns>Parsed Value object or null if invalid</returns>
    private static Value? ParseValueFromJson(JsonElement element)
    {
        if (!element.TryGetProperty("name", out var nameElem))
            return null;

        var name = nameElem.GetString() ?? string.Empty;

        if (!element.TryGetProperty("type", out var typeElem))
            return null;

        var typeId = typeElem.GetInt32();
        var valueType = (ValueTypes)typeId;

        // Handle container type (nested)
        if (valueType == ValueTypes.ContainerValue)
        {
            var container = new ContainerValue(name);
            if (element.TryGetProperty("children", out var childrenArray) && childrenArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var childElement in childrenArray.EnumerateArray())
                {
                    var child = ParseValueFromJson(childElement);
                    if (child != null)
                        container.Add(child);
                }
            }
            return container;
        }

        // Get data element
        if (!element.TryGetProperty("data", out var dataElem))
            return null;

        // Parse based on value type
        try
        {
            return valueType switch
            {
                ValueTypes.BoolValue => new BoolValue(name, dataElem.GetBoolean()),
                ValueTypes.ShortValue => new ShortValue(name, dataElem.GetInt16()),
                ValueTypes.UShortValue => new UShortValue(name, dataElem.GetUInt16()),
                ValueTypes.IntValue => new IntValue(name, dataElem.GetInt32()),
                ValueTypes.UIntValue => new UIntValue(name, dataElem.GetUInt32()),
                ValueTypes.LongValue => new LongValue(name, dataElem.GetInt64()),
                ValueTypes.ULongValue => new ULongValue(name, dataElem.GetUInt64()),
                ValueTypes.LLongValue => new LLongValue(name, dataElem.GetInt64()),
                ValueTypes.ULLongValue => new ULLongValue(name, dataElem.GetUInt64()),
                ValueTypes.FloatValue => new FloatValue(name, dataElem.GetSingle()),
                ValueTypes.DoubleValue => new DoubleValue(name, dataElem.GetDouble()),
                ValueTypes.StringValue => new StringValue(name, dataElem.GetString() ?? string.Empty),
                ValueTypes.BytesValue => ParseBytesValue(name, element),
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parses a BytesValue with base64 decoding support.
    /// </summary>
    /// <param name="name">Value name</param>
    /// <param name="element">JSON element</param>
    /// <returns>BytesValue or null</returns>
    private static BytesValue? ParseBytesValue(string name, JsonElement element)
    {
        if (!element.TryGetProperty("data", out var dataElem))
            return null;

        var dataStr = dataElem.GetString();
        if (string.IsNullOrEmpty(dataStr))
            return new BytesValue(name, Array.Empty<byte>());

        // Check for base64 encoding
        if (element.TryGetProperty("encoding", out var encodingElem) &&
            encodingElem.GetString() == "base64")
        {
            try
            {
                var bytes = Convert.FromBase64String(dataStr);
                return new BytesValue(name, bytes);
            }
            catch
            {
                return null;
            }
        }

        // Otherwise treat as UTF-8 string
        var utf8Bytes = Encoding.UTF8.GetBytes(dataStr);
        return new BytesValue(name, utf8Bytes);
    }

    /// <summary>
    /// Converts the container to XML format.
    /// </summary>
    /// <returns>XML string representation</returns>
    public string ToXml()
    {
        lock (_lock)
        {
            var sb = new StringBuilder();
            sb.Append("<container ");
            sb.Append($"message_type=\"{_messageType}\" ");
            sb.Append($"version=\"{_version}\" ");
            sb.Append($"source_id=\"{_sourceId}\" ");
            sb.Append($"source_sub_id=\"{_sourceSubId}\" ");
            sb.Append($"target_id=\"{_targetId}\" ");
            sb.Append($"target_sub_id=\"{_targetSubId}\">");

            foreach (var value in _values)
            {
                sb.Append(value.ToXml());
            }

            sb.Append("</container>");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Gets the number of values in the container.
    /// </summary>
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _values.Count;
            }
        }
    }
}
