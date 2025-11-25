# API Reference

> **Language:** **English**

**Last Updated:** 2025-11-26
**Version:** 1.0.0

Complete API documentation for .NET Container System with code examples and usage patterns.

---

## Table of Contents

1. [Core Classes](#core-classes)
   - [ValueContainer](#valuecontainer)
   - [ValueStore](#valuestore)
   - [Value (Abstract)](#value-abstract)
2. [Value Types](#value-types)
   - [NullValue](#nullvalue)
   - [BoolValue](#boolvalue)
   - [Numeric Values](#numeric-values)
   - [StringValue](#stringvalue)
   - [BytesValue](#bytesvalue)
   - [ContainerValue](#containervalue)
   - [ArrayValue](#arrayvalue)
3. [Adapters](#adapters)
   - [JsonV2Adapter](#jsonv2adapter)
4. [Enumerations](#enumerations)
   - [ValueTypes](#valuetypes)
5. [Usage Examples](#usage-examples)

---

## Core Classes

### ValueContainer

High-level message container with routing metadata and thread-safe operations.

```csharp
namespace ContainerSystem.Core;

public class ValueContainer
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `MessageType` | `string` | Type of the message |
| `SourceId` | `string` | Source identifier |
| `SourceSubId` | `string` | Source sub-identifier |
| `TargetId` | `string` | Target identifier |
| `TargetSubId` | `string` | Target sub-identifier |
| `Version` | `string` | Container version |
| `Count` | `int` | Number of values |
| `Store` | `ValueStore` | Underlying value store |

#### Constructors

```csharp
// Empty container
public ValueContainer()

// From JSON string
public ValueContainer(string json)
```

#### Methods

```csharp
// Set source identification
public void SetSource(string sourceId, string sourceSubId = "")

// Set target identification
public void SetTarget(string targetId, string targetSubId = "")

// Add a value
public void Add(Value value)

// Get single value by name
public Value? GetValue(string key)

// Get all values with name
public List<Value> ValueArray(string key)

// Get all values
public List<Value> Values()

// Clear all values
public void Clear()

// Serialize to JSON
public string Serialize()

// Serialize to XML
public string ToXml()
```

#### Example

```csharp
var container = new ValueContainer();
container.MessageType = "order";
container.SetSource("client", "session_1");
container.SetTarget("server", "handler");

container.Add(new StringValue("product", "Widget"));
container.Add(new IntValue("quantity", 5));

string json = container.Serialize();
```

---

### ValueStore

Key-value storage backend with optional thread safety.

```csharp
namespace ContainerSystem.Core;

public class ValueStore
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ThreadSafe` | `bool` | Thread-safe mode enabled |
| `Count` | `int` | Number of stored values |
| `ReadCount` | `long` | Number of read operations |
| `WriteCount` | `long` | Number of write operations |
| `SerializationCount` | `long` | Number of serializations |

#### Constructors

```csharp
// Default (not thread-safe)
public ValueStore()

// With thread-safe option
public ValueStore(bool threadSafe)
```

#### Methods

```csharp
// Add value
public void Add(Value value)

// Get value by key
public Value? Get(string key)

// Get all values with key
public List<Value> GetAll(string key)

// Get all values
public List<Value> GetAllValues()

// Remove by key
public bool Remove(string key)

// Clear all
public void Clear()

// Serialize to binary
public byte[] Serialize()

// Deserialize from binary
public void Deserialize(byte[] data)
```

---

### Value (Abstract)

Abstract base class for all value types.

```csharp
namespace ContainerSystem.Core;

public abstract class Value
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Name of the value |
| `Type` | `ValueTypes` | Type enumeration |
| `Parent` | `Value?` | Parent value (if nested) |
| `ChildCount` | `int` | Number of children |

#### Methods

```csharp
// Type checking
public bool IsNull()
public bool IsBoolean()
public bool IsNumeric()
public bool IsString()
public bool IsBytes()
public bool IsContainer()

// Type conversions
public virtual bool ToBoolean()
public virtual int ToInt()
public virtual long ToLong()
public virtual float ToFloat()
public virtual double ToDouble()
public override string ToString()
public virtual byte[] ToBytes()

// Data access
public abstract string Data()
public abstract int Size()

// Serialization
public abstract byte[] Serialize()
public virtual string ToJson()
public virtual string ToXml()

// Children management
public List<Value> Children(bool onlyContainer = false)
public List<Value> ValueArray(string key)
public void AddChild(Value child)
```

---

## Value Types

### NullValue

Represents a null/empty value.

```csharp
namespace ContainerSystem.Values;

public class NullValue : Value
```

```csharp
var value = new NullValue("empty");
```

---

### BoolValue

Boolean value.

```csharp
public class BoolValue : Value
```

```csharp
var value = new BoolValue("active", true);
bool b = value.ToBoolean(); // true
```

---

### Numeric Values

#### ShortValue / UShortValue

16-bit integers.

```csharp
public class ShortValue : Value    // -32,768 to 32,767
public class UShortValue : Value   // 0 to 65,535
```

```csharp
var signed = new ShortValue("small", -100);
var unsigned = new UShortValue("positive", 100);
```

#### IntValue / UIntValue

32-bit integers.

```csharp
public class IntValue : Value      // -2³¹ to 2³¹-1
public class UIntValue : Value     // 0 to 2³²-1
```

```csharp
var signed = new IntValue("count", -1000);
var unsigned = new UIntValue("positive", 1000);
```

#### LongValue / ULongValue

32-bit range integers (C++ compatible).

```csharp
public class LongValue : Value     // -2³¹ to 2³¹-1 (enforced)
public class ULongValue : Value    // 0 to 2³²-1 (enforced)
```

```csharp
// Throws OverflowException if out of 32-bit range
var value = new LongValue("medium", 1000000);
```

#### LLongValue / ULLongValue

Full 64-bit integers.

```csharp
public class LLongValue : Value    // -2⁶³ to 2⁶³-1
public class ULLongValue : Value   // 0 to 2⁶⁴-1
```

```csharp
var large = new LLongValue("big", 5_000_000_000L);
var huge = new ULLongValue("huge", 10_000_000_000UL);
```

#### FloatValue / DoubleValue

Floating point values.

```csharp
public class FloatValue : Value    // 32-bit float
public class DoubleValue : Value   // 64-bit double
```

```csharp
var f = new FloatValue("ratio", 0.5f);
var d = new DoubleValue("precise", 3.14159265359);
```

---

### StringValue

String value with UTF-8 encoding.

```csharp
public class StringValue : Value
```

```csharp
var value = new StringValue("name", "Hello World");
string s = value.ToString(); // "Hello World"
```

---

### BytesValue

Binary data.

```csharp
public class BytesValue : Value
```

```csharp
byte[] data = new byte[] { 1, 2, 3, 4, 5 };
var value = new BytesValue("data", data);
byte[] retrieved = value.ToBytes();
```

---

### ContainerValue

Nested container.

```csharp
public class ContainerValue : Value
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Container` | `ValueContainer` | The nested container |

```csharp
var inner = new ValueContainer();
inner.Add(new StringValue("city", "Seoul"));

var outer = new ValueContainer();
outer.Add(new ContainerValue("address", inner));
```

---

### ArrayValue

Array/list of values for homogeneous or heterogeneous collections.

```csharp
namespace ContainerSystem.Values;

public class ArrayValue : Value
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Count` | `int` | Number of elements |
| `IsEmpty` | `bool` | True if array has no elements |

#### Methods

```csharp
// Add element to end
public Value Append(Value value)

// Add element (C++ compatibility)
public void PushBack(Value value)

// Access element by index
public Value At(int index)

// Check if index is valid
public bool IsValidIndex(int index)

// Clear all elements
public void Clear()

// Enumerate all elements
public IEnumerator<Value> GetEnumerator()
```

#### Example

```csharp
// Create empty array
var scores = new ArrayValue("scores");
scores.Append(new IntValue("", 95));
scores.Append(new IntValue("", 87));
scores.Append(new IntValue("", 92));

// Create with initial elements
var colors = new ArrayValue("colors", new List<Value>
{
    new StringValue("", "red"),
    new StringValue("", "green"),
    new StringValue("", "blue")
});

// Access elements
var firstScore = scores.At(0);
var count = scores.Count;

// Iterate
foreach (var element in scores)
{
    Console.WriteLine(element.ToInt());
}

container.Add(scores);
```

---

## Adapters

### JsonV2Adapter

JSON v2.0 format adapter for cross-language compatibility with C++, Python, and other implementations.

```csharp
namespace ContainerSystem.Adapters;

public static class JsonV2Adapter
```

#### Methods

```csharp
// Convert to JSON v2.0 format (cross-language compatible)
public static string ToV2Json(ValueContainer container, bool pretty = false)

// Parse JSON v2.0 format
public static ValueContainer FromV2Json(string jsonStr)

// Convert to C++ nested format
public static string ToCppJson(ValueContainer container, bool pretty = false)

// Parse C++ nested format
public static ValueContainer FromCppJson(string jsonStr)

// Detect JSON format type
public static string DetectFormat(string jsonStr)
// Returns: "v2.0", "cpp", "python", "unknown", or "invalid"

// Convert between formats automatically
public static string ConvertFormat(string jsonStr, string targetFormat, bool pretty = false)
// targetFormat: "v2.0", "cpp", or "python"
```

#### JSON v2.0 Format

```json
{
  "container": {
    "version": "2.0",
    "metadata": {
      "message_type": "user_profile",
      "protocol_version": "1.0.0.0",
      "source": {
        "id": "client",
        "sub_id": "session"
      },
      "target": {
        "id": "server",
        "sub_id": "handler"
      }
    },
    "values": [
      {
        "name": "username",
        "type": 13,
        "type_name": "string",
        "data": "john_doe"
      }
    ]
  }
}
```

#### Example

```csharp
using ContainerSystem.Adapters;

var container = new ValueContainer();
container.MessageType = "user_profile";
container.SetSource("client", "session");
container.SetTarget("server", "handler");
container.Add(new StringValue("username", "john_doe"));
container.Add(new IntValue("age", 30));

// To JSON v2.0 (cross-language format)
string jsonV2 = JsonV2Adapter.ToV2Json(container, pretty: true);

// From JSON v2.0
var restored = JsonV2Adapter.FromV2Json(jsonV2);

// Detect format
string format = JsonV2Adapter.DetectFormat(jsonV2); // "v2.0"

// Convert between formats
string cppJson = JsonV2Adapter.ConvertFormat(jsonV2, "cpp", pretty: true);
```

---

## Enumerations

### ValueTypes

Enumeration of all value types.

```csharp
namespace ContainerSystem.Core;

public enum ValueTypes
{
    NullValue = 0,
    BoolValue = 1,
    ShortValue = 2,
    UShortValue = 3,
    IntValue = 4,
    UIntValue = 5,
    LongValue = 6,
    ULongValue = 7,
    LLongValue = 8,
    ULLongValue = 9,
    FloatValue = 10,
    DoubleValue = 11,
    BytesValue = 12,
    StringValue = 13,
    ContainerValue = 14,
    ArrayValue = 15
}
```

#### Helper Methods

```csharp
// Get type from integer
public static ValueTypes FromInt(int value)

// Get integer from type
public static int ToInt(ValueTypes type)

// Get type name
public static string GetTypeName(ValueTypes type)
```

---

## Usage Examples

### Basic Container Operations

```csharp
using ContainerSystem.Core;
using ContainerSystem.Values;

// Create container with metadata
var container = new ValueContainer();
container.MessageType = "user_profile";
container.SetSource("client_app", "session_123");
container.SetTarget("user_service", "profile_handler");

// Add various value types
container.Add(new StringValue("username", "alice"));
container.Add(new IntValue("age", 28));
container.Add(new DoubleValue("balance", 1500.75));
container.Add(new BoolValue("is_active", true));
container.Add(new BytesValue("avatar", avatarData));

// Retrieve values
var username = container.GetValue("username")?.ToString();
var age = container.GetValue("age")?.ToInt() ?? 0;
var balance = container.GetValue("balance")?.ToDouble() ?? 0.0;

// Check statistics
Console.WriteLine($"Values: {container.Count}");
Console.WriteLine($"Reads: {container.ReadCount}");
Console.WriteLine($"Writes: {container.WriteCount}");
```

### Nested Containers

```csharp
// Create nested structure
var address = new ValueContainer();
address.Add(new StringValue("street", "123 Main St"));
address.Add(new StringValue("city", "Seoul"));
address.Add(new StringValue("zip", "12345"));

var contact = new ValueContainer();
contact.Add(new StringValue("email", "alice@example.com"));
contact.Add(new StringValue("phone", "+82-10-1234-5678"));

var user = new ValueContainer();
user.MessageType = "user_data";
user.Add(new StringValue("name", "Alice"));
user.Add(new ContainerValue("address", address));
user.Add(new ContainerValue("contact", contact));

// Access nested values
var addressValue = user.GetValue("address") as ContainerValue;
var city = addressValue?.Container.GetValue("city")?.ToString();
```

### Array Operations

```csharp
// Create array of scores
var scores = new ArrayValue("test_scores");
scores.Append(new IntValue("", 95));
scores.Append(new IntValue("", 87));
scores.Append(new IntValue("", 92));
scores.Append(new IntValue("", 88));

// Calculate average
double sum = 0;
for (int i = 0; i < scores.Count; i++)
{
    sum += scores.At(i).ToInt();
}
double average = sum / scores.Count;

// Mixed-type array for flexible data
var record = new ArrayValue("user_record");
record.Append(new StringValue("", "Alice"));
record.Append(new IntValue("", 28));
record.Append(new DoubleValue("", 1500.75));
record.Append(new BoolValue("", true));

container.Add(scores);
container.Add(record);
```

### Serialization Formats

```csharp
// JSON serialization
string json = container.Serialize();
var fromJson = new ValueContainer(json);

// Binary serialization (via ValueStore)
byte[] binary = container.SerializeArray();
var fromBinary = new ValueContainer(binary);

// XML serialization
string xml = container.ToXml();

// JSON v2.0 for cross-language
string jsonV2 = JsonV2Adapter.ToV2Json(container, pretty: true);
var fromV2 = JsonV2Adapter.FromV2Json(jsonV2);
```

### Thread-Safe Operations

```csharp
var container = new ValueContainer();
container.EnableThreadSafety();

// Concurrent writes from multiple threads
Parallel.For(0, 1000, i =>
{
    container.Add(new IntValue($"value_{i}", i));
});

// Concurrent reads
var results = new ConcurrentBag<int>();
Parallel.For(0, 1000, i =>
{
    var value = container.GetValue($"value_{i}");
    if (value != null)
    {
        results.Add(value.ToInt());
    }
});

Console.WriteLine($"Total values: {container.Count}");
Console.WriteLine($"Results: {results.Count}");
```

### Type Conversions and Validation

```csharp
var value = container.GetValue("user_input");

// Safe type checking
if (value != null)
{
    if (value.IsNumeric())
    {
        int num = value.ToInt();
        Console.WriteLine($"Numeric value: {num}");
    }
    else if (value.IsString())
    {
        string str = value.ToString();
        Console.WriteLine($"String value: {str}");
    }
    else if (value.IsContainer())
    {
        var nested = (value as ContainerValue)?.Container;
        Console.WriteLine($"Nested container with {nested?.Count} values");
    }
}
```

### Cross-Language Communication

```csharp
// Send to C++ service
var request = new ValueContainer();
request.MessageType = "process_data";
request.SetSource("dotnet_client", "worker_1");
request.SetTarget("cpp_service", "processor");
request.Add(new StringValue("operation", "transform"));
request.Add(new IntValue("batch_size", 1000));

// Serialize in C++ compatible format
string cppJson = JsonV2Adapter.ToCppJson(request, pretty: false);
await SendToService(cppJson);

// Receive from Python service
string pythonResponse = await ReceiveFromService();
var format = JsonV2Adapter.DetectFormat(pythonResponse);
ValueContainer response;

if (format == "v2.0")
{
    response = JsonV2Adapter.FromV2Json(pythonResponse);
}
else if (format == "python")
{
    response = new ValueContainer(pythonResponse);
}
else
{
    throw new InvalidOperationException($"Unknown format: {format}");
}
```

### Multiple Values with Same Key

```csharp
// Add multiple tags
container.Add(new StringValue("tag", "important"));
container.Add(new StringValue("tag", "urgent"));
container.Add(new StringValue("tag", "customer-facing"));
container.Add(new StringValue("tag", "needs-review"));

// Retrieve all tags
var tags = container.ValueArray("tag");
Console.WriteLine($"Found {tags.Count} tags:");
foreach (var tag in tags)
{
    Console.WriteLine($"  - {tag.ToString()}");
}

// Filter tags
var urgentTags = tags
    .Where(t => t.ToString().Contains("urgent"))
    .ToList();
```

### Container Copy and Manipulation

```csharp
// Deep copy with values
var copy = container.Copy(containingValues: true);

// Shallow copy (header only)
var headerOnly = container.Copy(containingValues: false);

// Swap source and target
container.SwapHeader();
Console.WriteLine($"New source: {container.SourceId}");
Console.WriteLine($"New target: {container.TargetId}");

// Clear values while keeping header
container.ClearValue();
Console.WriteLine($"Values after clear: {container.Count}");

// Reset to defaults
container.Initialize();
```

### Memory and Performance Monitoring

```csharp
var container = new ValueContainer();

// Track operations
for (int i = 0; i < 1000; i++)
{
    container.Add(new IntValue($"key_{i}", i));
}

// Check statistics
var stats = container.MemoryStats();
Console.WriteLine($"Heap allocations: {stats.heapAllocations}");
Console.WriteLine($"Stack allocations: {stats.stackAllocations}");
Console.WriteLine($"Memory footprint: {container.MemoryFootprint()} bytes");
Console.WriteLine($"Read count: {container.ReadCount}");
Console.WriteLine($"Write count: {container.WriteCount}");
Console.WriteLine($"Serializations: {container.SerializationCount}");

// Reset counters
container.ResetStatistics();
```

---

## See Also

- 📚 [Features Documentation](FEATURES.md) - Complete feature overview
- 📖 [Quick Start Guide](guides/QUICK_START.md) - Getting started in 5 minutes
- ✅ [Best Practices](guides/BEST_PRACTICES.md) - Recommended usage patterns
- 🏗️ [Architecture](ARCHITECTURE.md) - System design and patterns
- 📊 [Benchmarks](performance/BENCHMARKS.md) - Performance metrics
- 🔗 [Compatibility Guide](advanced/COMPATIBILITY.md) - Cross-language compatibility
- ❓ [FAQ](guides/FAQ.md) - Frequently asked questions

---

**Last Updated**: 2025-11-26
