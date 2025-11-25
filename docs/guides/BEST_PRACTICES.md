# Best Practices

> **Language:** **English** | [한국어](BEST_PRACTICES_KO.md)

Recommended patterns and practices for using .NET Container System effectively.

---

## Table of Contents

1. [Container Creation](#container-creation)
2. [Value Handling](#value-handling)
3. [Serialization](#serialization)
4. [Thread Safety](#thread-safety)
5. [Error Handling](#error-handling)
6. [Performance](#performance)
7. [Cross-Language Compatibility](#cross-language-compatibility)

---

## Container Creation

### Do: Set Meaningful Metadata

```csharp
// Good: Clear identification
var container = new ValueContainer();
container.MessageType = "user_registration";
container.SetSource("web_client", "session_abc123");
container.SetTarget("auth_service", "registration_handler");
```

### Don't: Skip Metadata

```csharp
// Bad: No context
var container = new ValueContainer();
container.Add(new StringValue("data", "value"));
```

### Do: Use Descriptive Value Names

```csharp
// Good: Self-documenting
container.Add(new StringValue("user_email", "user@example.com"));
container.Add(new IntValue("login_attempt_count", 3));
container.Add(new DoubleValue("account_balance_usd", 1500.00));
```

### Don't: Use Cryptic Names

```csharp
// Bad: Unclear meaning
container.Add(new StringValue("e", "user@example.com"));
container.Add(new IntValue("cnt", 3));
container.Add(new DoubleValue("bal", 1500.00));
```

---

## Value Handling

### Do: Check for Null Before Use

```csharp
// Good: Safe access
var value = container.GetValue("username");
if (value != null)
{
    string username = value.ToString();
    // Process username
}
```

### Do: Use Type-Appropriate Conversions

```csharp
// Good: Correct type usage
var intValue = container.GetValue("count");
int count = intValue?.ToInt() ?? 0;

var doubleValue = container.GetValue("price");
double price = doubleValue?.ToDouble() ?? 0.0;
```

### Don't: Assume Values Exist

```csharp
// Bad: May throw NullReferenceException
string username = container.GetValue("username").ToString();
```

### Do: Use 64-bit Types for Large Numbers

```csharp
// Good: Use LLongValue for large numbers
container.Add(new LLongValue("file_size_bytes", 5_000_000_000L));
container.Add(new ULLongValue("total_records", 10_000_000_000UL));
```

### Don't: Use LongValue for Values > 2^31

```csharp
// Bad: LongValue enforces 32-bit range for C++ compatibility
container.Add(new LongValue("big_number", 5_000_000_000L)); // Throws OverflowException!
```

---

## Serialization

### Do: Handle Serialization Errors

```csharp
// Good: Error handling
try
{
    string json = container.Serialize();
    // Send or store json
}
catch (Exception ex)
{
    Console.WriteLine($"Serialization failed: {ex.Message}");
}
```

### Do: Validate After Deserialization

```csharp
// Good: Validate restored data
var restored = new ValueContainer(jsonData);
if (string.IsNullOrEmpty(restored.MessageType))
{
    throw new InvalidDataException("Missing message type");
}
```

### Do: Use JSON v2.0 for Cross-Language

```csharp
// Good: Cross-language compatible format
using ContainerSystem.Adapters;

string jsonV2 = JsonV2Adapter.ToJson(container);
var restored = JsonV2Adapter.FromJson(jsonV2);
```

---

## Thread Safety

### Do: Use Thread-Safe Mode for Concurrent Access

```csharp
// Good: Enable thread-safe mode
var store = new ValueStore(threadSafe: true);
```

### Do: Use Parallel.For for Bulk Operations

```csharp
// Good: Parallel processing
var container = new ValueContainer();
Parallel.For(0, 1000, i =>
{
    container.Add(new IntValue($"item_{i}", i));
});
```

### Don't: Modify While Iterating

```csharp
// Bad: May cause issues
foreach (var value in container.Values())
{
    container.Add(new IntValue("new_value", 1)); // Don't do this!
}
```

### Do: Get Snapshot for Safe Iteration

```csharp
// Good: Take a snapshot
var values = container.Values().ToList();
foreach (var value in values)
{
    // Safe to modify container here
}
```

---

## Error Handling

### Do: Use Try-Parse Pattern

```csharp
// Good: Safe parsing
public bool TryGetInt(ValueContainer container, string key, out int result)
{
    result = 0;
    var value = container.GetValue(key);
    if (value == null) return false;

    try
    {
        result = value.ToInt();
        return true;
    }
    catch
    {
        return false;
    }
}
```

### Do: Provide Default Values

```csharp
// Good: Default value pattern
int count = container.GetValue("count")?.ToInt() ?? 0;
string name = container.GetValue("name")?.ToString() ?? "Unknown";
bool active = container.GetValue("active")?.ToBoolean() ?? false;
```

### Do: Log Errors with Context

```csharp
// Good: Contextual logging
try
{
    ProcessContainer(container);
}
catch (Exception ex)
{
    Console.WriteLine($"Error processing container " +
        $"[Type: {container.MessageType}, " +
        $"Source: {container.SourceId}]: {ex.Message}");
}
```

---

## Performance

### Do: Reuse Containers When Possible

```csharp
// Good: Clear and reuse
container.Clear();
container.MessageType = "new_message";
container.Add(new StringValue("data", "new_data"));
```

### Do: Use Binary Serialization for Speed

```csharp
// Good: Binary for performance-critical paths
byte[] binary = container.Store.Serialize();
// Binary is faster than JSON
```

### Don't: Create Containers in Hot Loops

```csharp
// Bad: Creates garbage
for (int i = 0; i < 1000000; i++)
{
    var temp = new ValueContainer();
    temp.Add(new IntValue("x", i));
    // Process and discard
}

// Good: Reuse container
var container = new ValueContainer();
for (int i = 0; i < 1000000; i++)
{
    container.Clear();
    container.Add(new IntValue("x", i));
    // Process
}
```

### Do: Use ValueArray for Multiple Same-Name Values

```csharp
// Good: Efficient retrieval
var tags = container.ValueArray("tag");
// Instead of multiple GetValue calls
```

---

## Cross-Language Compatibility

### Do: Use Standard Types

```csharp
// Good: Types that work across languages
container.Add(new IntValue("int32_value", 42));           // 32-bit signed
container.Add(new LLongValue("int64_value", 123456789L)); // 64-bit signed
container.Add(new DoubleValue("float64_value", 3.14159)); // 64-bit float
container.Add(new StringValue("string_value", "hello"));  // UTF-8 string
container.Add(new BytesValue("binary_value", bytes));     // Binary data
```

### Do: Document Type Constraints

```csharp
/// <summary>
/// User ID must be within Int32 range for C++ compatibility.
/// </summary>
public void SetUserId(ValueContainer container, long userId)
{
    if (userId < int.MinValue || userId > int.MaxValue)
    {
        throw new ArgumentOutOfRangeException(nameof(userId),
            "User ID must be within Int32 range for cross-language compatibility");
    }
    container.Add(new IntValue("user_id", (int)userId));
}
```

### Do: Use JSON v2.0 Adapter for Interchange

```csharp
// Good: Cross-language compatible serialization
string jsonV2 = JsonV2Adapter.ToJson(container);

// This JSON can be read by:
// - C++ container_system
// - Python python_container_system
// - Go, Rust, Node.js implementations
```

---

## Summary

| Category | Key Practice |
|----------|--------------|
| **Creation** | Set meaningful metadata and descriptive names |
| **Values** | Always check for null, use correct types |
| **Serialization** | Handle errors, validate after deserialize |
| **Thread Safety** | Use thread-safe mode for concurrent access |
| **Error Handling** | Provide defaults, log with context |
| **Performance** | Reuse containers, avoid hot-loop allocations |
| **Compatibility** | Use standard types, document constraints |

---

## See Also

- [Quick Start](QUICK_START.md) - Getting started
- [Troubleshooting](TROUBLESHOOTING.md) - Common issues
- [API Reference](../API_REFERENCE.md) - Complete API documentation
