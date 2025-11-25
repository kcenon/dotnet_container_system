# Troubleshooting Guide

> **Language:** **English** | [한국어](TROUBLESHOOTING_KO.md)

Solutions to common issues when using .NET Container System.

---

## Table of Contents

1. [Build Issues](#build-issues)
2. [Runtime Errors](#runtime-errors)
3. [Serialization Problems](#serialization-problems)
4. [Type Conversion Issues](#type-conversion-issues)
5. [Thread Safety Issues](#thread-safety-issues)
6. [Cross-Language Compatibility](#cross-language-compatibility)
7. [Performance Issues](#performance-issues)

---

## Build Issues

### SDK Not Found

**Error:**
```
The SDK 'Microsoft.NET.Sdk' specified could not be found.
```

**Solution:**
```bash
# Check installed SDKs
dotnet --list-sdks

# Install .NET 8 SDK
# Windows
winget install Microsoft.DotNet.SDK.8

# macOS
brew install --cask dotnet-sdk

# Linux (Ubuntu)
sudo apt-get install dotnet-sdk-8.0
```

### Package Restore Failed

**Error:**
```
Unable to resolve package...
```

**Solution:**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore with verbose output
dotnet restore --verbosity detailed
```

### Project Reference Not Found

**Error:**
```
Project reference '../ContainerSystem/ContainerSystem.csproj' could not be found.
```

**Solution:**
1. Ensure you're in the correct directory
2. Check that `ContainerSystem.csproj` exists
3. Restore the solution:
```bash
cd dotnet_container_system
dotnet restore dotnet-container-system.sln
```

---

## Runtime Errors

### NullReferenceException When Reading Values

**Error:**
```
System.NullReferenceException: Object reference not set to an instance of an object.
```

**Cause:** Accessing a value that doesn't exist.

**Solution:**
```csharp
// Bad
string name = container.GetValue("name").ToString();

// Good
var value = container.GetValue("name");
if (value != null)
{
    string name = value.ToString();
}

// Better - using null-conditional operator
string name = container.GetValue("name")?.ToString() ?? "default";
```

### InvalidCastException

**Error:**
```
System.InvalidCastException: Unable to cast object...
```

**Cause:** Wrong type cast.

**Solution:**
```csharp
// Bad
var intVal = (IntValue)container.GetValue("price"); // price is DoubleValue!

// Good - check type first
var value = container.GetValue("price");
if (value is DoubleValue doubleVal)
{
    double price = doubleVal.ToDouble();
}
```

### OverflowException with LongValue

**Error:**
```
System.OverflowException: Value was either too large or too small for an Int32.
```

**Cause:** LongValue enforces 32-bit range for C++ compatibility.

**Solution:**
```csharp
// Bad - value exceeds Int32 range
container.Add(new LongValue("big_number", 5_000_000_000L));

// Good - use LLongValue for 64-bit values
container.Add(new LLongValue("big_number", 5_000_000_000L));
```

---

## Serialization Problems

### JSON Deserialization Fails

**Error:**
```
Failed to deserialize container from JSON
```

**Possible Causes:**
1. Malformed JSON
2. Missing required fields
3. Type mismatch

**Solution:**
```csharp
// Validate JSON before deserializing
try
{
    var container = new ValueContainer(jsonString);
}
catch (Exception ex)
{
    Console.WriteLine($"Invalid JSON: {ex.Message}");

    // Try to parse as raw JSON for debugging
    try
    {
        var json = System.Text.Json.JsonDocument.Parse(jsonString);
        Console.WriteLine("JSON is valid but incompatible with container format");
    }
    catch
    {
        Console.WriteLine("JSON is malformed");
    }
}
```

### Binary Deserialization Fails

**Error:**
```
Failed to deserialize binary data
```

**Cause:** Corrupted or incompatible binary data.

**Solution:**
```csharp
// Check data length
if (binaryData == null || binaryData.Length < 4)
{
    throw new ArgumentException("Invalid binary data");
}

// Verify header
try
{
    var container = new ValueContainer();
    container.Store.Deserialize(binaryData);
}
catch (Exception ex)
{
    Console.WriteLine($"Deserialization failed: {ex.Message}");
}
```

### C++ Binary Incompatibility

**Error:**
Data serialized in C++ cannot be read in .NET.

**Solution:**
Use JSON v2.0 Adapter for cross-language compatibility:
```csharp
using ContainerSystem.Adapters;

// Serialize in compatible format
string jsonV2 = JsonV2Adapter.ToJson(container);

// Deserialize from C++ JSON
var restored = JsonV2Adapter.FromJson(cppJsonString);
```

---

## Type Conversion Issues

### Numeric Conversion Fails

**Error:**
```
Input string was not in a correct format.
```

**Solution:**
```csharp
// Use safe conversion
var value = container.GetValue("number");
if (value != null)
{
    try
    {
        int num = value.ToInt();
    }
    catch (FormatException)
    {
        Console.WriteLine($"Cannot convert '{value.ToString()}' to int");
    }
}
```

### Boolean Conversion

**Issue:** Non-standard boolean values.

**Solution:**
```csharp
// Standard boolean conversion
var value = container.GetValue("flag");
bool flag = value?.ToBoolean() ?? false;

// Custom string to boolean
string strValue = value?.ToString()?.ToLower();
bool customFlag = strValue == "yes" || strValue == "1" || strValue == "true";
```

---

## Thread Safety Issues

### Race Condition

**Symptom:** Inconsistent data or exceptions during concurrent access.

**Solution:**
```csharp
// Enable thread-safe mode
var store = new ValueStore(threadSafe: true);

// Or use locks manually
private readonly object _lock = new();

lock (_lock)
{
    container.Add(new IntValue("counter", count));
}
```

### Deadlock

**Symptom:** Application hangs during concurrent operations.

**Solution:**
1. Avoid nested locks
2. Use timeout-based locks:
```csharp
private readonly ReaderWriterLockSlim _rwLock = new();

if (_rwLock.TryEnterWriteLock(TimeSpan.FromSeconds(5)))
{
    try
    {
        // Write operation
    }
    finally
    {
        _rwLock.ExitWriteLock();
    }
}
else
{
    throw new TimeoutException("Could not acquire write lock");
}
```

---

## Cross-Language Compatibility

### Long Type Mismatch

**Issue:** C++ `long` is 4 bytes on Windows, 8 bytes on Linux.

**Solution:**
```csharp
// Use explicit types
// For 32-bit values (C++ int/long on Windows)
container.Add(new IntValue("small_num", 1000));
container.Add(new LongValue("medium_num", 1000000)); // Enforces 32-bit range

// For 64-bit values (C++ long long)
container.Add(new LLongValue("large_num", 5_000_000_000L));
```

### String Encoding

**Issue:** String encoding mismatch.

**Solution:**
```csharp
// Always use UTF-8
var bytes = System.Text.Encoding.UTF8.GetBytes(text);
container.Add(new BytesValue("text_bytes", bytes));

// Or use StringValue which handles UTF-8 automatically
container.Add(new StringValue("text", text));
```

---

## Performance Issues

### Slow Serialization

**Solution:**
1. Use binary format for performance-critical paths
2. Avoid unnecessary serialization/deserialization
3. Reuse containers

```csharp
// Binary is faster than JSON
byte[] binary = container.Store.Serialize();

// Reuse containers
container.Clear();
// Add new values instead of creating new container
```

### High Memory Usage

**Solution:**
1. Clear containers when done
2. Use value types appropriately
3. Avoid storing large binary data if possible

```csharp
// Clear when done
container.Clear();

// Use streaming for large data
// Instead of loading entire file into BytesValue
```

### Slow Value Lookup

**Solution:**
Use `ValueArray` for multiple values with same name:
```csharp
// Bad - multiple lookups
var tag1 = container.GetValue("tag");

// Good - single lookup for all
var tags = container.ValueArray("tag");
```

---

## Getting More Help

If your issue isn't covered here:

1. **Check the FAQ**: [FAQ.md](FAQ.md)
2. **Search Issues**: [GitHub Issues](https://github.com/kcenon/dotnet_container_system/issues)
3. **Ask a Question**: [GitHub Discussions](https://github.com/kcenon/dotnet_container_system/discussions)
4. **Email**: kcenon@naver.com

When reporting issues, please include:
- .NET version (`dotnet --version`)
- OS and architecture
- Minimal reproducible code
- Full error message and stack trace
