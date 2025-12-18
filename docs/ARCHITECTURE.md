# Container System Architecture (.NET/C#)

**Version**: 1.0.0
**Last Updated**: 2025-10-27
**Language**: C# (.NET 6.0+)

---

## Table of Contents

- [Overview](#overview)
- [Design Philosophy](#design-philosophy)
- [System Architecture](#system-architecture)
- [Core Components](#core-components)
- [Data Flow](#data-flow)
- [Concurrency Model](#concurrency-model)
- [Value Type System](#value-type-system)
- [Long/ULong Type Policy](#longulong-type-policy)
- [Serialization Architecture](#serialization-architecture)
- [Container Implementation](#container-implementation)
- [Error Handling](#error-handling)
- [Cross-Language Compatibility](#cross-language-compatibility)
- [Performance Considerations](#performance-considerations)
- [Best Practices](#best-practices)
- [Cross-References](#cross-references)

---

## Overview

The .NET Container System is the C# implementation of the KCENON Container System, designed for **high-performance message exchange** across multiple programming languages. It provides a type-safe, cross-language compatible data serialization framework with the unified long/ulong type policy to ensure binary compatibility with C++, Python, Go, Rust, and Node.js/TypeScript implementations.

### Key Features

- **15 Value Types**: Complete support for all standard types
- **Type Safety**: Strong typing with C# generics
- **Platform Independence**: Enforced 4-byte long/ulong serialization
- **Binary Compatibility**: Wire-protocol compatible across 6 languages
- **LINQ Support**: Fluent API for container operations
- **Async/Await**: Asynchronous serialization support

---

## Design Philosophy

### 1. Type Safety First

```csharp
// Compile-time type checking
var intValue = new IntValue("count", 42);
int count = intValue.GetValue();  // ✅ Type-safe

// Runtime type validation
var value = container.GetValue("count");
if (value is IntValue iv) {
    int count = iv.GetValue();
}
```

### 2. LINQ-Friendly Design

```csharp
// Query containers with LINQ
var largeValues = container
    .Values
    .OfType<IntValue>()
    .Where(v => v.GetValue() > 1000)
    .ToList();
```

### 3. Cross-Platform Consistency

All numeric types use explicit sizes (`Int32`, `Int64`, etc.) to avoid platform differences.

---

## System Architecture

### Project Structure

```
dotnet_container_system/
├── ContainerSystem/              # Main library
│   ├── Core/
│   │   ├── Value.cs             # Base value interface
│   │   ├── ValueTypes.cs        # Enum (0-14)
│   │   └── Container.cs         # Container class
│   ├── Values/
│   │   ├── NumericValue.cs      # Numeric types (0-6)
│   │   ├── UIntValues.cs        # Unsigned types (2, 4, 7, 9)
│   │   ├── Int64Values.cs       # 64-bit types (8, 9)
│   │   ├── FloatValue.cs        # Float/Double (5, 10)
│   │   ├── StringValue.cs       # String (11)
│   │   ├── BytesValue.cs        # Bytes (12)
│   │   └── ArrayValue.cs        # Array (14)
│   └── ContainerSystem.csproj
├── ContainerSystem.Tests/        # Unit tests
│   ├── LongRangeCheckingTests.cs (39 tests)
│   └── ContainerTests.cs
└── README.md
```

### Class Hierarchy

```
IValue (interface)
├── Value (abstract base)
    ├── BoolValue
    ├── ShortValue / UShortValue
    ├── IntValue / UIntValue
    ├── FloatValue / DoubleValue
    ├── LongValue / ULongValue       (32-bit enforced)
    ├── LLongValue / ULLongValue     (64-bit)
    ├── StringValue
    ├── BytesValue
    ├── Container
    └── ArrayValue
```

---

## Core Components

### ValueContainer

The `ValueContainer` is the main entry point for the container system. It holds:

- **Header**: Metadata including source/target IDs, message type, and version
- **Body**: A list of `Value` objects stored internally

```csharp
public class ValueContainer : IEnumerable<Value>, IDisposable
{
    // Header fields
    private string _messageType;
    private string _sourceId;
    private string _sourceSubId;
    private string _targetId;
    private string _targetSubId;
    private string _version;

    // Body - list of values
    private readonly List<Value> _values;

    // Thread safety via ReaderWriterLockSlim
    private readonly ReaderWriterLockSlim _rwLock;
    private volatile bool _threadSafeEnabled;
}
```

### ValueStore

The `ValueStore` provides a high-performance key-value storage engine with the following characteristics:

#### Dictionary-of-Lists Structure

The internal storage uses a **Dictionary-of-Lists** pattern, allowing multiple values to be stored under the same key:

```csharp
private readonly Dictionary<string, List<Value>> _values;
```

This design enables:
- **O(1) average lookup** for key-based access
- **Multiple values per key** support for array-like semantics
- **Efficient iteration** over all values

#### Thread-Safety Model

Thread safety is implemented using `ReaderWriterLockSlim`:

```csharp
private readonly ReaderWriterLockSlim _rwLock;
private volatile bool _threadSafeEnabled;
```

- **Read operations**: Use `EnterReadLock()` - multiple concurrent readers allowed
- **Write operations**: Use `EnterWriteLock()` - exclusive access
- **Statistics tracking**: Read/write counts via `Interlocked` operations

### Value Hierarchy

All value types inherit from the abstract `Value` base class and map to Protocol IDs:

| Protocol ID | Type Name | .NET Type | Size (bytes) |
|-------------|-----------|-----------|--------------|
| 0 | BoolValue | bool | 1 |
| 1 | ShortValue | Int16 | 2 |
| 2 | UShortValue | UInt16 | 2 |
| 3 | IntValue | Int32 | 4 |
| 4 | UIntValue | UInt32 | 4 |
| 5 | FloatValue | Single | 4 |
| 6 | LongValue | Int32 | 4 |
| 7 | ULongValue | UInt32 | 4 |
| 8 | LLongValue | Int64 | 8 |
| 9 | ULLongValue | UInt64 | 8 |
| 10 | DoubleValue | Double | 8 |
| 11 | StringValue | string | variable |
| 12 | BytesValue | byte[] | variable |
| 13 | ContainerValue | nested | variable |
| 14 | ArrayValue | array | variable |

---

## Data Flow

The following diagram illustrates the typical data flow in the container system:

```
┌─────────────────────────────────────────────────────────────────┐
│                          User Code                              │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                     ValueContainerFactory                        │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │ • Create()          - Create empty container            │    │
│  │ • Create(msgType)   - Create with message type          │    │
│  │ • FromJson(json)    - Deserialize from JSON             │    │
│  │ • FromBytes(data)   - Deserialize from bytes            │    │
│  │ • CreateBuilder()   - Fluent builder pattern            │    │
│  └─────────────────────────────────────────────────────────┘    │
│                              │                                   │
│                              │ ApplyOptions()                    │
│                              │ (thread safety, etc.)             │
│                              ▼                                   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      ValueContainer                              │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │ Header: source_id, target_id, message_type, version     │    │
│  ├─────────────────────────────────────────────────────────┤    │
│  │ Body: List<Value>                                       │    │
│  │   ├── Add(value)                                        │    │
│  │   ├── GetValue(key)                                     │    │
│  │   └── SetValue(key, value)                              │    │
│  └─────────────────────────────────────────────────────────┘    │
│                              │                                   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                       Serialization                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌────────────────┐  │
│  │   ToJson()      │  │   ToXml()       │  │ SerializeArray │  │
│  │   JSON format   │  │   XML format    │  │ byte[] output  │  │
│  └─────────────────┘  └─────────────────┘  └────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### Typical Usage Flow

```csharp
// 1. Create via Factory (DI recommended)
var factory = new ValueContainerFactory(options);
var container = factory.Create("request_message");

// 2. Add values
container.Add(new StringValue("user", "alice"));
container.Add(new IntValue("age", 30));

// 3. Serialize for transport
byte[] data = container.SerializeArray();

// 4. Deserialize on receiver
var received = factory.FromBytes(data);
var user = received.GetValue("user")?.ToString();
```

---

## Concurrency Model

### Default Behavior

**IMPORTANT**: Container instances are **NOT thread-safe by default**. This design choice optimizes for single-threaded scenarios which are more common.

### Enabling Thread Safety

Thread safety can be enabled in two ways:

#### 1. Per-Instance Activation

```csharp
var container = new ValueContainer();
container.EnableThreadSafety();  // Now thread-safe

// Check status
bool isSafe = container.IsThreadSafe;  // true
```

#### 2. Via Dependency Injection (Recommended)

```csharp
// In Startup.cs or Program.cs
services.AddContainerSystem(options =>
{
    options.EnableThreadSafetyByDefault = true;
});

// All containers created via factory will be thread-safe
public class MyService
{
    private readonly IValueContainerFactory _factory;

    public MyService(IValueContainerFactory factory)
    {
        _factory = factory;
    }

    public ValueContainer CreateSafeContainer()
    {
        // Automatically thread-safe due to DI configuration
        return _factory.Create("my_message");
    }
}
```

### Lock Semantics

When thread safety is enabled:

| Operation | Lock Type | Behavior |
|-----------|-----------|----------|
| Read (GetValue, Count, etc.) | `EnterReadLock()` | Multiple concurrent readers |
| Write (Add, SetValue, etc.) | `EnterWriteLock()` | Exclusive access |
| Iteration | Snapshot | Creates copy for safe enumeration |

### Performance Trade-offs

```csharp
// Single-threaded (default) - fastest
var container = new ValueContainer();
for (int i = 0; i < 1000000; i++)
{
    container.Add(new IntValue($"key_{i}", i));
}

// Multi-threaded - ~10-15% overhead
container.EnableThreadSafety();
Parallel.For(0, 1000000, i =>
{
    container.Add(new IntValue($"key_{i}", i));
});
```

---

## Value Type System

### ValueTypes Enum

```csharp
public enum ValueTypes : byte
{
    BoolValue = 0,
    ShortValue = 1,
    UShortValue = 2,
    IntValue = 3,
    UIntValue = 4,
    FloatValue = 5,
    LongValue = 6,      // 32-bit signed
    ULongValue = 7,     // 32-bit unsigned
    LLongValue = 8,     // 64-bit signed
    ULLongValue = 9,    // 64-bit unsigned
    DoubleValue = 10,
    StringValue = 11,
    BytesValue = 12,
    ContainerValue = 13,
    ArrayValue = 14
}
```

### IValue Interface

```csharp
public interface IValue
{
    string Name { get; }
    ValueTypes Type { get; }
    byte[] Serialize();
    int Size();
    object GetValue();
}
```

---

## Long/ULong Type Policy

### Problem: Platform and Language Inconsistencies

Different platforms and languages have different `long` sizes:
- **C/C++ on Unix**: 8 bytes
- **C/C++ on Windows**: 4 bytes
- **C# long**: Always 8 bytes (Int64)
- **Python int**: Arbitrary precision

This caused serious cross-language compatibility issues.

### Solution: Enforced 32-Bit Serialization

**Type Mapping**:
| Type ID | .NET Name | Backing Type | Range | Serialization |
|---------|-----------|--------------|-------|---------------|
| 6 | LongValue | Int32 | [-2³¹, 2³¹-1] | 4 bytes LE |
| 7 | ULongValue | UInt32 | [0, 2³²-1] | 4 bytes LE |
| 8 | LLongValue | Int64 | [-2⁶³, 2⁶³-1] | 8 bytes LE |
| 9 | ULLongValue | UInt64 | [0, 2⁶⁴-1] | 8 bytes LE |

### Implementation

**LongValue** (Values/NumericValue.cs):
```csharp
public class LongValue : Value
{
    private const int INT32_MIN = int.MinValue;
    private const int INT32_MAX = int.MaxValue;
    private int _value;

    public LongValue(string name, long value) : base(name, ValueTypes.LongValue)
    {
        if (value < INT32_MIN || value > INT32_MAX)
        {
            throw new OverflowException(
                $"LongValue: value {value} exceeds 32-bit range " +
                $"[{INT32_MIN}, {INT32_MAX}]. " +
                "Use LLongValue for 64-bit values."
            );
        }
        _value = (int)value;
    }

    public override int Size() => sizeof(int);  // Always 4 bytes

    public override byte[] Serialize()
    {
        return BitConverter.GetBytes(_value);
    }

    public override object GetValue() => _value;
    public int GetInt32() => _value;
}
```

**ULongValue** (Values/UIntValues.cs):
```csharp
public class ULongValue : Value
{
    private const uint UINT32_MAX = uint.MaxValue;
    private uint _value;

    public ULongValue(string name, ulong value) : base(name, ValueTypes.ULongValue)
    {
        if (value > UINT32_MAX)
        {
            throw new OverflowException(
                $"ULongValue: value {value} exceeds 32-bit range [0, {UINT32_MAX}]. " +
                "Use ULLongValue for 64-bit values."
            );
        }
        _value = (uint)value;
    }

    public override int Size() => sizeof(uint);  // Always 4 bytes

    public override byte[] Serialize()
    {
        return BitConverter.GetBytes(_value);
    }

    public override object GetValue() => _value;
    public uint GetUInt32() => _value;
}
```

**LLongValue** (Values/Int64Values.cs):
```csharp
public class LLongValue : Value
{
    private long _value;

    public LLongValue(string name, long value) : base(name, ValueTypes.LLongValue)
    {
        _value = value;
    }

    public override int Size() => sizeof(long);  // 8 bytes

    public override byte[] Serialize()
    {
        return BitConverter.GetBytes(_value);
    }

    public override object GetValue() => _value;
    public long GetInt64() => _value;
}
```

### Breaking Changes

**Before**:
```csharp
// C# long is always 8 bytes, but serialized inconsistently
var lv = new LongValue("id", 5_000_000_000L);  // Might overflow on deserialization
```

**After**:
```csharp
// Now range-checked at construction
var lv = new LongValue("id", 5_000_000_000L);
// ❌ Throws OverflowException

// ✅ Use LLongValue for 64-bit values
var llv = new LLongValue("id", 5_000_000_000L);
```

### Migration Guide

**Step 1**: Identify values exceeding 32-bit range
```powershell
# Search for large long values in C# files
Select-String -Path "*.cs" -Pattern "LongValue.*[0-9]{10,}"
```

**Step 2**: Update to appropriate type
```csharp
// For values in [-2³¹, 2³¹-1]
var lv = new LongValue("count", 1_000_000_000L);  // ✅

// For values beyond 32-bit range
var llv = new LLongValue("big", 5_000_000_000L);  // ✅
```

**Step 3**: Handle exceptions
```csharp
try
{
    var lv = new LongValue("id", userInput);
}
catch (OverflowException ex)
{
    // Value exceeds 32-bit range, use LLongValue
    var llv = new LLongValue("id", userInput);
}
```

### Type Selection Guide

| Value Range | Type to Use | Example |
|-------------|-------------|---------|
| [-2³¹, 2³¹-1] | LongValue | `new LongValue("id", 1_000_000_000L)` |
| [0, 2³²-1] | ULongValue | `new ULongValue("count", 3_000_000_000UL)` |
| Beyond 32-bit signed | LLongValue | `new LLongValue("big", 5_000_000_000L)` |
| Beyond 32-bit unsigned | ULLongValue | `new ULLongValue("huge", 10_000_000_000UL)` |

---

## Serialization Architecture

### Wire Protocol Format

All values follow the same serialization format:

```
[type: 1 byte][name_length: 4 bytes LE][name: UTF-8][value_size: 4 bytes LE][value: bytes]
```

### Example: IntValue Serialization

```csharp
var value = new IntValue("count", 42);
var bytes = value.Serialize();

// Result:
// [03][05 00 00 00][63 6F 75 6E 74][04 00 00 00][2A 00 00 00]
//  ^   ^            ^                ^            ^
//  |   |            |                |            |
//  |   |            |                |            +-- Value: 42 (LE)
//  |   |            |                +-- Size: 4 bytes
//  |   |            +-- Name: "count" (UTF-8)
//  |   +-- Name Length: 5
//  +-- Type: IntValue (3)
```

### Container Serialization

```csharp
public override byte[] Serialize()
{
    using var ms = new MemoryStream();
    using var writer = new BinaryWriter(ms);

    // Header
    writer.Write((byte)Type);
    var nameBytes = Encoding.UTF8.GetBytes(Name);
    writer.Write(nameBytes.Length);
    writer.Write(nameBytes);

    // Serialize all values
    var valueStream = new MemoryStream();
    var valueWriter = new BinaryWriter(valueStream);
    foreach (var value in _values.Values)
    {
        valueWriter.Write(value.Serialize());
    }

    // Write size and values
    var valueBytes = valueStream.ToArray();
    writer.Write(valueBytes.Length);
    writer.Write(valueBytes);

    return ms.ToArray();
}
```

---

## Container Implementation

### Key Features

```csharp
public class Container : Value
{
    private readonly Dictionary<string, IValue> _values;

    // Add value
    public void Add(IValue value)
    {
        _values[value.Name] = value;
    }

    // Get value with type checking
    public T GetValueAs<T>(string name) where T : IValue
    {
        if (!_values.TryGetValue(name, out var value))
            throw new KeyNotFoundException($"Value '{name}' not found");

        if (value is not T typedValue)
            throw new InvalidCastException(
                $"Value '{name}' is {value.GetType().Name}, not {typeof(T).Name}");

        return typedValue;
    }

    // LINQ support
    public IEnumerable<IValue> Values => _values.Values;

    // Fluent API
    public Container AddValue(IValue value)
    {
        Add(value);
        return this;
    }
}
```

### Usage Examples

```csharp
// Create container
var container = new Container("user")
    .AddValue(new StringValue("name", "Alice"))
    .AddValue(new IntValue("age", 30))
    .AddValue(new LongValue("timestamp", 1234567890L));

// Type-safe retrieval
var name = container.GetValueAs<StringValue>("name").GetString();
var age = container.GetValueAs<IntValue>("age").GetInt32();

// LINQ queries
var numericValues = container.Values
    .Where(v => v is IntValue || v is LongValue)
    .ToList();

// Serialization
byte[] data = container.Serialize();

// Deserialization
var restored = Container.Deserialize(data);
```

---

## Error Handling

### Exception Hierarchy

```csharp
Exception
├── OverflowException           // Range checking
├── InvalidCastException        // Type mismatches
├── KeyNotFoundException        // Missing values
├── FormatException             // Deserialization errors
└── ArgumentException           // Invalid arguments
```

### Best Practices

```csharp
// 1. Use try-catch for range validation
try
{
    var lv = new LongValue("id", userInput);
}
catch (OverflowException)
{
    var llv = new LLongValue("id", userInput);
}

// 2. Use pattern matching for type checking
if (container.TryGetValue("count", out var value))
{
    switch (value)
    {
        case IntValue iv:
            Console.WriteLine($"Int: {iv.GetInt32()}");
            break;
        case LongValue lv:
            Console.WriteLine($"Long: {lv.GetInt32()}");
            break;
    }
}

// 3. Use null-conditional operators
var name = container
    .TryGetValue("name", out var v) && v is StringValue sv
    ? sv.GetString()
    : "Unknown";
```

---

## Cross-Language Compatibility

This implementation is **binary-compatible** with:

- ✅ **C++** container_system (4-byte long/ulong)
- ✅ **Python** container_system (4-byte enforcement)
- ✅ **Go** container_system (int32/uint32 types)
- ✅ **Rust** container_system (i32/u32 with Result<T>)
- ✅ **Node.js/TypeScript** container_system (4-byte Buffer)

### Interoperability Example

```csharp
// C# creates container
var container = new Container("data")
    .AddValue(new LongValue("timestamp", 1234567890L))
    .AddValue(new StringValue("message", "Hello"));

byte[] serialized = container.Serialize();

// Python reads it
# data = Container.deserialize(serialized)
# print(data.get("timestamp").value)  # 1234567890

// Or TypeScript
# const container = Container.deserialize(Buffer.from(serialized));
# console.log(container.get("timestamp").getValue());  // 1234567890
```

---

## Performance Considerations

### Optimization Tips

1. **Use appropriate collection sizes**:
```csharp
var container = new Container("data", initialCapacity: 100);
```

2. **Avoid boxing**:
```csharp
// ❌ Slow (boxing)
object value = intValue.GetValue();

// ✅ Fast (no boxing)
int value = intValue.GetInt32();
```

3. **Use Span<T> for large data**:
```csharp
public ReadOnlySpan<byte> SerializeToSpan()
{
    // Zero-copy serialization
}
```

4. **Pool byte arrays**:
```csharp
var arrayPool = ArrayPool<byte>.Shared;
var buffer = arrayPool.Rent(size);
try
{
    // Use buffer
}
finally
{
    arrayPool.Return(buffer);
}
```

### Benchmarks

**Environment**: .NET 8.0, Apple M1 Max

| Operation | Time (ns) | Allocations |
|-----------|-----------|-------------|
| IntValue creation | 50 | 48 bytes |
| LongValue creation | 80 | 48 bytes |
| Container (10 values) | 800 | 512 bytes |
| Serialize (1KB) | 2,000 | 1,024 bytes |
| Deserialize (1KB) | 3,000 | 1,200 bytes |

---

## Best Practices

### 1. Type Selection

```csharp
// ✅ Good: Use specific types
var count = new IntValue("count", 42);
var timestamp = new LongValue("timestamp", 1234567890L);

// ❌ Avoid: Using 64-bit types unnecessarily
var count = new LLongValue("count", 42L);  // Wastes 4 bytes per value
```

### 2. Error Handling

```csharp
// ✅ Good: Validate at boundaries
public void ProcessUserInput(long value)
{
    try
    {
        var lv = new LongValue("input", value);
        container.Add(lv);
    }
    catch (OverflowException)
    {
        // Log and use 64-bit type
        var llv = new LLongValue("input", value);
        container.Add(llv);
    }
}
```

### 3. LINQ Usage

```csharp
// ✅ Good: Fluent queries
var summary = container.Values
    .OfType<IntValue>()
    .Select(v => v.GetInt32())
    .Where(v => v > 0)
    .Sum();

// ❌ Avoid: Multiple enumerations
var values = container.Values.OfType<IntValue>();
var count = values.Count();  // Enumeration 1
var sum = values.Sum(v => v.GetInt32());  // Enumeration 2
```

### 4. Naming Conventions

```csharp
// ✅ Good: Descriptive names
var userId = new LongValue("user_id", id);
var createdAt = new LongValue("created_at", timestamp);

// ❌ Avoid: Generic names
var val1 = new LongValue("v1", id);
var data = new LongValue("d", timestamp);
```

---

## Testing

### Test Coverage

**LongRangeCheckingTests.cs** (39 tests):
- Range validation (10 tests)
- Overflow rejection (10 tests)
- Serialization format (4 tests)
- Type conversion (6 tests)
- Error messages (2 tests)
- Platform independence (2 tests)
- Boundary value tests (5 tests via Theory)

```bash
$ dotnet test
Test Run Successful.
Total tests: 39
     Passed: 39
```

### Example Test

```csharp
[Theory]
[InlineData(int.MinValue)]
[InlineData(-1_000_000_000)]
[InlineData(0)]
[InlineData(1_000_000_000)]
[InlineData(int.MaxValue)]
public void LongValue_AcceptsBoundaryValues(int value)
{
    var longValue = new LongValue("test", value);
    Assert.Equal(value, longValue.GetInt32());
}

[Fact]
public void LongValue_RejectsOverflow()
{
    Assert.Throws<OverflowException>(() =>
        new LongValue("test", 5_000_000_000L));
}
```

---

## Cross-References

### Related Container System Implementations

This .NET implementation is part of the KCENON Container System family, designed for cross-language interoperability:

| Language | Repository | Notes |
|----------|------------|-------|
| **Rust** | [rust_container_system](https://github.com/kcenon/rust_container_system) | **Gold Standard** for protocol behavior |
| **C++** | [cpp_container_system](https://github.com/kcenon/cpp_container_system) | High-performance native implementation |
| **Python** | [python_container_system](https://github.com/kcenon/python_container_system) | Scripting and prototyping |
| **Node.js** | [node_container_system](https://github.com/kcenon/node_container_system) | JavaScript/TypeScript support |
| **Go** | [go_container_system](https://github.com/kcenon/go_container_system) | Cloud-native applications |

### Protocol Compatibility

All implementations follow the unified wire protocol defined in `rust_container_system/ARCHITECTURE.md`. When in doubt about serialization behavior or edge cases, refer to the Rust implementation as the authoritative source.

---

## References

- **Policy Document**: `CONTAINER_SYSTEMS_UNIFIED_LONG_POLICY.md`
- **Implementation**: `ContainerSystem/Values/NumericValue.cs`
- **Tests**: `ContainerSystem.Tests/LongRangeCheckingTests.cs`
- **Progress**: `LONG_TYPE_POLICY_IMPLEMENTATION_PROGRESS.md`

---

**Maintainer**: kcenon@naver.com
**License**: BSD 3-Clause
**Version**: 1.1.0
**Last Updated**: 2025-12-18
