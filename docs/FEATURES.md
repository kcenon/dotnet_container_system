# Features

> **Language:** **English** | [한국어](FEATURES_KO.md)

**Last Updated:** 2025-11-26
**Version:** 1.0.0

Complete feature documentation for .NET Container System.

---

## Overview

.NET Container System provides a type-safe, high-performance container framework for message serialization and data storage with cross-language compatibility. This document provides comprehensive details about all features and capabilities, including value types, serialization formats, advanced features, and integration capabilities.

---

## Table of Contents

1. [Core Features](#core-features)
   - [Type-Safe Value System](#1-type-safe-value-system)
   - [Message Container](#2-message-container)
   - [Multiple Serialization Formats](#3-multiple-serialization-formats)
   - [Thread Safety](#4-thread-safety)
   - [Nested Containers](#5-nested-containers)
   - [Multiple Values with Same Name](#6-multiple-values-with-same-name)
   - [Array Values](#7-array-values)
2. [Value Types Detail](#value-types-detail)
3. [Advanced Features](#advanced-features)
4. [Serialization Format](#serialization-format)
5. [Cross-Language Compatibility](#cross-language-compatibility)
6. [Performance Characteristics](#performance-characteristics)
7. [Comparison with C++ Version](#comparison-with-c-version)

---

## Core Features

### 1. Type-Safe Value System

16 distinct value types with compile-time type checking:

```csharp
// Null
container.Add(new NullValue("empty"));

// Boolean
container.Add(new BoolValue("active", true));

// Integers (16-bit)
container.Add(new ShortValue("small", 100));
container.Add(new UShortValue("unsigned_small", 200));

// Integers (32-bit)
container.Add(new IntValue("count", 1000));
container.Add(new UIntValue("unsigned_count", 2000));

// Integers (32-bit range, C++ compatible)
container.Add(new LongValue("medium", 100000));
container.Add(new ULongValue("unsigned_medium", 200000));

// Integers (64-bit)
container.Add(new LLongValue("large", 5000000000L));
container.Add(new ULLongValue("unsigned_large", 10000000000UL));

// Floating point
container.Add(new FloatValue("ratio", 0.5f));
container.Add(new DoubleValue("precise", 3.14159265359));

// String
container.Add(new StringValue("name", "Hello World"));

// Binary
container.Add(new BytesValue("data", new byte[] { 1, 2, 3 }));

// Nested container
container.Add(new ContainerValue("nested", innerContainer));

// Array
container.Add(new ArrayValue("list"));
```

### 2. Message Container

High-level container with routing metadata:

```csharp
var container = new ValueContainer();

// Message type
container.MessageType = "user_profile";

// Source identification
container.SetSource("client_app", "session_123");

// Target identification
container.SetTarget("user_service", "profile_handler");

// Version tracking
container.Version = "1.0.0";
```

### 3. Multiple Serialization Formats

#### JSON Serialization

```csharp
// Serialize to JSON
string json = container.Serialize();

// Deserialize from JSON
var restored = new ValueContainer(json);
```

#### Binary Serialization

```csharp
// Serialize to binary
byte[] binary = container.Store.Serialize();

// Deserialize from binary
container.Store.Deserialize(binary);
```

#### XML Serialization

```csharp
// Serialize to XML
string xml = container.ToXml();
```

#### JSON v2.0 (Cross-Language)

```csharp
using ContainerSystem.Adapters;

// To JSON v2.0 format
string jsonV2 = JsonV2Adapter.ToJson(container);

// From JSON v2.0 format
var restored = JsonV2Adapter.FromJson(jsonV2);
```

### 4. Thread Safety

Built-in thread-safe operations:

```csharp
// Thread-safe container operations
var container = new ValueContainer();

Parallel.For(0, 1000, i =>
{
    container.Add(new IntValue($"item_{i}", i));
});

// Thread-safe value store
var store = new ValueStore(threadSafe: true);
```

### 5. Nested Containers

Hierarchical data structures:

```csharp
// Create nested structure
var address = new ValueContainer();
address.Add(new StringValue("street", "123 Main St"));
address.Add(new StringValue("city", "Seoul"));

var user = new ValueContainer();
user.Add(new StringValue("name", "John"));
user.Add(new ContainerValue("address", address));
```

### 6. Multiple Values with Same Name

Support for repeated values:

```csharp
// Add multiple tags
container.Add(new StringValue("tag", "important"));
container.Add(new StringValue("tag", "urgent"));
container.Add(new StringValue("tag", "review"));

// Retrieve all tags
var tags = container.ValueArray("tag");
foreach (var tag in tags)
{
    Console.WriteLine(tag.ToString());
}
```

### 7. Array Values

.NET Container System includes ArrayValue for storing homogeneous or heterogeneous collections:

```csharp
// Create an empty array
var scores = new ArrayValue("scores");
scores.Append(new IntValue("", 95));
scores.Append(new IntValue("", 87));
scores.Append(new IntValue("", 92));

// Create array with initial elements
var colors = new ArrayValue("colors", new List<Value>
{
    new StringValue("", "red"),
    new StringValue("", "green"),
    new StringValue("", "blue")
});

// Access elements
var firstScore = scores.At(0);
var lastColor = colors.At(colors.Count - 1);

// Iterate over array
foreach (var element in scores)
{
    Console.WriteLine($"Score: {element.ToInt()}");
}

// Mixed-type array
var mixed = new ArrayValue("data");
mixed.Append(new StringValue("", "Alice"));
mixed.Append(new IntValue("", 30));
mixed.Append(new DoubleValue("", 5000.50));
mixed.Append(new BoolValue("", true));
```

**Key Features:**
- O(1) element access by index
- Support for homogeneous and heterogeneous arrays
- Fluent API with method chaining
- C++ compatibility with `PushBack()` method
- Type safety with compile-time checking

---

## Value Types Detail

### Numeric Types

| Type | .NET Type | Range | Bytes |
|------|-----------|-------|-------|
| ShortValue | short | -32,768 to 32,767 | 2 |
| UShortValue | ushort | 0 to 65,535 | 2 |
| IntValue | int | -2³¹ to 2³¹-1 | 4 |
| UIntValue | uint | 0 to 2³²-1 | 4 |
| LongValue | int | -2³¹ to 2³¹-1 (C++ compat) | 4 |
| ULongValue | uint | 0 to 2³²-1 (C++ compat) | 4 |
| LLongValue | long | -2⁶³ to 2⁶³-1 | 8 |
| ULLongValue | ulong | 0 to 2⁶⁴-1 | 8 |
| FloatValue | float | ±3.4 × 10³⁸ | 4 |
| DoubleValue | double | ±1.7 × 10³⁰⁸ | 8 |

### Type Conversions

All value types support safe type conversion:

```csharp
var value = container.GetValue("number");

// Conversions
bool b = value.ToBoolean();
int i = value.ToInt();
long l = value.ToLong();
float f = value.ToFloat();
double d = value.ToDouble();
string s = value.ToString();
byte[] bytes = value.ToBytes();
```

### Type Checking

```csharp
var value = container.GetValue("data");

if (value.IsNull()) { /* ... */ }
if (value.IsBoolean()) { /* ... */ }
if (value.IsNumeric()) { /* ... */ }
if (value.IsString()) { /* ... */ }
if (value.IsBytes()) { /* ... */ }
if (value.IsContainer()) { /* ... */ }
```

---

## Advanced Features

### Statistics Tracking

```csharp
var container = new ValueContainer();

// Operations...
container.Add(new IntValue("x", 1));
var val = container.GetValue("x");

// Check statistics
Console.WriteLine($"Read count: {container.Store.ReadCount}");
Console.WriteLine($"Write count: {container.Store.WriteCount}");
Console.WriteLine($"Serialization count: {container.Store.SerializationCount}");
```

### Parent-Child Relationships

```csharp
var parent = new ContainerValue("parent", new ValueContainer());
var child = new StringValue("child", "value");

parent.AddChild(child);

// Navigate
Console.WriteLine(child.Parent?.Name); // "parent"
Console.WriteLine(parent.ChildCount);   // 1
```

### Value Iteration

```csharp
// Get all values
var allValues = container.Values();

// Get values by name
var namedValues = container.ValueArray("tag");

// Get children (optionally only containers)
var children = value.Children(onlyContainer: false);
```

---

## Serialization Format

### Binary Format

```
[type: 1 byte][name_length: 4 bytes LE][name: UTF-8][value_size: 4 bytes LE][value: bytes]
```

### JSON Format

```json
{
  "message_type": "user_profile",
  "source_id": "client",
  "source_sub_id": "session_1",
  "target_id": "server",
  "target_sub_id": "handler",
  "values": [
    {"name": "username", "type": "string", "value": "john_doe"},
    {"name": "age", "type": "int", "value": 30}
  ]
}
```

### JSON v2.0 Format (Cross-Language)

```json
{
  "header": {
    "message_type": "user_profile",
    "source": {"id": "client", "sub_id": "session_1"},
    "target": {"id": "server", "sub_id": "handler"}
  },
  "values": {
    "username": {"type": 13, "value": "john_doe"},
    "age": {"type": 4, "value": 30}
  }
}
```

---

## Cross-Language Compatibility

### Supported Languages

| Language | Library | Status |
|----------|---------|--------|
| C++ | container_system | ✅ Full |
| Python | python_container_system | ✅ Full |
| Go | - | ✅ Via JSON v2.0 |
| Rust | - | ✅ Via JSON v2.0 |
| Node.js | - | ✅ Via JSON v2.0 |
| TypeScript | - | ✅ Via JSON v2.0 |

### Type Mapping

| .NET Type | C++ Type | Python Type |
|-----------|----------|-------------|
| NullValue | null_value | NullValue |
| BoolValue | bool_value | BoolValue |
| IntValue | int32_value | IntValue |
| LLongValue | int64_value | LLongValue |
| DoubleValue | double_value | DoubleValue |
| StringValue | string_value | StringValue |
| BytesValue | bytes_value | BytesValue |
| ContainerValue | container_value | ContainerValue |

---

## Performance Characteristics

### Container Operations

| Operation | Complexity | Throughput | Notes |
|-----------|-----------|------------|-------|
| Container Creation | O(1) | ~1M/sec | Empty container |
| Value Addition | O(1) | ~2M/sec | Single value |
| Value Lookup | O(1) | ~5M/sec | Dictionary-based |
| Value Removal | O(1) | ~2M/sec | By key |
| Iteration | O(n) | ~10M/sec | All values |

### Serialization Performance

| Format | Serialize | Deserialize | Notes |
|--------|-----------|-------------|-------|
| JSON | ~200K/sec | ~180K/sec | 10 values |
| Binary | ~500K/sec | ~450K/sec | 10 values |
| XML | ~150K/sec | ~120K/sec | 10 values |
| JSON v2.0 | ~180K/sec | ~160K/sec | Cross-language format |

### Thread Safety Overhead

| Mode | Operation | Overhead | Notes |
|------|-----------|----------|-------|
| Single-threaded | Read/Write | 0% | No locking |
| Thread-safe (read) | Read | ~5% | ReaderWriterLockSlim |
| Thread-safe (write) | Write | ~15% | Exclusive lock |

### Memory Efficiency

- **Value storage**: 16-24 bytes per value (excluding data)
- **Container overhead**: ~200 bytes base + data
- **Array values**: 8 bytes per element reference
- **Nested containers**: Shared pointer overhead (~16 bytes)

### Optimization Tips

1. **Pre-allocate capacity**: Use `ValueStore(initialCapacity: N)` when size is known
2. **Disable thread safety**: For single-threaded scenarios, avoid locking overhead
3. **Use binary format**: 2.5x faster than JSON for serialization
4. **Batch operations**: Group multiple operations to reduce overhead
5. **Reuse containers**: Clear and reuse instead of creating new instances

---

## Comparison with C++ Version

| Feature | C++ | .NET |
|---------|-----|------|
| Value types | 15 | 16 (+ ArrayValue) |
| Binary serialization | ✅ | ✅ |
| JSON serialization | ✅ | ✅ |
| XML serialization | ✅ | ✅ |
| SIMD optimization | ✅ | ❌ |
| Memory pooling | ✅ | ❌ |
| Thread safety | ✅ | ✅ |
| Cross-language | ✅ | ✅ |

---

## Use Cases

### Enterprise Messaging
- **Message routing** with source/target identification
- **Protocol versioning** for backward compatibility
- **Message type discrimination** for handlers
- **Metadata management** for audit trails

### Data Serialization
- **Configuration files** with nested structures
- **Network protocols** with compact binary format
- **API payloads** with JSON serialization
- **Data exchange** between microservices

### Cross-Platform Communication
- **Polyglot systems** with JSON v2.0 adapter
- **Legacy system integration** with multiple format support
- **Service meshes** with type-safe messaging
- **Event streaming** with efficient serialization

---

## See Also

- 📖 [Quick Start Guide](guides/QUICK_START.md) - Getting started in 5 minutes
- 🏗️ [Architecture](ARCHITECTURE.md) - System design and patterns
- 📚 [API Reference](API_REFERENCE.md) - Complete API documentation
- 📊 [Benchmarks](performance/BENCHMARKS.md) - Performance metrics
- 🔗 [Compatibility Guide](advanced/COMPATIBILITY.md) - Cross-language compatibility
- ✅ [Best Practices](guides/BEST_PRACTICES.md) - Recommended patterns
- ❓ [FAQ](guides/FAQ.md) - Frequently asked questions

---

**Last Updated**: 2025-11-26
