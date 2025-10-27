# .NET Container System

A high-performance container system for message serialization and type-safe value storage. This is the .NET equivalent of the C++ container_system.

## Features

- **Type-Safe Value Storage**: Support for 15 different value types (null, bool, numeric types, strings, bytes, containers)
- **Message Container**: High-level container with source/target IDs and message types
- **Serialization**: JSON and XML serialization support
- **Thread-Safe**: Thread-safe operations for concurrent access
- **Type Conversions**: Safe type conversions with null checking
- **Nested Containers**: Support for hierarchical container structures
- **Multiple Values**: Support for multiple values with the same name

## Architecture

```
dotnet_container_system/
├── ContainerSystem/              # Main library
│   ├── Core/                     # Core abstractions
│   │   ├── ValueTypes.cs         # Value type enumeration
│   │   ├── Value.cs              # Abstract base class
│   │   └── ValueContainer.cs     # Message container
│   └── Values/                   # Concrete value types
│       ├── StringValue.cs        # String values
│       └── NumericValue.cs       # Numeric values (int, long, double, float, bool, bytes)
├── ContainerSystem.Examples/     # Usage examples
│   └── BasicUsage.cs
└── ContainerSystem.Tests/        # Unit tests
```

## Installation

### NuGet Package (Future)

```bash
dotnet add package ContainerSystem
```

### Build from Source

```bash
git clone https://github.com/kcenon/dotnet_container_system.git
cd dotnet_container_system
dotnet build
```

## Quick Start

### Basic Usage

```csharp
using ContainerSystem.Core;
using ContainerSystem.Values;

// Create a container
var container = new ValueContainer();
container.MessageType = "user_profile";

// Add values
container.Add(new StringValue("user_id", "12345"));
container.Add(new StringValue("username", "john_doe"));
container.Add(new IntValue("age", 30));
container.Add(new BoolValue("is_active", true));
container.Add(new DoubleValue("balance", 1000.50));

// Read values
var username = container.GetValue("username");
if (username != null)
{
    Console.WriteLine($"Username: {username.ToString()}");
}

var age = container.GetValue("age");
if (age != null)
{
    Console.WriteLine($"Age: {age.ToInt()}");
}
```

### Multiple Values with Same Name

```csharp
// Add multiple tags
container.Add(new StringValue("tag", "csharp"));
container.Add(new StringValue("tag", "dotnet"));
container.Add(new StringValue("tag", "example"));

// Retrieve all tags
var tags = container.ValueArray("tag");
foreach (var tag in tags)
{
    Console.WriteLine($"Tag: {tag.ToString()}");
}
```

### Serialization

```csharp
// Serialize to JSON
string serialized = container.Serialize();
Console.WriteLine($"Serialized: {serialized}");

// Deserialize from JSON
var restored = new ValueContainer(serialized);
Console.WriteLine($"Message Type: {restored.MessageType}");
```

### Container Metadata

```csharp
// Set source and target
container.SetSource("client_app", "user_session_123");
container.SetTarget("server_api", "profile_handler");

Console.WriteLine($"Source: {container.SourceId}/{container.SourceSubId}");
Console.WriteLine($"Target: {container.TargetId}/{container.TargetSubId}");
```

## API Reference

### ValueTypes Enum

```csharp
public enum ValueTypes
{
    NullValue = 0,        // Null value
    BoolValue = 1,        // Boolean
    ShortValue = 2,       // 16-bit signed integer
    UShortValue = 3,      // 16-bit unsigned integer
    IntValue = 4,         // 32-bit signed integer
    UIntValue = 5,        // 32-bit unsigned integer
    LongValue = 6,        // 64-bit signed integer
    ULongValue = 7,       // 64-bit unsigned integer
    LLongValue = 8,       // 64-bit signed integer (same as Long)
    ULLongValue = 9,      // 64-bit unsigned integer
    FloatValue = 10,      // 32-bit floating point
    DoubleValue = 11,     // 64-bit floating point
    BytesValue = 12,      // Binary data
    StringValue = 13,     // String data
    ContainerValue = 14   // Nested container
}
```

### Value Base Class

**Properties:**
- `string Name` - Name of the value
- `ValueTypes Type` - Type of the value
- `Value? Parent` - Parent value (if nested)
- `int ChildCount` - Number of child values

**Methods:**
- `List<Value> Children(bool onlyContainer = false)` - Get child values
- `List<Value> ValueArray(string key)` - Get values by name
- `void AddChild(Value child)` - Add a child value
- `bool IsNull()` / `IsBytes()` / `IsBoolean()` / `IsNumeric()` / `IsString()` / `IsContainer()` - Type checks
- `bool ToBoolean()` / `int ToInt()` / `long ToLong()` / `float ToFloat()` / `double ToDouble()` / `string ToString()` / `byte[] ToBytes()` - Type conversions
- `string Data()` - Get raw data as string
- `int Size()` - Get size in bytes
- `byte[] Serialize()` - Serialize to bytes
- `string ToJson()` / `ToXml()` - Serialize to JSON/XML

### ValueContainer Class

**Properties:**
- `string MessageType` - Type of the message
- `string SourceId` / `SourceSubId` - Source identification
- `string TargetId` / `TargetSubId` - Target identification
- `string Version` - Container version
- `int Count` - Number of values

**Methods:**
- `void SetSource(string sourceId, string sourceSubId = "")` - Set source IDs
- `void SetTarget(string targetId, string targetSubId = "")` - Set target IDs
- `void Add(Value value)` - Add a value
- `Value? GetValue(string key)` - Get single value by name
- `List<Value> ValueArray(string key)` - Get all values with name
- `List<Value> Values()` - Get all values
- `string Serialize()` - Serialize to JSON
- `string ToXml()` - Serialize to XML

### Typed Value Classes

- `StringValue(string name, string value)` - String values
- `IntValue(string name, int value)` - 32-bit integers
- `LongValue(string name, long value)` - 64-bit integers
- `DoubleValue(string name, double value)` - Double precision floats
- `FloatValue(string name, float value)` - Single precision floats
- `BoolValue(string name, bool value)` - Boolean values
- `BytesValue(string name, byte[] value)` - Binary data

## Type Conversion

All value types support safe type conversion:

```csharp
var stringValue = new StringValue("number", "42");
int number = stringValue.ToInt();        // 42
double dbl = stringValue.ToDouble();     // 42.0

var intValue = new IntValue("count", 100);
string str = intValue.ToString();        // "100"
bool flag = intValue.ToBoolean();        // true (non-zero)
```

## Comparison with C++ Version

| Feature | C++ | .NET | Status |
|---------|-----|------|--------|
| Value Types | `value_types` enum | `ValueTypes` enum | ✅ Complete |
| Base Value | `value` class | `Value` class | ✅ Complete |
| Container | `value_container` class | `ValueContainer` class | ✅ Complete |
| String Values | `string_value` | `StringValue` | ✅ Complete |
| Numeric Values | `numeric_value` | `NumericValue` classes | ✅ Complete |
| Boolean Values | `bool_value` | `BoolValue` | ✅ Complete |
| Binary Data | `bytes_value` | `BytesValue` | ✅ Complete |
| Serialization | Custom binary + JSON | JSON + XML | ✅ Complete |
| Type Safety | Template-based | Generic + Type hints | ✅ Complete |
| Thread Safety | `std::mutex` | `lock` statement | ✅ Complete |
| Nested Containers | ✅ | ✅ | ✅ Complete |
| Multiple Values | ✅ | ✅ | ✅ Complete |

## Thread Safety

The `ValueContainer` class uses internal locking to ensure thread-safe operations:

```csharp
// Thread-safe operations
var container = new ValueContainer();

// Multiple threads can safely add values
Parallel.For(0, 100, i =>
{
    container.Add(new IntValue($"value_{i}", i));
});

// Thread-safe retrieval
var values = container.Values();
```

## Examples

See the `ContainerSystem.Examples` project for complete working examples:

- `BasicUsage.cs` - Comprehensive example covering all features

Run examples:
```bash
cd ContainerSystem.Examples
dotnet run
```

## Testing

Run unit tests:
```bash
cd ContainerSystem.Tests
dotnet test
```

## License

BSD 3-Clause License. See LICENSE file for details.

## Related Projects

- `container_system` (C++) - Original C++ implementation
- `python_container_system` - Python equivalent
- `python_database_system` - Python database abstraction layer

## Contributing

Contributions are welcome! Please ensure:
- Code follows .NET coding conventions
- All tests pass
- Documentation is updated
- Commit messages are clear and descriptive

## Repository

**GitHub**: https://github.com/kcenon/dotnet_container_system

## Author

kcenon (kcenon@naver.com)
