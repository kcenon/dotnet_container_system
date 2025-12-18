[![CI](https://github.com/kcenon/dotnet_container_system/actions/workflows/ci.yml/badge.svg)](https://github.com/kcenon/dotnet_container_system/actions/workflows/ci.yml)
[![Code Coverage](https://github.com/kcenon/dotnet_container_system/actions/workflows/coverage.yml/badge.svg)](https://github.com/kcenon/dotnet_container_system/actions/workflows/coverage.yml)

# .NET Container System

> **Language:** **English** | [한국어](README_KO.md)

## Overview

A high-performance, type-safe container system for message serialization and data storage. This is the .NET equivalent of the C++ [container_system](https://github.com/kcenon/container_system), providing cross-language compatibility for enterprise messaging applications.

**Key Highlights**:
- **Type Safety**: 16 distinct value types with compile-time checking
- **Cross-Language**: Binary-compatible with C++, Python, Go, Rust, Node.js
- **Thread-Safe**: Built-in concurrent access with ReaderWriterLockSlim
- **High Performance**: O(1) key lookups, efficient serialization
- **Multiple Formats**: Binary, JSON, XML serialization

### Mission

Making cross-platform data serialization **type-safe**, **efficient**, and **interoperable** for .NET developers.

## Quick Start

### Basic Usage Example

```csharp
using ContainerSystem.Core;
using ContainerSystem.Values;

// Create container with metadata
var container = new ValueContainer();
container.MessageType = "user_profile";
container.SetSource("client_app", "session_123");
container.SetTarget("user_service", "handler");

// Add typed values
container.Add(new StringValue("username", "john_doe"));
container.Add(new IntValue("age", 30));
container.Add(new DoubleValue("balance", 1500.75));
container.Add(new BoolValue("is_active", true));

// Serialize to JSON
string json = container.Serialize();

// Deserialize
var restored = new ValueContainer(json);
var username = restored.GetValue("username")?.ToString();
```

### Fluent Builder Pattern

```csharp
using ContainerSystem.Messaging;

// Create container using fluent builder
var container = new ContainerBuilder()
    .WithSource("client_app", "session_123")
    .WithTarget("user_service", "handler")
    .WithMessageType("user_profile")
    .WithValue(new StringValue("username", "john_doe"))
    .WithValue(new IntValue("age", 30))
    .WithThreadSafety()
    .Build();

// Or use convenience factory methods
var request = ContainerBuilder
    .CreateRequest("client", "server")
    .WithValue(new StringValue("action", "login"))
    .Build();
```

### Prerequisites

- **.NET SDK**: 8.0 or later
- **IDE**: Visual Studio 2022, VS Code, or JetBrains Rider

### Installation

```bash
# NuGet Package (when published)
dotnet add package ContainerSystem

# Or build from source
git clone https://github.com/kcenon/dotnet_container_system.git
cd dotnet_container_system
./scripts/build.sh      # Linux/macOS
# or
scripts\build.bat       # Windows (CMD)
.\scripts\build.ps1     # Windows (PowerShell)
```

## Core Features

### Type-Safe Value System
- **16 built-in types**: From null to nested containers
- **Compile-time checks**: Strong typing prevents runtime errors
- **Safe conversions**: ToInt(), ToDouble(), ToString() with null handling

### Multiple Serialization Formats
- **Binary**: Fast, compact serialization
- **JSON**: Human-readable, debugging-friendly
- **XML**: Structured format for enterprise systems
- **JSON v2.0**: Cross-language compatible format
- **Wire Protocol**: C++ native protocol for direct interop

### Thread Safety
- **ReaderWriterLockSlim**: Efficient concurrent access
- **Thread-safe operations**: Add, Get, Serialize safely from multiple threads
- **Statistics tracking**: Read/write/serialization counts

### Cross-Language Compatibility
- **C++ compatible**: Works with container_system
- **Python compatible**: Works with python_container_system
- **Universal JSON**: JSON v2.0 adapter for any language

📚 **[Complete Features →](docs/FEATURES.md)**

## Performance

| Operation | Throughput | Notes |
|-----------|------------|-------|
| Container Creation | ~1M/sec | Empty container |
| Value Addition | ~2M/sec | Single value |
| JSON Serialization | ~200K/sec | 10 values |
| Binary Serialization | ~500K/sec | 10 values |
| Value Lookup | ~5M/sec | By key |

⚡ **[Full Benchmarks →](docs/performance/BENCHMARKS.md)**

## Architecture

```
                    ┌─────────────────┐
                    │ common_system   │
                    │   (Shared)      │
                    └────────┬────────┘
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
         ▼                   ▼                   ▼
┌────────────────┐  ┌────────────────┐  ┌────────────────┐
│container_system│  │   .NET equiv   │  │ Python equiv   │
│     (C++)      │  │  (This Project)│  │    (Python)    │
└────────────────┘  └────────────────┘  └────────────────┘
```

🏗️ **[Architecture Guide →](docs/ARCHITECTURE.md)**

## Documentation

### Getting Started
- 📖 [Quick Start Guide](docs/guides/QUICK_START.md)
- 🔧 [Build Guide](docs/guides/BUILD_GUIDE.md)
- ✅ [Best Practices](docs/guides/BEST_PRACTICES.md)
- 🔍 [Troubleshooting](docs/guides/TROUBLESHOOTING.md)

### Core Documentation
- 📚 [Features](docs/FEATURES.md) - Complete feature documentation
- ⚡ [Benchmarks](docs/performance/BENCHMARKS.md) - Performance analysis
- 📦 [Project Structure](docs/PROJECT_STRUCTURE.md) - Code organization
- 📘 [API Reference](docs/API_REFERENCE.md) - Complete API documentation

### Advanced Topics
- 🔗 [Cross-Language Compatibility](docs/advanced/COMPATIBILITY.md) - Interoperability
- 📋 [FAQ](docs/guides/FAQ.md) - Frequently asked questions
- 📝 [Changelog](docs/CHANGELOG.md) - Version history

## Value Types

16 distinct value types for comprehensive data handling:

| Category | Types | Size |
|----------|-------|------|
| **Null** | NullValue | 0 bytes |
| **Boolean** | BoolValue | 1 byte |
| **16-bit** | ShortValue, UShortValue | 2 bytes |
| **32-bit** | IntValue, UIntValue, LongValue*, ULongValue* | 4 bytes |
| **64-bit** | LLongValue, ULLongValue | 8 bytes |
| **Floating** | FloatValue, DoubleValue | 4-8 bytes |
| **Complex** | StringValue, BytesValue, ContainerValue, ArrayValue | Variable |

*\* LongValue/ULongValue enforce 32-bit range for C++ compatibility*

**Example**:
```csharp
// 32-bit integer
container.Add(new IntValue("count", 100));

// 64-bit integer (use for large values)
container.Add(new LLongValue("big_number", 5_000_000_000L));

// Floating point
container.Add(new DoubleValue("price", 99.99));

// Nested container
var address = new ValueContainer();
address.Add(new StringValue("city", "Seoul"));
container.Add(new ContainerValue("address", address));
```

📚 **[Value Types Details →](docs/FEATURES.md#value-types)**

## Cross-Language Usage

### JSON v2.0 Adapter

```csharp
using ContainerSystem.Adapters;

// Serialize to JSON v2.0 format (C++ compatible)
string jsonV2 = JsonV2Adapter.ToJson(container);

// Deserialize from JSON v2.0
var restored = JsonV2Adapter.FromJson(jsonV2);
```

This JSON can be read by:
- C++ `container_system`
- Python `python_container_system`
- Go, Rust, Node.js (via JSON parsing)

🔗 **[Compatibility Guide →](docs/advanced/COMPATIBILITY.md)**

## Building

### Using Build Scripts

```bash
# Linux/macOS
./scripts/build.sh              # Release build
./scripts/build.sh debug        # Debug build
./scripts/build.sh --test       # Build and test
./scripts/build.sh --pack       # Build and create NuGet package

# Windows (PowerShell)
.\scripts\build.ps1 -Test -Pack
```

### Manual Build

```bash
dotnet restore
dotnet build --configuration Release
dotnet test
```

🔧 **[Build Guide →](docs/guides/BUILD_GUIDE.md)**

## Thread Safety

```csharp
var container = new ValueContainer();

// Thread-safe concurrent writes
Parallel.For(0, 1000, i =>
{
    container.Add(new IntValue($"value_{i}", i));
});

// Thread-safe reads
var values = container.Values();
Console.WriteLine($"Total: {container.Count}");
```

## Comparison with C++ Version

| Feature | C++ | .NET |
|---------|-----|------|
| Value Types | 15 | 16 (+ArrayValue) |
| Binary Serialization | ✅ 1.8M/sec | ✅ 500K/sec |
| JSON Serialization | ✅ 950K/sec | ✅ 200K/sec |
| SIMD Optimization | ✅ | ❌ |
| Thread Safety | ✅ | ✅ |
| Cross-Language | ✅ | ✅ |
| Memory Pooling | ✅ | ❌ |

## Contributing

We welcome contributions! Please see our guidelines:

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open Pull Request

### Code Style
- Follow .NET coding conventions
- Use nullable reference types
- Write comprehensive tests
- Document public APIs

## Support

- 💬 [GitHub Discussions](https://github.com/kcenon/dotnet_container_system/discussions)
- 🐛 [Issue Tracker](https://github.com/kcenon/dotnet_container_system/issues)
- 📧 Email: kcenon@naver.com

## License

This project is licensed under the BSD 3-Clause License - see the [LICENSE](LICENSE) file for details.

## Related Projects

- [container_system](https://github.com/kcenon/container_system) (C++) - Original implementation
- [python_container_system](https://github.com/kcenon/python_container_system) - Python equivalent
- [messaging_system](https://github.com/kcenon/messaging_system) - High-level messaging

---

<p align="center">
  Made with ❤️ by 🍀☀🌕🌥 🌊
</p>
