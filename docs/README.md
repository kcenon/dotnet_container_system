# .NET Container System Documentation

> **Language:** **English** | [한국어](README_KO.md)

**Version:** 1.0.0
**Last Updated:** 2025-11-26
**Status:** Comprehensive

Welcome to the dotnet_container_system documentation! A type-safe, high-performance container and serialization system for .NET 8 with cross-language compatibility.

---

## 🚀 Quick Navigation

| I want to... | Document |
|--------------|----------|
| ⚡ Get started in 5 minutes | [Quick Start Guide](guides/QUICK_START.md) |
| 🏗️ Understand the architecture | [Architecture](ARCHITECTURE.md) |
| 📚 See all features | [Features Documentation](FEATURES.md) |
| 📖 Browse the API | [API Reference](API_REFERENCE.md) |
| ❓ Find answers to questions | [FAQ](guides/FAQ.md) (25+ Q&A) |
| 🔧 Build from source | [Build Guide](guides/BUILD_GUIDE.md) |
| 📊 Review performance | [Benchmarks](performance/BENCHMARKS.md) |
| 🔗 Cross-language compatibility | [Compatibility Guide](advanced/COMPATIBILITY.md) |
| 🐛 Solve problems | [Troubleshooting](guides/TROUBLESHOOTING.md) |
| ✅ Learn best practices | [Best Practices](guides/BEST_PRACTICES.md) |

---

## Documentation Structure

### 📘 Core Documentation

| Document | Description | Korean | Lines |
|----------|-------------|--------|-------|
| [ARCHITECTURE.md](ARCHITECTURE.md) | System architecture, design patterns, type system | - | 800+ |
| [FEATURES.md](FEATURES.md) | Complete feature documentation with examples | [🇰🇷](FEATURES_KO.md) | 500+ |
| [API_REFERENCE.md](API_REFERENCE.md) | Complete API documentation with code samples | - | 600+ |
| [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) | Code organization and module structure | - | 200+ |

### 📗 User Guides

| Document | Description | Lines |
|----------|-------------|-------|
| [QUICK_START.md](guides/QUICK_START.md) | Getting started in 5 minutes | 150+ |
| [BUILD_GUIDE.md](guides/BUILD_GUIDE.md) | Build instructions and options | 200+ |
| [BEST_PRACTICES.md](guides/BEST_PRACTICES.md) | Recommended usage patterns | 250+ |
| [FAQ.md](guides/FAQ.md) | 25+ frequently asked questions | 300+ |
| [TROUBLESHOOTING.md](guides/TROUBLESHOOTING.md) | Common issues and solutions | 200+ |

### 📙 Advanced Topics

| Document | Description | Lines |
|----------|-------------|-------|
| [COMPATIBILITY.md](advanced/COMPATIBILITY.md) | Cross-language compatibility analysis | 400+ |
| [COMPATIBILITY_UPDATE.md](advanced/COMPATIBILITY_UPDATE.md) | Implementation status and updates | 150+ |

### 📊 Performance

| Document | Description | Korean | Lines |
|----------|-------------|--------|-------|
| [BENCHMARKS.md](performance/BENCHMARKS.md) | Performance analysis and metrics | - | 300+ |

### 🤝 Contributing

| Document | Description | Lines |
|----------|-------------|-------|
| [CONTRIBUTING.md](CONTRIBUTING.md) | Contribution guidelines | 200+ |
| [TESTING.md](contributing/TESTING.md) | Testing strategy and best practices | 300+ |
| [CHANGELOG.md](CHANGELOG.md) | Version history and changes | 150+ |

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

## 📞 Getting Help

- **Issues**: [GitHub Issues](https://github.com/kcenon/dotnet_container_system/issues)
- **Discussions**: [GitHub Discussions](https://github.com/kcenon/dotnet_container_system/discussions)
- **Email**: kcenon@naver.com

---

**Last Updated**: 2025-11-26
