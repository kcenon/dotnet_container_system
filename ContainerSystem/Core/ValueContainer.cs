/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using System.Text;
using System.Text.Json;

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
    public string Serialize()
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

            // TODO: Parse values array
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize container: {ex.Message}", ex);
        }
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
