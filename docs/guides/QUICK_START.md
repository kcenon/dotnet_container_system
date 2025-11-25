# Quick Start Guide

> **Language:** **English** | [한국어](QUICK_START_KO.md)

Get started with .NET Container System in 5 minutes!

---

## Prerequisites

- **.NET SDK**: 8.0 or later
- **IDE**: Visual Studio 2022, VS Code, or JetBrains Rider

## Installation

### Option 1: NuGet Package (Recommended)

```bash
dotnet add package ContainerSystem
```

### Option 2: Build from Source

```bash
# Clone repository
git clone https://github.com/kcenon/dotnet_container_system.git
cd dotnet_container_system

# Build
dotnet build

# Run tests
dotnet test
```

---

## Your First Container

### Step 1: Create a Container

```csharp
using ContainerSystem.Core;
using ContainerSystem.Values;

// Create a new container
var container = new ValueContainer();
container.MessageType = "user_profile";
```

### Step 2: Add Values

```csharp
// Add various value types
container.Add(new StringValue("username", "john_doe"));
container.Add(new IntValue("age", 30));
container.Add(new BoolValue("is_active", true));
container.Add(new DoubleValue("balance", 1500.75));
```

### Step 3: Read Values

```csharp
// Read values with type safety
var username = container.GetValue("username");
Console.WriteLine($"Username: {username?.ToString()}");

var age = container.GetValue("age");
Console.WriteLine($"Age: {age?.ToInt()}");
```

### Step 4: Serialize and Deserialize

```csharp
// Serialize to JSON
string json = container.Serialize();
Console.WriteLine($"Serialized: {json}");

// Deserialize from JSON
var restored = new ValueContainer(json);
Console.WriteLine($"Restored MessageType: {restored.MessageType}");
```

---

## Complete Example

```csharp
using ContainerSystem.Core;
using ContainerSystem.Values;

class Program
{
    static void Main()
    {
        // Create container with metadata
        var container = new ValueContainer();
        container.MessageType = "order";
        container.SetSource("client_app", "session_001");
        container.SetTarget("order_service", "handler");

        // Add order details
        container.Add(new StringValue("order_id", "ORD-12345"));
        container.Add(new StringValue("product", "Widget Pro"));
        container.Add(new IntValue("quantity", 5));
        container.Add(new DoubleValue("unit_price", 29.99));
        container.Add(new DoubleValue("total", 149.95));
        container.Add(new BoolValue("express_shipping", true));

        // Serialize
        string serialized = container.Serialize();

        // Deserialize and read
        var restored = new ValueContainer(serialized);

        Console.WriteLine($"Order: {restored.GetValue("order_id")?.ToString()}");
        Console.WriteLine($"Product: {restored.GetValue("product")?.ToString()}");
        Console.WriteLine($"Quantity: {restored.GetValue("quantity")?.ToInt()}");
        Console.WriteLine($"Total: ${restored.GetValue("total")?.ToDouble():F2}");
    }
}
```

---

## Multiple Values with Same Name

```csharp
// Add multiple tags
container.Add(new StringValue("tag", "electronics"));
container.Add(new StringValue("tag", "sale"));
container.Add(new StringValue("tag", "featured"));

// Retrieve all tags
var tags = container.ValueArray("tag");
foreach (var tag in tags)
{
    Console.WriteLine($"Tag: {tag.ToString()}");
}
```

---

## Nested Containers

```csharp
// Create nested container
var address = new ValueContainer();
address.Add(new StringValue("street", "123 Main St"));
address.Add(new StringValue("city", "Seoul"));
address.Add(new StringValue("country", "Korea"));

// Add as nested container
container.Add(new ContainerValue("shipping_address", address));

// Access nested values
var addressContainer = container.GetValue("shipping_address") as ContainerValue;
var city = addressContainer?.Container?.GetValue("city");
Console.WriteLine($"City: {city?.ToString()}");
```

---

## Thread-Safe Operations

```csharp
var container = new ValueContainer();

// Multiple threads can safely add values
Parallel.For(0, 100, i =>
{
    container.Add(new IntValue($"value_{i}", i));
});

Console.WriteLine($"Total values: {container.Count}");
```

---

## Next Steps

- [Build Guide](BUILD_GUIDE.md) - Detailed build instructions
- [Best Practices](BEST_PRACTICES.md) - Recommended patterns
- [API Reference](../API_REFERENCE.md) - Complete API documentation
- [FAQ](FAQ.md) - Frequently asked questions

---

## Common Issues

| Issue | Solution |
|-------|----------|
| NullReferenceException when reading | Always check if `GetValue()` returns null |
| Type conversion fails | Use appropriate conversion method (ToInt, ToDouble, etc.) |
| Serialization error | Ensure all values are properly initialized |

For more troubleshooting, see [Troubleshooting Guide](TROUBLESHOOTING.md).
