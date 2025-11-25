# API Reference

> **Language:** **English** | [한국어](API_REFERENCE_KO.md)

Complete API documentation for .NET Container System.

---

## Table of Contents

1. [Core Classes](#core-classes)
2. [Value Types](#value-types)
3. [Adapters](#adapters)
4. [Enumerations](#enumerations)

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

Array/list of values.

```csharp
public class ArrayValue : Value
```

#### Methods

```csharp
public void AddItem(Value item)
public Value? GetItem(int index)
public List<Value> GetItems()
public int ItemCount { get; }
```

```csharp
var array = new ArrayValue("numbers");
array.AddItem(new IntValue("", 1));
array.AddItem(new IntValue("", 2));
array.AddItem(new IntValue("", 3));

container.Add(array);
```

---

## Adapters

### JsonV2Adapter

JSON v2.0 format adapter for cross-language compatibility.

```csharp
namespace ContainerSystem.Adapters;

public static class JsonV2Adapter
```

#### Methods

```csharp
// Convert container to JSON v2.0 format
public static string ToJson(ValueContainer container)

// Parse JSON v2.0 format to container
public static ValueContainer FromJson(string json)

// Detect format (v1 or v2)
public static bool IsV2Format(string json)
```

#### Example

```csharp
using ContainerSystem.Adapters;

var container = new ValueContainer();
container.Add(new StringValue("name", "test"));

// To JSON v2.0
string jsonV2 = JsonV2Adapter.ToJson(container);

// From JSON v2.0
var restored = JsonV2Adapter.FromJson(jsonV2);
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

## See Also

- [Features](FEATURES.md) - Feature documentation
- [Quick Start](guides/QUICK_START.md) - Getting started
- [Best Practices](guides/BEST_PRACTICES.md) - Usage patterns
