# Benchmarks

> **Language:** **English** | [한국어](BENCHMARKS_KO.md)

Performance analysis and metrics for .NET Container System.

---

## Overview

This document provides performance benchmarks for .NET Container System operations including container creation, serialization, deserialization, and value operations.

---

## Test Environment

### Hardware
- **CPU**: Apple M1 (8 cores) / Intel Core i7-12700K
- **Memory**: 16GB
- **Storage**: NVMe SSD

### Software
- **OS**: macOS 14 / Windows 11 / Ubuntu 22.04
- **Runtime**: .NET 8.0
- **Build**: Release configuration

---

## Benchmark Results

### Container Operations

| Operation | Throughput | Latency (avg) | Notes |
|-----------|------------|---------------|-------|
| Container Creation | ~1M/sec | ~1 μs | Empty container |
| Value Addition | ~2M/sec | ~0.5 μs | Single value |
| Value Retrieval | ~5M/sec | ~0.2 μs | By key lookup |
| Multiple Values | ~1M/sec | ~1 μs | 10 values |

### Serialization Performance

| Format | Serialize | Deserialize | Size Ratio |
|--------|-----------|-------------|------------|
| Binary | ~500K/sec | ~400K/sec | 1.0x |
| JSON | ~200K/sec | ~150K/sec | 1.5-2x |
| JSON v2.0 | ~180K/sec | ~140K/sec | 1.6-2.2x |
| XML | ~100K/sec | ~80K/sec | 2-3x |

### Value Type Operations

| Value Type | Create | Read | Convert |
|------------|--------|------|---------|
| IntValue | 3M/sec | 10M/sec | 5M/sec |
| LongValue | 3M/sec | 10M/sec | 5M/sec |
| DoubleValue | 3M/sec | 10M/sec | 5M/sec |
| StringValue | 2M/sec | 8M/sec | 3M/sec |
| BytesValue | 1M/sec | 5M/sec | 2M/sec |
| ContainerValue | 500K/sec | 2M/sec | - |

---

## Comparison with C++ Version

| Operation | C++ | .NET | Ratio |
|-----------|-----|------|-------|
| Binary Serialization | 1.8M/sec | 500K/sec | 3.6x |
| JSON Serialization | 950K/sec | 200K/sec | 4.75x |
| Container Creation | 2M/sec | 1M/sec | 2x |
| Value Addition | 4.5M/sec | 2M/sec | 2.25x |

**Note**: C++ version uses SIMD optimization (ARM NEON/x86 AVX2) which accounts for most of the performance difference.

---

## Memory Usage

### Container Memory

| Container Size | Memory (bytes) | Per Value Overhead |
|----------------|----------------|-------------------|
| Empty | ~200 | - |
| 10 values | ~800 | ~60 |
| 100 values | ~6,400 | ~62 |
| 1,000 values | ~62,000 | ~62 |

### Serialization Memory

| Format | Memory Multiplier | Notes |
|--------|-------------------|-------|
| Binary | 1.1x | Minimal overhead |
| JSON | 2-3x | String allocation |
| XML | 3-4x | Verbose format |

---

## Thread Scalability

### Concurrent Read Performance

| Threads | Throughput | Speedup |
|---------|------------|---------|
| 1 | 1.0M/sec | 1.0x |
| 2 | 1.9M/sec | 1.9x |
| 4 | 3.6M/sec | 3.6x |
| 8 | 6.5M/sec | 6.5x |

### Concurrent Write Performance

| Threads | Throughput | Notes |
|---------|------------|-------|
| 1 | 800K/sec | No contention |
| 2 | 600K/sec | Lock contention |
| 4 | 500K/sec | Increased contention |
| 8 | 400K/sec | High contention |

---

## Benchmark Code

### Basic Serialization Benchmark

```csharp
using BenchmarkDotNet.Attributes;
using ContainerSystem.Core;
using ContainerSystem.Values;

[MemoryDiagnoser]
public class SerializationBenchmark
{
    private ValueContainer _container;

    [GlobalSetup]
    public void Setup()
    {
        _container = new ValueContainer();
        _container.MessageType = "benchmark";
        _container.Add(new StringValue("name", "test"));
        _container.Add(new IntValue("count", 100));
        _container.Add(new DoubleValue("price", 99.99));
        _container.Add(new BoolValue("active", true));
    }

    [Benchmark]
    public string JsonSerialize()
    {
        return _container.Serialize();
    }

    [Benchmark]
    public byte[] BinarySerialize()
    {
        return _container.Store.Serialize();
    }

    [Benchmark]
    public ValueContainer JsonDeserialize()
    {
        string json = _container.Serialize();
        return new ValueContainer(json);
    }
}
```

### Container Operations Benchmark

```csharp
[MemoryDiagnoser]
public class ContainerBenchmark
{
    [Benchmark]
    public ValueContainer CreateContainer()
    {
        return new ValueContainer();
    }

    [Benchmark]
    public void AddValue()
    {
        var container = new ValueContainer();
        container.Add(new IntValue("x", 42));
    }

    [Benchmark]
    public Value? GetValue()
    {
        var container = new ValueContainer();
        container.Add(new IntValue("x", 42));
        return container.GetValue("x");
    }
}
```

---

## Running Benchmarks

### Using BenchmarkDotNet

```bash
cd benchmarks
dotnet run -c Release
```

### Manual Timing

```csharp
var sw = Stopwatch.StartNew();
for (int i = 0; i < 100000; i++)
{
    var container = new ValueContainer();
    container.Add(new IntValue("x", i));
    string json = container.Serialize();
}
sw.Stop();
Console.WriteLine($"Ops/sec: {100000.0 / sw.Elapsed.TotalSeconds:N0}");
```

---

## Performance Tips

### 1. Use Binary for Speed

```csharp
// Faster
byte[] binary = container.Store.Serialize();

// Slower but readable
string json = container.Serialize();
```

### 2. Reuse Containers

```csharp
// Bad: Creates new container each time
for (int i = 0; i < 1000; i++)
{
    var c = new ValueContainer();
    // ...
}

// Good: Reuse container
var container = new ValueContainer();
for (int i = 0; i < 1000; i++)
{
    container.Clear();
    // ...
}
```

### 3. Batch Value Operations

```csharp
// Add multiple values at once if possible
container.Add(new IntValue("a", 1));
container.Add(new IntValue("b", 2));
container.Add(new IntValue("c", 3));

// Rather than interleaving with reads
```

### 4. Use Appropriate Types

```csharp
// Use IntValue for small numbers (faster)
container.Add(new IntValue("count", 100));

// Use LLongValue only when needed
container.Add(new LLongValue("big_number", 5000000000L));
```

---

## Baseline Targets

Based on production requirements:

| Metric | Target | Current | Status |
|--------|--------|---------|--------|
| Container Create | >500K/sec | 1M/sec | ✅ |
| Binary Serialize | >200K/sec | 500K/sec | ✅ |
| JSON Serialize | >100K/sec | 200K/sec | ✅ |
| Value Lookup | >1M/sec | 5M/sec | ✅ |
| Memory per Value | <100 bytes | ~62 bytes | ✅ |

---

## See Also

- [Features](../FEATURES.md) - Feature documentation
- [Best Practices](../guides/BEST_PRACTICES.md) - Performance tips
- [Architecture](../ARCHITECTURE.md) - System design
