# .NET Container System Documentation

> **Language:** **English** | [한국어](README_KO.md)

**Version:** 1.0.0
**Last Updated:** 2025-11-25
**Status:** Comprehensive

Welcome to the dotnet_container_system documentation! A type-safe, high-performance container and serialization system for .NET 8 with cross-language compatibility.

---

## Quick Navigation

| I want to... | Document |
|--------------|----------|
| Get started quickly | [Quick Start](guides/QUICK_START.md) |
| Understand the architecture | [Architecture](ARCHITECTURE.md) |
| Build from source | [Build Guide](guides/BUILD_GUIDE.md) |
| Find answers to common questions | [FAQ](guides/FAQ.md) |
| Review performance | [Benchmarks](performance/BENCHMARKS.md) |
| Cross-language compatibility | [Compatibility](advanced/COMPATIBILITY.md) |

---

## Documentation Structure

### Core Documentation

| Document | Description | Korean |
|----------|-------------|--------|
| [ARCHITECTURE.md](ARCHITECTURE.md) | System architecture, design patterns, type system | [🇰🇷](ARCHITECTURE_KO.md) |
| [FEATURES.md](FEATURES.md) | Complete feature documentation | [🇰🇷](FEATURES_KO.md) |
| [API_REFERENCE.md](API_REFERENCE.md) | Complete API documentation | - |
| [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) | Code organization | - |

### User Guides

| Document | Description |
|----------|-------------|
| [QUICK_START.md](guides/QUICK_START.md) | Getting started in 5 minutes |
| [BUILD_GUIDE.md](guides/BUILD_GUIDE.md) | Build instructions and options |
| [BEST_PRACTICES.md](guides/BEST_PRACTICES.md) | Recommended usage patterns |
| [TROUBLESHOOTING.md](guides/TROUBLESHOOTING.md) | Common issues and solutions |
| [FAQ.md](guides/FAQ.md) | Frequently asked questions |

### Advanced Topics

| Document | Description |
|----------|-------------|
| [COMPATIBILITY.md](advanced/COMPATIBILITY.md) | Cross-language compatibility analysis |
| [COMPATIBILITY_UPDATE.md](advanced/COMPATIBILITY_UPDATE.md) | Implementation status updates |

### Performance

| Document | Description | Korean |
|----------|-------------|--------|
| [BENCHMARKS.md](performance/BENCHMARKS.md) | Performance analysis and metrics | [🇰🇷](performance/BENCHMARKS_KO.md) |

### Development

| Document | Description |
|----------|-------------|
| [CHANGELOG.md](CHANGELOG.md) | Version history and changes |
| [CONTRIBUTING.md](CONTRIBUTING.md) | Contribution guidelines |

---

## Project Information

### Current Status
- **Version**: 1.0.0
- **Target Framework**: .NET 8.0
- **License**: BSD 3-Clause

### Key Features
- **Type-safe containers** - 16 distinct value types with compile-time checking
- **Cross-language compatibility** - Binary-compatible with C++, Python, Go, Rust, Node.js
- **Thread-safe operations** - ReaderWriterLockSlim for concurrent access
- **Multiple formats** - Binary, JSON serialization
- **High performance** - O(1) lookups, efficient serialization

### Supported Value Types

| Category | Types |
|----------|-------|
| **Null** | NullValue |
| **Boolean** | BoolValue |
| **16-bit Integers** | ShortValue, UShortValue |
| **32-bit Integers** | IntValue, UIntValue, LongValue*, ULongValue* |
| **64-bit Integers** | LLongValue, ULLongValue |
| **Floating Point** | FloatValue, DoubleValue |
| **Complex** | StringValue, BytesValue, ContainerValue, ArrayValue |

*\* LongValue/ULongValue are 32-bit for C++ compatibility*

---

## Related Projects

- [container_system](https://github.com/kcenon/container_system) - Original C++ implementation
- [python_container_system](https://github.com/kcenon/python_container_system) - Python equivalent
- [messaging_system](https://github.com/kcenon/messaging_system) - High-level messaging

---

## Getting Help

- **Issues**: [GitHub Issues](https://github.com/kcenon/dotnet_container_system/issues)
- **Discussions**: [GitHub Discussions](https://github.com/kcenon/dotnet_container_system/discussions)
- **Email**: kcenon@naver.com

---

**Last Updated**: 2025-11-25
