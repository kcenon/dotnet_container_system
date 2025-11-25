# Frequently Asked Questions (FAQ)

> **Language:** **English** | [한국어](FAQ_KO.md)

Answers to frequently asked questions about .NET Container System.

---

## Table of Contents

1. [General](#general)
2. [Installation & Setup](#installation--setup)
3. [Value Types](#value-types)
4. [Serialization](#serialization)
5. [Thread Safety](#thread-safety)
6. [Cross-Language Compatibility](#cross-language-compatibility)
7. [Performance](#performance)

---

## General

### Q: What is .NET Container System?

**A:** .NET Container System is a type-safe, high-performance container library for message serialization and data storage. It provides 16 distinct value types and supports JSON/Binary serialization with cross-language compatibility.

### Q: How is it different from System.Text.Json or Newtonsoft.Json?

**A:** Unlike general-purpose JSON libraries, Container System:
- Provides a strongly-typed value system (16 types)
- Supports binary serialization for performance
- Ensures cross-language compatibility with C++, Python, Go, Rust
- Includes built-in thread safety
- Supports message routing metadata (source, target, message type)

### Q: Which .NET versions are supported?

**A:** .NET 8.0 and later. The library uses modern C# features like nullable reference types and implicit usings.

### Q: What's the license?

**A:** BSD 3-Clause License. Free for commercial and personal use.

---

## Installation & Setup

### Q: How do I install the package?

**A:**
```bash
# NuGet (when published)
dotnet add package ContainerSystem

# Build from source
git clone https://github.com/kcenon/dotnet_container_system.git
cd dotnet_container_system
dotnet build
```

### Q: Do I need any dependencies?

**A:** No external dependencies required. The library uses only standard .NET libraries.

### Q: How do I add this to my project?

**A:** Add a project reference:
```xml
<ItemGroup>
  <ProjectReference Include="path/to/ContainerSystem/ContainerSystem.csproj" />
</ItemGroup>
```

Or reference the built DLL:
```xml
<ItemGroup>
  <Reference Include="ContainerSystem">
    <HintPath>path/to/ContainerSystem.dll</HintPath>
  </Reference>
</ItemGroup>
```

---

## Value Types

### Q: What value types are supported?

**A:** 16 types are supported:

| Type | .NET Type | Size |
|------|-----------|------|
| NullValue | null | 0 |
| BoolValue | bool | 1 byte |
| ShortValue | short | 2 bytes |
| UShortValue | ushort | 2 bytes |
| IntValue | int | 4 bytes |
| UIntValue | uint | 4 bytes |
| LongValue | int (32-bit range) | 4 bytes |
| ULongValue | uint (32-bit range) | 4 bytes |
| LLongValue | long | 8 bytes |
| ULLongValue | ulong | 8 bytes |
| FloatValue | float | 4 bytes |
| DoubleValue | double | 8 bytes |
| StringValue | string | variable |
| BytesValue | byte[] | variable |
| ContainerValue | ValueContainer | variable |
| ArrayValue | List<Value> | variable |

### Q: Why are LongValue and ULongValue limited to 32-bit range?

**A:** For cross-language compatibility with C++, where `long` is 4 bytes on Windows. This ensures data serialized in .NET can be correctly read by C++ applications. Use `LLongValue` or `ULLongValue` for full 64-bit range.

### Q: How do I store a list of values?

**A:** Use `ArrayValue`:
```csharp
var array = new ArrayValue("numbers");
array.AddItem(new IntValue("", 1));
array.AddItem(new IntValue("", 2));
array.AddItem(new IntValue("", 3));
container.Add(array);
```

Or add multiple values with the same name:
```csharp
container.Add(new IntValue("number", 1));
container.Add(new IntValue("number", 2));
container.Add(new IntValue("number", 3));
var numbers = container.ValueArray("number");
```

### Q: How do I store binary data?

**A:**
```csharp
byte[] data = File.ReadAllBytes("image.png");
container.Add(new BytesValue("image", data));

// Retrieve
byte[] retrieved = container.GetValue("image")?.ToBytes();
```

---

## Serialization

### Q: What serialization formats are supported?

**A:**
- **JSON**: Human-readable, cross-platform
- **Binary**: Fast, compact

```csharp
// JSON
string json = container.Serialize();
var fromJson = new ValueContainer(json);

// Binary
byte[] binary = container.Store.Serialize();
container.Store.Deserialize(binary);
```

### Q: How do I use JSON v2.0 format for C++ compatibility?

**A:**
```csharp
using ContainerSystem.Adapters;

// To JSON v2.0
string jsonV2 = JsonV2Adapter.ToJson(container);

// From JSON v2.0
var restored = JsonV2Adapter.FromJson(jsonV2);
```

### Q: Can I serialize to XML?

**A:** Yes, use `ToXml()` method:
```csharp
string xml = container.ToXml();
```

### Q: How do I handle serialization errors?

**A:**
```csharp
try
{
    string json = container.Serialize();
}
catch (Exception ex)
{
    Console.WriteLine($"Serialization failed: {ex.Message}");
}
```

---

## Thread Safety

### Q: Is ValueContainer thread-safe?

**A:** Yes, `ValueContainer` uses internal locking for thread-safe operations.

### Q: How do I enable thread-safe mode for ValueStore?

**A:**
```csharp
var store = new ValueStore(threadSafe: true);
```

### Q: Can I use containers in parallel operations?

**A:** Yes:
```csharp
var container = new ValueContainer();
Parallel.For(0, 100, i =>
{
    container.Add(new IntValue($"value_{i}", i));
});
```

### Q: How do I safely iterate while modifying?

**A:** Take a snapshot first:
```csharp
var values = container.Values().ToList();
foreach (var value in values)
{
    // Safe to modify container
}
```

---

## Cross-Language Compatibility

### Q: Can I exchange data with C++ container_system?

**A:** Yes, using JSON v2.0 Adapter:
```csharp
// .NET to C++
string json = JsonV2Adapter.ToJson(container);
// Send to C++ application

// C++ to .NET
var container = JsonV2Adapter.FromJson(cppJsonString);
```

### Q: Which languages are compatible?

**A:**
- C++ (container_system)
- Python (python_container_system)
- Go, Rust, Node.js (via JSON v2.0 format)

### Q: How do I handle platform-specific type differences?

**A:**
- Use `IntValue` for 32-bit integers (universal)
- Use `LLongValue` for 64-bit integers
- Use `DoubleValue` for floating point
- Avoid platform-specific types like `long` (varies by platform)

---

## Performance

### Q: What's the performance of serialization?

**A:** Approximate benchmarks on modern hardware:
- Binary serialization: ~500K ops/sec
- JSON serialization: ~200K ops/sec
- Container creation: ~1M ops/sec

### Q: How can I improve performance?

**A:**
1. Use binary serialization for speed
2. Reuse containers instead of creating new ones
3. Use `ValueArray` for bulk retrieval
4. Enable thread-safe mode only when needed

### Q: Is there memory pooling?

**A:** The current version doesn't include memory pooling. Consider implementing object pooling at the application level for high-throughput scenarios.

### Q: How does it compare to the C++ version?

**A:** The C++ version with SIMD optimization is faster (1.8M ops/sec for binary serialization). The .NET version prioritizes ease of use and cross-platform compatibility.

---

## More Questions?

- **GitHub Issues**: [Report a bug or request a feature](https://github.com/kcenon/dotnet_container_system/issues)
- **GitHub Discussions**: [Ask questions](https://github.com/kcenon/dotnet_container_system/discussions)
- **Email**: kcenon@naver.com
