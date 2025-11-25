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
/// - Optional thread-safe mode with ReaderWriterLockSlim
/// - Read/write statistics tracking
/// - Conditional locking for single-threaded performance
/// </summary>
public class ValueStore : IDisposable
{
    private readonly Dictionary<string, Value> _values;
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
        _values = new Dictionary<string, Value>(initialCapacity);
        _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        _threadSafeEnabled = threadSafe;
        _readCount = 0;
        _writeCount = 0;
        _disposed = false;
    }

    /// <summary>
    /// Adds or updates a value in the store.
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
                _values[key] = value;
                Interlocked.Increment(ref _writeCount);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }
        else
        {
            _values[key] = value;
            Interlocked.Increment(ref _writeCount);
        }
    }

    /// <summary>
    /// Gets a value by key.
    /// Equivalent to C++ value_store::get().
    /// </summary>
    /// <param name="key">The key to search for</param>
    /// <returns>The value if found, null otherwise</returns>
    public Value? Get(string key)
    {
        if (_threadSafeEnabled)
        {
            _rwLock.EnterReadLock();
            try
            {
                if (_values.TryGetValue(key, out var value))
                {
                    Interlocked.Increment(ref _readCount);
                    return value;
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
            if (_values.TryGetValue(key, out var value))
            {
                Interlocked.Increment(ref _readCount);
                return value;
            }
            return null;
        }
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
    /// Removes a value by key.
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
    /// Gets the number of values in the store.
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
    /// Gets all values in the store.
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
                    return _values.Values.ToList();
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            return _values.Values.ToList();
        }
    }

    /// <summary>
    /// Indexer for convenient access.
    /// </summary>
    public Value? this[string key]
    {
        get => Get(key);
        set
        {
            if (value != null)
                Add(key, value);
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
