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
/// Tests for enhanced ValueContainer features that match C++ container_system.
///
/// Tests:
/// - IEnumerable&lt;Value&gt; iteration support
/// - Conditional thread safety with ReaderWriterLockSlim
/// - Memory footprint tracking
/// - Read/write statistics
/// - C++ compatible API methods
/// </summary>
public class ValueContainerEnhancedTests : IDisposable
{
    private readonly ValueContainer _container;

    public ValueContainerEnhancedTests()
    {
        _container = new ValueContainer();
    }

    public void Dispose()
    {
        _container.Dispose();
    }

    // ========================================================================
    // IEnumerable<Value> Tests
    // ========================================================================

    [Fact]
    public void Container_SupportsForEachIteration()
    {
        _container.Add(new StringValue("name1", "value1"));
        _container.Add(new IntValue("name2", 42));
        _container.Add(new BoolValue("name3", true));

        var count = 0;
        foreach (var value in _container)
        {
            count++;
            Assert.NotNull(value);
        }

        Assert.Equal(3, count);
    }

    [Fact]
    public void Container_SupportsLinq()
    {
        _container.Add(new IntValue("a", 1));
        _container.Add(new IntValue("b", 2));
        _container.Add(new IntValue("c", 3));

        // LINQ Where
        var filtered = _container.Where(v => v.ToInt() > 1).ToList();
        Assert.Equal(2, filtered.Count);

        // LINQ Select
        var names = _container.Select(v => v.Name).ToList();
        Assert.Contains("a", names);
        Assert.Contains("b", names);
        Assert.Contains("c", names);

        // LINQ First
        var first = _container.First();
        Assert.Equal("a", first.Name);
    }

    [Fact]
    public void Container_IterationIsSnapshot()
    {
        _container.Add(new IntValue("initial", 1));

        // Start iteration
        var enumerator = _container.GetEnumerator();
        enumerator.MoveNext();

        // Modify container during iteration - should not affect snapshot
        _container.Add(new IntValue("new", 2));

        // Original iteration should still work
        Assert.Equal("initial", enumerator.Current.Name);
    }

    // ========================================================================
    // Thread Safety Tests
    // ========================================================================

    [Fact]
    public void Container_ThreadSafetyDisabledByDefault()
    {
        Assert.False(_container.IsThreadSafe);
    }

    [Fact]
    public void Container_CanEnableThreadSafety()
    {
        _container.EnableThreadSafety();
        Assert.True(_container.IsThreadSafe);
    }

    [Fact]
    public void Container_CanDisableThreadSafety()
    {
        _container.EnableThreadSafety();
        _container.DisableThreadSafety();
        Assert.False(_container.IsThreadSafe);
    }

    [Fact]
    public async Task Container_ThreadSafeAccessFromMultipleThreads()
    {
        _container.EnableThreadSafety();

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
                        _ = _container.Count;
                        _ = _container.MessageType;
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
                        _container.Add(new IntValue($"key_{index}_{j}", j));
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
    public void Container_TracksReadCount()
    {
        var initialCount = _container.ReadCount;

        _ = _container.Count;
        _ = _container.MessageType;
        _ = _container.SourceId;

        Assert.True(_container.ReadCount > initialCount);
    }

    [Fact]
    public void Container_TracksWriteCount()
    {
        var initialCount = _container.WriteCount;

        _container.Add(new IntValue("test", 1));
        _container.SetSource("src", "sub");

        Assert.True(_container.WriteCount > initialCount);
    }

    [Fact]
    public void Container_TracksSerializationCount()
    {
        var initialCount = _container.SerializationCount;

        _ = _container.ToJson();
        _ = _container.ToXml();

        Assert.Equal(initialCount + 2, _container.SerializationCount);
    }

    [Fact]
    public void Container_CanResetStatistics()
    {
        _container.Add(new IntValue("test", 1));
        _ = _container.Count;
        _ = _container.ToJson();

        _container.ResetStatistics();

        Assert.Equal(0, _container.ReadCount);
        Assert.Equal(0, _container.WriteCount);
        Assert.Equal(0, _container.SerializationCount);
    }

    // ========================================================================
    // Memory Tracking Tests
    // ========================================================================

    [Fact]
    public void Container_ReturnsMemoryStats()
    {
        _container.Add(new IntValue("test", 1));

        var (heap, stack) = _container.MemoryStats();

        // In .NET, all allocations are heap
        Assert.True(heap >= 0);
        Assert.Equal(0, stack);
    }

    [Fact]
    public void Container_CalculatesMemoryFootprint()
    {
        var emptyFootprint = _container.MemoryFootprint();

        _container.Add(new StringValue("longString", new string('x', 1000)));
        _container.Add(new BytesValue("bytes", new byte[500]));

        var largerFootprint = _container.MemoryFootprint();

        Assert.True(largerFootprint > emptyFootprint);
    }

    // ========================================================================
    // C++ Compatible API Tests
    // ========================================================================

    [Fact]
    public void Container_SetValueUpdatesExistingKey()
    {
        _container.Add(new IntValue("key", 1));
        _container.SetValue("key", new IntValue("key", 2));

        Assert.Equal(1, _container.Count);
        Assert.Equal(2, _container.GetValue("key")?.ToInt());
    }

    [Fact]
    public void Container_SetValueAddsNewKey()
    {
        _container.SetValue("new_key", new StringValue("new_key", "value"));

        Assert.Equal(1, _container.Count);
        Assert.Equal("value", _container.GetValue("new_key")?.ToString());
    }

    [Fact]
    public void Container_SizeProperty()
    {
        Assert.Equal(0, _container.Count);
        Assert.True(_container.Empty);

        _container.Add(new IntValue("a", 1));
        _container.Add(new IntValue("b", 2));

        Assert.Equal(2, _container.Count);
        Assert.False(_container.Empty);
    }

    [Fact]
    public void Container_SwapHeader()
    {
        _container.SetSource("source1", "sub1");
        _container.SetTarget("target1", "sub2");

        _container.SwapHeader();

        Assert.Equal("target1", _container.SourceId);
        Assert.Equal("sub2", _container.SourceSubId);
        Assert.Equal("source1", _container.TargetId);
        Assert.Equal("sub1", _container.TargetSubId);
    }

    [Fact]
    public void Container_ClearValue()
    {
        _container.Add(new IntValue("a", 1));
        _container.Add(new IntValue("b", 2));

        _container.ClearValue();

        Assert.Equal(0, _container.Count);
    }

    [Fact]
    public void Container_Initialize()
    {
        _container.SetSource("src", "sub");
        _container.MessageType = "custom_type";
        _container.Add(new IntValue("test", 1));

        _container.Initialize();

        Assert.Equal("data_container", _container.MessageType);
        Assert.Equal(string.Empty, _container.SourceId);
        Assert.Equal(0, _container.Count);
    }

    [Fact]
    public void Container_Copy()
    {
        _container.SetSource("src", "sub");
        _container.Add(new IntValue("a", 1));
        _container.Add(new IntValue("b", 2));

        var fullCopy = _container.Copy(containingValues: true);
        var headerOnly = _container.Copy(containingValues: false);

        Assert.Equal("src", fullCopy.SourceId);
        Assert.Equal(2, fullCopy.Count);

        Assert.Equal("src", headerOnly.SourceId);
        Assert.Equal(0, headerOnly.Count);

        fullCopy.Dispose();
        headerOnly.Dispose();
    }

    [Fact]
    public void Container_Remove()
    {
        _container.Add(new IntValue("a", 1));
        _container.Add(new IntValue("b", 2));

        var removed = _container.Remove("a");

        Assert.True(removed);
        Assert.Equal(1, _container.Count);
        Assert.Null(_container.GetValue("a"));
    }

    [Fact]
    public void Container_RemoveNonExistent()
    {
        var removed = _container.Remove("nonexistent");
        Assert.False(removed);
    }

    // ========================================================================
    // Serialization Tests
    // ========================================================================

    [Fact]
    public void Container_SerializeArray()
    {
        _container.Add(new StringValue("test", "value"));

        var bytes = _container.SerializeArray();

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);

        // Should be valid UTF-8 JSON
        var json = System.Text.Encoding.UTF8.GetString(bytes);
        Assert.Contains("test", json);
    }

    [Fact]
    public void Container_DeserializeFromByteArray()
    {
        var json = "{\"message_type\":\"test_type\",\"version\":\"1.0.0.0\",\"source_id\":\"\",\"source_sub_id\":\"\",\"target_id\":\"\",\"target_sub_id\":\"\",\"values\":[]}";
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        using var container = new ValueContainer();
        var success = container.Deserialize(bytes);

        Assert.True(success);
        Assert.Equal("test_type", container.MessageType);
    }
}
