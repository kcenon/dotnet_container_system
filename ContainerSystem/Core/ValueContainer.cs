/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using System.Collections;
using System.Text;
using System.Text.Json;
using ContainerSystem.Values;

namespace ContainerSystem.Core;

/// <summary>
/// A high-level container for messages, including source/target IDs, message type,
/// and a list of values.
/// Equivalent to C++ value_container class.
///
/// Features:
/// - STL-style iteration via IEnumerable&lt;Value&gt;
/// - Conditional thread safety with ReaderWriterLockSlim
/// - Memory footprint tracking
/// - Read/write statistics
/// </summary>
public class ValueContainer : IEnumerable<Value>, IDisposable
{
    private string _messageType;
    private string _sourceId;
    private string _sourceSubId;
    private string _targetId;
    private string _targetSubId;
    private string _version;
    private readonly List<Value> _values;

    // Thread safety
    private readonly ReaderWriterLockSlim _rwLock;
    private volatile bool _threadSafeEnabled;

    // Statistics (C++ compatible)
    private long _readCount;
    private long _writeCount;
    private long _serializationCount;

    private bool _disposed;

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
        _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        _threadSafeEnabled = false;
    }

    /// <summary>
    /// Construct from a serialized JSON string.
    /// </summary>
    /// <param name="dataString">Serialized container data</param>
    public ValueContainer(string dataString) : this()
    {
        Deserialize(dataString);
    }

    /// <summary>
    /// Construct from a byte array.
    /// </summary>
    /// <param name="dataArray">Byte array containing serialized data</param>
    public ValueContainer(byte[] dataArray) : this()
    {
        var dataString = Encoding.UTF8.GetString(dataArray);
        Deserialize(dataString);
    }

    /// <summary>
    /// Construct with full metadata specification.
    /// </summary>
    public ValueContainer(
        string sourceId,
        string sourceSubId,
        string targetId,
        string targetSubId,
        string messageType,
        string version = "1.0.0.0") : this()
    {
        _sourceId = sourceId;
        _sourceSubId = sourceSubId;
        _targetId = targetId;
        _targetSubId = targetSubId;
        _messageType = messageType;
        _version = version;
    }

    #region Properties

    /// <summary>
    /// Gets or sets the message type.
    /// </summary>
    public string MessageType
    {
        get => WithReadLock(() => _messageType);
        set => WithWriteLock(() => _messageType = value);
    }

    /// <summary>
    /// Gets the source ID.
    /// </summary>
    public string SourceId => WithReadLock(() => _sourceId);

    /// <summary>
    /// Gets the source sub ID.
    /// </summary>
    public string SourceSubId => WithReadLock(() => _sourceSubId);

    /// <summary>
    /// Gets the target ID.
    /// </summary>
    public string TargetId => WithReadLock(() => _targetId);

    /// <summary>
    /// Gets the target sub ID.
    /// </summary>
    public string TargetSubId => WithReadLock(() => _targetSubId);

    /// <summary>
    /// Gets the version.
    /// </summary>
    public string Version => WithReadLock(() => _version);

    /// <summary>
    /// Gets all units/values in the container (read-only access).
    /// </summary>
    public IReadOnlyList<Value> Units => WithReadLock(() => _values.AsReadOnly());

    /// <summary>
    /// Gets the number of values in the container.
    /// Equivalent to C++ value_container::size().
    /// </summary>
    public int Count => WithReadLock(() => _values.Count);

    /// <summary>
    /// Returns whether container is empty.
    /// Equivalent to C++ value_container::empty().
    /// </summary>
    public bool Empty => Count == 0;

    #endregion

    #region Thread Safety Control

    /// <summary>
    /// Enables thread-safe mode.
    /// Equivalent to C++ conditional thread safety.
    /// </summary>
    public void EnableThreadSafety()
    {
        _threadSafeEnabled = true;
    }

    /// <summary>
    /// Disables thread-safe mode for better single-threaded performance.
    /// </summary>
    public void DisableThreadSafety()
    {
        _threadSafeEnabled = false;
    }

    /// <summary>
    /// Gets whether thread-safe mode is enabled.
    /// </summary>
    public bool IsThreadSafe => _threadSafeEnabled;

    #endregion

    #region Statistics (C++ compatible)

    /// <summary>
    /// Gets the read operation count.
    /// Equivalent to C++ statistics tracking.
    /// </summary>
    public long ReadCount => Interlocked.Read(ref _readCount);

    /// <summary>
    /// Gets the write operation count.
    /// </summary>
    public long WriteCount => Interlocked.Read(ref _writeCount);

    /// <summary>
    /// Gets the serialization operation count.
    /// </summary>
    public long SerializationCount => Interlocked.Read(ref _serializationCount);

    /// <summary>
    /// Resets all statistics counters.
    /// </summary>
    public void ResetStatistics()
    {
        Interlocked.Exchange(ref _readCount, 0);
        Interlocked.Exchange(ref _writeCount, 0);
        Interlocked.Exchange(ref _serializationCount, 0);
    }

    /// <summary>
    /// Gets memory statistics.
    /// Equivalent to C++ value_container::memory_stats().
    /// </summary>
    /// <returns>Tuple of (heapAllocations, stackAllocations) - in .NET all are heap</returns>
    public (long heapAllocations, long stackAllocations) MemoryStats()
    {
        // In .NET, all objects are heap allocated
        return (WriteCount, 0);
    }

    /// <summary>
    /// Estimates total memory footprint in bytes.
    /// Equivalent to C++ value_container::memory_footprint().
    /// </summary>
    public long MemoryFootprint()
    {
        return WithReadLock(() =>
        {
            long total = 0;

            // Estimate string sizes
            total += Encoding.UTF8.GetByteCount(_messageType);
            total += Encoding.UTF8.GetByteCount(_sourceId);
            total += Encoding.UTF8.GetByteCount(_sourceSubId);
            total += Encoding.UTF8.GetByteCount(_targetId);
            total += Encoding.UTF8.GetByteCount(_targetSubId);
            total += Encoding.UTF8.GetByteCount(_version);

            // Estimate value sizes
            foreach (var value in _values)
            {
                total += value.Size();
                total += Encoding.UTF8.GetByteCount(value.Name);
            }

            return total;
        });
    }

    #endregion

    #region Source/Target Management

    /// <summary>
    /// Sets the source IDs.
    /// </summary>
    public void SetSource(string sourceId, string sourceSubId = "")
    {
        WithWriteLock(() =>
        {
            _sourceId = sourceId;
            _sourceSubId = sourceSubId;
        });
    }

    /// <summary>
    /// Sets the target IDs.
    /// </summary>
    public void SetTarget(string targetId, string targetSubId = "")
    {
        WithWriteLock(() =>
        {
            _targetId = targetId;
            _targetSubId = targetSubId;
        });
    }

    /// <summary>
    /// Swaps source and target IDs.
    /// Equivalent to C++ value_container::swap_header().
    /// </summary>
    public void SwapHeader()
    {
        WithWriteLock(() =>
        {
            (_sourceId, _targetId) = (_targetId, _sourceId);
            (_sourceSubId, _targetSubId) = (_targetSubId, _sourceSubId);
        });
    }

    #endregion

    #region Value Management

    /// <summary>
    /// Adds a value to the container.
    /// </summary>
    public void Add(Value value)
    {
        WithWriteLock(() => _values.Add(value));
    }

    /// <summary>
    /// Sets a value by key, updating if exists or adding if new.
    /// Equivalent to C++ value_container::set_value().
    /// </summary>
    public void SetValue(string key, Value value)
    {
        WithWriteLock(() =>
        {
            value.Name = key;
            var index = _values.FindIndex(v => v.Name == key);
            if (index >= 0)
            {
                _values[index] = value;
            }
            else
            {
                _values.Add(value);
            }
        });
    }

    /// <summary>
    /// Gets a single value by name.
    /// Equivalent to C++ value_container::get_value().
    /// </summary>
    public Value? GetValue(string key)
    {
        return WithReadLock(() => _values.FirstOrDefault(v => v.Name == key));
    }

    /// <summary>
    /// Gets all values with the specified name.
    /// Equivalent to C++ value_container for multiple values.
    /// </summary>
    public List<Value> ValueArray(string key)
    {
        return WithReadLock(() => _values.Where(v => v.Name == key).ToList());
    }

    /// <summary>
    /// Gets all values in the container.
    /// </summary>
    public List<Value> Values()
    {
        return WithReadLock(() => new List<Value>(_values));
    }

    /// <summary>
    /// Removes a value by name.
    /// Equivalent to C++ value_container::remove().
    /// </summary>
    public bool Remove(string targetName)
    {
        return WithWriteLock(() =>
        {
            var index = _values.FindIndex(v => v.Name == targetName);
            if (index >= 0)
            {
                _values.RemoveAt(index);
                return true;
            }
            return false;
        });
    }

    /// <summary>
    /// Clears all stored child values.
    /// Equivalent to C++ value_container::clear_value().
    /// </summary>
    public void ClearValue()
    {
        WithWriteLock(() => _values.Clear());
    }

    /// <summary>
    /// Reinitializes the container to defaults.
    /// Equivalent to C++ value_container::initialize().
    /// </summary>
    public void Initialize()
    {
        WithWriteLock(() =>
        {
            _messageType = "data_container";
            _sourceId = string.Empty;
            _sourceSubId = string.Empty;
            _targetId = string.Empty;
            _targetSubId = string.Empty;
            _version = "1.0.0.0";
            _values.Clear();
        });
    }

    /// <summary>
    /// Creates a copy of this container.
    /// Equivalent to C++ value_container::copy().
    /// </summary>
    /// <param name="containingValues">If false, only copy header</param>
    public ValueContainer Copy(bool containingValues = true)
    {
        return WithReadLock(() =>
        {
            var copy = new ValueContainer(
                _sourceId, _sourceSubId,
                _targetId, _targetSubId,
                _messageType, _version);

            if (containingValues)
            {
                foreach (var value in _values)
                {
                    copy.Add(value);
                }
            }

            return copy;
        });
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serializes the container to a JSON string.
    /// </summary>
    public string Serialize() => ToJson();

    /// <summary>
    /// Converts the container to JSON format.
    /// </summary>
    public string ToJson()
    {
        Interlocked.Increment(ref _serializationCount);

        return WithReadLock(() =>
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
        });
    }

    /// <summary>
    /// Serializes to byte array.
    /// Equivalent to C++ value_container::serialize_array().
    /// </summary>
    public byte[] SerializeArray()
    {
        return Encoding.UTF8.GetBytes(Serialize());
    }

    /// <summary>
    /// Deserializes JSON string into this container.
    /// Equivalent to C++ value_container::deserialize().
    /// </summary>
    public bool Deserialize(string dataString)
    {
        try
        {
            using var doc = JsonDocument.Parse(dataString);
            var root = doc.RootElement;

            WithWriteLock(() =>
            {
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
                if (root.TryGetProperty("values", out var valuesArray) &&
                    valuesArray.ValueKind == JsonValueKind.Array)
                {
                    _values.Clear();
                    foreach (var valueElement in valuesArray.EnumerateArray())
                    {
                        var value = ParseValueFromJson(valueElement);
                        if (value != null)
                            _values.Add(value);
                    }
                }
            });

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Deserializes from byte array.
    /// </summary>
    public bool Deserialize(byte[] dataArray)
    {
        return Deserialize(Encoding.UTF8.GetString(dataArray));
    }

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
            if (element.TryGetProperty("children", out var childrenArray) &&
                childrenArray.ValueKind == JsonValueKind.Array)
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
    public string ToXml()
    {
        Interlocked.Increment(ref _serializationCount);

        return WithReadLock(() =>
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
        });
    }

    #endregion

    #region IEnumerable<Value> Implementation

    /// <summary>
    /// Returns an enumerator for iterating over values.
    /// Enables range-based for loops like C++ iterators.
    /// </summary>
    public IEnumerator<Value> GetEnumerator()
    {
        // Take a snapshot for thread-safe iteration
        List<Value> snapshot;
        if (_threadSafeEnabled)
        {
            _rwLock.EnterReadLock();
            try
            {
                snapshot = new List<Value>(_values);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }
        else
        {
            snapshot = new List<Value>(_values);
        }

        return snapshot.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    #region Lock Helpers

    private T WithReadLock<T>(Func<T> action)
    {
        if (_threadSafeEnabled)
        {
            _rwLock.EnterReadLock();
            try
            {
                Interlocked.Increment(ref _readCount);
                return action();
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }
        else
        {
            Interlocked.Increment(ref _readCount);
            return action();
        }
    }

    private void WithWriteLock(Action action)
    {
        if (_threadSafeEnabled)
        {
            _rwLock.EnterWriteLock();
            try
            {
                Interlocked.Increment(ref _writeCount);
                action();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }
        else
        {
            Interlocked.Increment(ref _writeCount);
            action();
        }
    }

    private T WithWriteLock<T>(Func<T> action)
    {
        if (_threadSafeEnabled)
        {
            _rwLock.EnterWriteLock();
            try
            {
                Interlocked.Increment(ref _writeCount);
                return action();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }
        else
        {
            Interlocked.Increment(ref _writeCount);
            return action();
        }
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _rwLock.Dispose();
            }
            _disposed = true;
        }
    }

    ~ValueContainer()
    {
        Dispose(false);
    }

    #endregion
}
