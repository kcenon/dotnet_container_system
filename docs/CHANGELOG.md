# Changelog

> **Language:** **English** | [한국어](CHANGELOG_KO.md)

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added
- **WireProtocol**: C++ Wire Protocol serialization support
  - `WireProtocol.Serialize()` - Serialize container to C++ wire format
  - `WireProtocol.Deserialize()` - Deserialize from C++ wire format
  - `WireProtocol.TryDeserialize()` - Safe deserialization without exceptions
  - Full support for all value types including nested containers
  - Special character escaping for protocol delimiters
  - Header field encoding (target, source, message_type, version)
- Comprehensive unit tests for wire protocol (25+ tests)

### Fixed
- **WireProtocol**: Fixed parsing of escaped special characters (`\[`, `\]`, `\,`, `\;`)
  - Replaced regex-based parsing with escape-aware manual parser
  - Roundtrip tests now correctly handle special characters in names and values
- **ShortValue/UShortValue**: Added missing `ToShort()` and `ToUShort()` methods
- **UIntValue**: Added missing `ToUInt()` method
- **ULongValue/ULLongValue**: Added missing `ToULong()` method
- **WireProtocolTests**: Fixed type ambiguity in test assertions

### Planned
- Memory pooling for high-throughput scenarios
- Async serialization support
- Additional serialization formats (MessagePack, BSON)

---

## [1.0.0] - 2025-10-27

### Added

#### Core Features
- **ValueContainer**: High-level message container with routing metadata
  - Source/Target identification
  - Message type support
  - Thread-safe operations
- **ValueStore**: Key-value storage backend
  - Optional thread-safe mode
  - Statistics tracking (read/write/serialization counts)
  - O(1) key lookup

#### Value Types (16 types)
- **Null**: `NullValue`
- **Boolean**: `BoolValue`
- **16-bit Integers**: `ShortValue`, `UShortValue`
- **32-bit Integers**: `IntValue`, `UIntValue`
- **32-bit Range (C++ compatible)**: `LongValue`, `ULongValue`
- **64-bit Integers**: `LLongValue`, `ULLongValue`
- **Floating Point**: `FloatValue`, `DoubleValue`
- **String**: `StringValue`
- **Binary**: `BytesValue`
- **Container**: `ContainerValue`
- **Array**: `ArrayValue`

#### Serialization
- JSON serialization and deserialization
- Binary serialization for performance
- XML serialization
- JSON v2.0 adapter for C++ compatibility

#### Cross-Language Compatibility
- Binary format compatible with C++ container_system
- JSON v2.0 format for universal compatibility
- Type policy enforcement (LongValue/ULongValue 32-bit range)

#### Thread Safety
- `ReaderWriterLockSlim` for concurrent access
- Thread-safe container operations
- Recursive lock policy support

#### Documentation
- Complete API reference
- Quick start guide
- Build guide
- Best practices
- Troubleshooting guide
- FAQ
- Cross-language compatibility documentation

#### Testing
- 39+ unit tests for type policy
- Container operation tests
- C++ compatibility tests
- Value store tests

#### Examples
- Basic usage examples
- JSON v2.0 compatibility examples
- Thread-safe operation examples

---

## Version History Summary

| Version | Date | Highlights |
|---------|------|------------|
| 1.0.0 | 2025-10-27 | Initial release with 16 value types, cross-language compatibility |

---

## Migration Notes

### From Pre-release to 1.0.0

No breaking changes - this is the initial release.

### From C++ container_system

Key differences when migrating from C++:
1. Use `LLongValue`/`ULLongValue` for 64-bit integers (not `LongValue`)
2. Use `JsonV2Adapter` for cross-language JSON exchange
3. Thread safety is built-in (no separate wrapper needed)

```csharp
// C++ equivalent
// auto val = value_factory::create_int64("id", 12345);

// .NET equivalent
var val = new LLongValue("id", 12345);
```

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on contributing to this project.

When making changes:
1. Update this CHANGELOG
2. Follow semantic versioning
3. Document breaking changes clearly

---

## Links

- [GitHub Repository](https://github.com/kcenon/dotnet_container_system)
- [Issue Tracker](https://github.com/kcenon/dotnet_container_system/issues)
- [C++ Version](https://github.com/kcenon/container_system)
- [Python Version](https://github.com/kcenon/python_container_system)
