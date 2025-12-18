/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

namespace ContainerSystem.Core;

/// <summary>
/// High-performance key-value store for Value objects with optional thread safety.
/// Equivalent to C++ value_store class.
///
/// Features:
/// - O(1) average lookup via Dictionary
/// - Multiple values per key support (Dictionary-of-Lists structure)
/// - Optional thread-safe mode with ReaderWriterLockSlim
/// - Read/write statistics tracking
/// - Conditional locking for single-threaded performance
/// </summary>
public class ValueStore : IDisposable
{
    private readonly Dictionary<string, List<Value>> _values;
    private readonly ReaderWriterLockSlim _rwLock;
    private volatile bool _threadSafeEnabled;
    private long _readCount;
    private long _writeCount;
    private bool _disposed;

    /// <summary>
    /// Creates a new ValueStore instance.
    /// </summary>
    /// <param name="threadSafe">If true, enables thread-safe mode from start</param>
    /// <param name="initialCapacity">Initial capacity for internal dictionary</param>
    public ValueStore(bool threadSafe = false, int initialCapacity = 16)
    {
        _values = new Dictionary<string, List<Value>>(initialCapacity);
        _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        _threadSafeEnabled = threadSafe;
        _readCount = 0;
        _writeCount = 0;
        _disposed = false;
    }

    /// <summary>
    /// Adds a value to the store. Multiple values can be stored under the same key.
    /// Equivalent to C++ value_store::add().
    /// </summary>
    /// <param name="key">The key/name for the value</param>
    /// <param name="value">The value to store</param>
    public void Add(string key, Value value)
    {
        if (_threadSafeEnabled)
        {
            _rwLock.EnterWriteLock();
            try
            {
                AddInternal(key, value);
                Interlocked.Increment(ref _writeCount);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }
        else
        {
            AddInternal(key, value);
            Interlocked.Increment(ref _writeCount);
        }
    }

    private void AddInternal(string key, Value value)
    {
        if (_values.TryGetValue(key, out var list))
        {
            list.Add(value);
        }
        else
        {
            _values[key] = new List<Value> { value };
        }
    }

    /// <summary>
    /// Sets a single value for the key, replacing all existing values.
    /// Useful when single-value semantics are required.
    /// </summary>
    /// <param name="key">The key/name for the value</param>
    /// <param name="value">The value to store</param>
    public void Set(string key, Value value)
    {
        if (_threadSafeEnabled)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _values[key] = new List<Value> { value };
                Interlocked.Increment(ref _writeCount);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }
        else
        {
            _values[key] = new List<Value> { value };
            Interlocked.Increment(ref _writeCount);
        }
    }

    /// <summary>
    /// Gets the first value by key for API compatibility.
    /// Equivalent to C++ value_store::get().
    /// </summary>
    /// <param name="key">The key to search for</param>
    /// <returns>The first value if found, null otherwise</returns>
    public Value? Get(string key)
    {
        if (_threadSafeEnabled)
        {
            _rwLock.EnterReadLock();
            try
            {
                if (_values.TryGetValue(key, out var list) && list.Count > 0)
                {
                    Interlocked.Increment(ref _readCount);
                    return list[0];
                }
                return null;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }
        else
        {
            if (_values.TryGetValue(key, out var list) && list.Count > 0)
            {
                Interlocked.Increment(ref _readCount);
                return list[0];
            }
            return null;
        }
    }

    /// <summary>
    /// Gets all values associated with a key.
    /// Use this method when multiple values per key are expected.
    /// </summary>
    /// <param name="key">The key to search for</param>
    /// <returns>Read-only list of values, empty if key not found</returns>
    public IReadOnlyList<Value> GetValues(string key)
    {
        if (_threadSafeEnabled)
        {
            _rwLock.EnterReadLock();
            try
            {
                if (_values.TryGetValue(key, out var list))
                {
                    Interlocked.Increment(ref _readCount);
                    return list.AsReadOnly();
                }
                return Array.Empty<Value>();
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }
        else
        {
            if (_values.TryGetValue(key, out var list))
            {
                Interlocked.Increment(ref _readCount);
                return list.AsReadOnly();
            }
            return Array.Empty<Value>();
        }
    }

    /// <summary>
    /// Gets the count of values for a specific key.
    /// </summary>
    /// <param name="key">The key to count values for</param>
    /// <returns>Number of values for the key, 0 if key not found</returns>
    public int GetValueCount(string key)
    {
        if (_threadSafeEnabled)
        {
            _rwLock.EnterReadLock();
            try
            {
                return _values.TryGetValue(key, out var list) ? list.Count : 0;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }
        return _values.TryGetValue(key, out var list) ? list.Count : 0;
    }

    /// <summary>
    /// Checks if a key exists in the store.
    /// Equivalent to C++ value_store::contains().
    /// </summary>
    /// <param name="key">The key to check</param>
    /// <returns>True if key exists</returns>
    public bool Contains(string key)
    {
        if (_threadSafeEnabled)
        {
            _rwLock.EnterReadLock();
            try
            {
                return _values.ContainsKey(key);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }
        return _values.ContainsKey(key);
    }

    /// <summary>
    /// Removes all values for a key.
    /// Equivalent to C++ value_store::remove().
    /// </summary>
    /// <param name="key">The key to remove</param>
    /// <returns>True if removed, false if not found</returns>
    public bool Remove(string key)
    {
        if (_threadSafeEnabled)
        {
            _rwLock.EnterWriteLock();
            try
            {
                return _values.Remove(key);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }
        return _values.Remove(key);
    }

    /// <summary>
    /// Removes a specific value from a key's value list.
    /// </summary>
    /// <param name="key">The key to remove from</param>
    /// <param name="value">The specific value to remove</param>
    /// <returns>True if the value was removed</returns>
    public bool RemoveValue(string key, Value value)
    {
        if (_threadSafeEnabled)
        {
            _rwLock.EnterWriteLock();
            try
            {
                return RemoveValueInternal(key, value);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }
        return RemoveValueInternal(key, value);
    }

    private bool RemoveValueInternal(string key, Value value)
    {
        if (!_values.TryGetValue(key, out var list))
        {
            return false;
        }

        var removed = list.Remove(value);

        // Clean up empty list
        if (list.Count == 0)
        {
            _values.Remove(key);
        }

        return removed;
    }

    /// <summary>
    /// Clears all values from the store.
    /// Equivalent to C++ value_store::clear().
    /// </summary>
    public void Clear()
    {
        if (_threadSafeEnabled)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _values.Clear();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }
        else
        {
            _values.Clear();
        }
    }

    /// <summary>
    /// Gets the number of unique keys in the store.
    /// Equivalent to C++ value_store::size().
    /// </summary>
    public int Size
    {
        get
        {
            if (_threadSafeEnabled)
            {
                _rwLock.EnterReadLock();
                try
                {
                    return _values.Count;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            return _values.Count;
        }
    }

    /// <summary>
    /// Gets the total number of values across all keys.
    /// </summary>
    public int TotalValueCount
    {
        get
        {
            if (_threadSafeEnabled)
            {
                _rwLock.EnterReadLock();
                try
                {
                    return _values.Values.Sum(list => list.Count);
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            return _values.Values.Sum(list => list.Count);
        }
    }

    /// <summary>
    /// Checks if the store is empty.
    /// Equivalent to C++ value_store::empty().
    /// </summary>
    public bool Empty => Size == 0;

    /// <summary>
    /// Enables thread-safe mode.
    /// Equivalent to C++ value_store::enable_thread_safety().
    /// </summary>
    public void EnableThreadSafety()
    {
        _threadSafeEnabled = true;
    }

    /// <summary>
    /// Disables thread-safe mode.
    /// Equivalent to C++ value_store::disable_thread_safety().
    /// </summary>
    public void DisableThreadSafety()
    {
        _threadSafeEnabled = false;
    }

    /// <summary>
    /// Gets whether thread-safe mode is enabled.
    /// Equivalent to C++ value_store::is_thread_safe().
    /// </summary>
    public bool IsThreadSafe => _threadSafeEnabled;

    /// <summary>
    /// Gets the read operation count.
    /// Equivalent to C++ value_store::get_read_count().
    /// </summary>
    public long ReadCount => Interlocked.Read(ref _readCount);

    /// <summary>
    /// Gets the write operation count.
    /// Equivalent to C++ value_store::get_write_count().
    /// </summary>
    public long WriteCount => Interlocked.Read(ref _writeCount);

    /// <summary>
    /// Resets read/write statistics.
    /// Equivalent to C++ value_store::reset_statistics().
    /// </summary>
    public void ResetStatistics()
    {
        Interlocked.Exchange(ref _readCount, 0);
        Interlocked.Exchange(ref _writeCount, 0);
    }

    /// <summary>
    /// Gets all keys in the store.
    /// </summary>
    public IEnumerable<string> Keys
    {
        get
        {
            if (_threadSafeEnabled)
            {
                _rwLock.EnterReadLock();
                try
                {
                    return _values.Keys.ToList();
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            return _values.Keys.ToList();
        }
    }

    /// <summary>
    /// Gets all values in the store (flattened from all keys).
    /// </summary>
    public IEnumerable<Value> Values
    {
        get
        {
            if (_threadSafeEnabled)
            {
                _rwLock.EnterReadLock();
                try
                {
                    return _values.Values.SelectMany(list => list).ToList();
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            return _values.Values.SelectMany(list => list).ToList();
        }
    }

    /// <summary>
    /// Indexer for convenient access.
    /// Setting a value replaces all existing values for the key (single-value semantics).
    /// </summary>
    public Value? this[string key]
    {
        get => Get(key);
        set
        {
            if (value != null)
                Set(key, value);
            else
                Remove(key);
        }
    }

    /// <summary>
    /// Disposes resources.
    /// </summary>
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

    ~ValueStore()
    {
        Dispose(false);
    }
}
