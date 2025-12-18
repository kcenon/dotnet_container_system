/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using ContainerSystem.Core;
using ContainerSystem.Values;
using Xunit;

namespace ContainerSystem.Tests;

/// <summary>
/// Tests for ValueStore class - equivalent to C++ value_store.
///
/// Tests:
/// - Basic CRUD operations
/// - Thread safety functionality
/// - Statistics tracking
/// - Indexer access
/// </summary>
public class ValueStoreTests : IDisposable
{
    private readonly ValueStore _store;

    public ValueStoreTests()
    {
        _store = new ValueStore();
    }

    public void Dispose()
    {
        _store.Dispose();
    }

    // ========================================================================
    // Basic Operations Tests
    // ========================================================================

    [Fact]
    public void Store_AddAndGet()
    {
        _store.Add("key1", new IntValue("key1", 42));

        var value = _store.Get("key1");

        Assert.NotNull(value);
        Assert.Equal(42, value.ToInt());
    }

    [Fact]
    public void Store_GetNonExistentReturnsNull()
    {
        var value = _store.Get("nonexistent");
        Assert.Null(value);
    }

    [Fact]
    public void Store_Contains()
    {
        _store.Add("existing", new StringValue("existing", "test"));

        Assert.True(_store.Contains("existing"));
        Assert.False(_store.Contains("nonexistent"));
    }

    [Fact]
    public void Store_Remove()
    {
        _store.Add("key", new IntValue("key", 1));

        var removed = _store.Remove("key");

        Assert.True(removed);
        Assert.False(_store.Contains("key"));
    }

    [Fact]
    public void Store_RemoveNonExistent()
    {
        var removed = _store.Remove("nonexistent");
        Assert.False(removed);
    }

    [Fact]
    public void Store_Clear()
    {
        _store.Add("a", new IntValue("a", 1));
        _store.Add("b", new IntValue("b", 2));
        _store.Add("c", new IntValue("c", 3));

        _store.Clear();

        Assert.Equal(0, _store.Size);
        Assert.True(_store.Empty);
    }

    [Fact]
    public void Store_SizeAndEmpty()
    {
        Assert.Equal(0, _store.Size);
        Assert.True(_store.Empty);

        _store.Add("key", new IntValue("key", 1));

        Assert.Equal(1, _store.Size);
        Assert.False(_store.Empty);
    }

    // ========================================================================
    // Multi-Value Support Tests
    // ========================================================================

    [Fact]
    public void Store_AddAppendsToExistingKey()
    {
        _store.Add("key", new IntValue("key", 1));
        _store.Add("key", new IntValue("key", 2));

        Assert.Equal(1, _store.Size);
        Assert.Equal(2, _store.TotalValueCount);
        Assert.Equal(1, _store.Get("key")?.ToInt()); // Get returns first value
    }

    [Fact]
    public void Store_GetValuesReturnsAllValuesForKey()
    {
        _store.Add("tag", new IntValue("tag", 1));
        _store.Add("tag", new IntValue("tag", 2));
        _store.Add("tag", new IntValue("tag", 3));

        var values = _store.GetValues("tag");

        Assert.Equal(3, values.Count);
        Assert.Equal(1, values[0].ToInt());
        Assert.Equal(2, values[1].ToInt());
        Assert.Equal(3, values[2].ToInt());
    }

    [Fact]
    public void Store_GetValuesReturnsEmptyForNonExistent()
    {
        var values = _store.GetValues("nonexistent");
        Assert.Empty(values);
    }

    [Fact]
    public void Store_GetValueCountReturnsCorrectCount()
    {
        _store.Add("key", new IntValue("key", 1));
        _store.Add("key", new IntValue("key", 2));

        Assert.Equal(2, _store.GetValueCount("key"));
        Assert.Equal(0, _store.GetValueCount("nonexistent"));
    }

    [Fact]
    public void Store_SetReplacesAllValues()
    {
        _store.Add("key", new IntValue("key", 1));
        _store.Add("key", new IntValue("key", 2));
        _store.Set("key", new IntValue("key", 99));

        Assert.Equal(1, _store.GetValueCount("key"));
        Assert.Equal(99, _store.Get("key")?.ToInt());
    }

    [Fact]
    public void Store_TotalValueCountAcrossAllKeys()
    {
        _store.Add("a", new IntValue("a", 1));
        _store.Add("a", new IntValue("a", 2));
        _store.Add("b", new IntValue("b", 3));

        Assert.Equal(2, _store.Size);
        Assert.Equal(3, _store.TotalValueCount);
    }

    [Fact]
    public void Store_RemoveValueRemovesSpecificValue()
    {
        var val1 = new IntValue("key", 1);
        var val2 = new IntValue("key", 2);
        _store.Add("key", val1);
        _store.Add("key", val2);

        var removed = _store.RemoveValue("key", val1);

        Assert.True(removed);
        Assert.Equal(1, _store.GetValueCount("key"));
        Assert.Equal(2, _store.Get("key")?.ToInt());
    }

    [Fact]
    public void Store_RemoveValueCleansUpEmptyList()
    {
        var val1 = new IntValue("key", 1);
        _store.Add("key", val1);

        _store.RemoveValue("key", val1);

        Assert.False(_store.Contains("key"));
        Assert.Equal(0, _store.Size);
    }

    [Fact]
    public void Store_IndexerSetUsesSingleValueSemantics()
    {
        _store.Add("key", new IntValue("key", 1));
        _store.Add("key", new IntValue("key", 2));

        _store["key"] = new IntValue("key", 99);

        Assert.Equal(1, _store.GetValueCount("key"));
        Assert.Equal(99, _store.Get("key")?.ToInt());
    }

    [Fact]
    public void Store_ValuesReturnsAllValuesFlattenedForMultipleKeys()
    {
        _store.Add("a", new IntValue("a", 1));
        _store.Add("a", new IntValue("a", 2));
        _store.Add("b", new IntValue("b", 3));

        var values = _store.Values.ToList();

        Assert.Equal(3, values.Count);
    }

    // ========================================================================
    // Thread Safety Tests
    // ========================================================================

    [Fact]
    public void Store_ThreadSafetyDisabledByDefault()
    {
        Assert.False(_store.IsThreadSafe);
    }

    [Fact]
    public void Store_CanEnableThreadSafety()
    {
        _store.EnableThreadSafety();
        Assert.True(_store.IsThreadSafe);
    }

    [Fact]
    public void Store_CanDisableThreadSafety()
    {
        _store.EnableThreadSafety();
        _store.DisableThreadSafety();
        Assert.False(_store.IsThreadSafe);
    }

    [Fact]
    public async Task Store_ThreadSafeAccessFromMultipleThreads()
    {
        _store.EnableThreadSafety();

        var tasks = new List<Task>();
        var exceptions = new List<Exception>();

        // Multiple readers
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < 100; j++)
                    {
                        _ = _store.Size;
                        _ = _store.Get("test");
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions) { exceptions.Add(ex); }
                }
            }));
        }

        // Multiple writers
        for (int i = 0; i < 5; i++)
        {
            var index = i;
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < 50; j++)
                    {
                        _store.Add($"key_{index}_{j}", new IntValue($"key_{index}_{j}", j));
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions) { exceptions.Add(ex); }
                }
            }));
        }

        await Task.WhenAll(tasks);
        Assert.Empty(exceptions);
    }

    // ========================================================================
    // Statistics Tests
    // ========================================================================

    [Fact]
    public void Store_TracksReadCount()
    {
        _store.Add("key", new IntValue("key", 1));

        var initialCount = _store.ReadCount;

        _ = _store.Get("key");
        _ = _store.Get("key");
        _ = _store.Get("key");

        Assert.Equal(initialCount + 3, _store.ReadCount);
    }

    [Fact]
    public void Store_TracksWriteCount()
    {
        var initialCount = _store.WriteCount;

        _store.Add("a", new IntValue("a", 1));
        _store.Add("b", new IntValue("b", 2));

        Assert.Equal(initialCount + 2, _store.WriteCount);
    }

    [Fact]
    public void Store_CanResetStatistics()
    {
        _store.Add("key", new IntValue("key", 1));
        _ = _store.Get("key");

        _store.ResetStatistics();

        Assert.Equal(0, _store.ReadCount);
        Assert.Equal(0, _store.WriteCount);
    }

    // ========================================================================
    // Indexer Tests
    // ========================================================================

    [Fact]
    public void Store_IndexerGet()
    {
        _store.Add("key", new IntValue("key", 42));

        var value = _store["key"];

        Assert.NotNull(value);
        Assert.Equal(42, value.ToInt());
    }

    [Fact]
    public void Store_IndexerGetNonExistent()
    {
        var value = _store["nonexistent"];
        Assert.Null(value);
    }

    [Fact]
    public void Store_IndexerSet()
    {
        _store["key"] = new StringValue("key", "value");

        Assert.True(_store.Contains("key"));
        Assert.Equal("value", _store["key"]?.ToString());
    }

    [Fact]
    public void Store_IndexerSetNullRemoves()
    {
        _store.Add("key", new IntValue("key", 1));

        _store["key"] = null;

        Assert.False(_store.Contains("key"));
    }

    // ========================================================================
    // Keys and Values Tests
    // ========================================================================

    [Fact]
    public void Store_Keys()
    {
        _store.Add("a", new IntValue("a", 1));
        _store.Add("b", new IntValue("b", 2));
        _store.Add("c", new IntValue("c", 3));

        var keys = _store.Keys.ToList();

        Assert.Equal(3, keys.Count);
        Assert.Contains("a", keys);
        Assert.Contains("b", keys);
        Assert.Contains("c", keys);
    }

    [Fact]
    public void Store_Values()
    {
        _store.Add("a", new IntValue("a", 1));
        _store.Add("b", new IntValue("b", 2));

        var values = _store.Values.ToList();

        Assert.Equal(2, values.Count);
    }

    // ========================================================================
    // Constructor Tests
    // ========================================================================

    [Fact]
    public void Store_ConstructWithThreadSafety()
    {
        using var store = new ValueStore(threadSafe: true);
        Assert.True(store.IsThreadSafe);
    }

    [Fact]
    public void Store_ConstructWithInitialCapacity()
    {
        using var store = new ValueStore(threadSafe: false, initialCapacity: 100);
        Assert.Equal(0, store.Size); // Capacity doesn't affect size
    }
}
