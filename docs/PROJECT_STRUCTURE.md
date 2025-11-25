# Project Structure

> **Language:** **English** | [한국어](PROJECT_STRUCTURE_KO.md)

Code organization and file structure for .NET Container System.

---

## Directory Structure

```
dotnet_container_system/
├── .github/
│   └── workflows/           # CI/CD workflows
│       ├── ci.yml           # Main CI pipeline
│       └── coverage.yml     # Code coverage
│
├── ContainerSystem/         # Main library (2,375 lines)
│   ├── Core/                # Core abstractions (1,541 lines)
│   │   ├── ValueTypes.cs    # Value type enumeration
│   │   ├── Value.cs         # Abstract base class
│   │   ├── ValueContainer.cs# Message container
│   │   └── ValueStore.cs    # Key-value store
│   │
│   ├── Values/              # Concrete value types (834 lines)
│   │   ├── NumericValue.cs  # IntValue, LongValue
│   │   ├── ShortValues.cs   # ShortValue, UShortValue
│   │   ├── UIntValues.cs    # UIntValue, ULongValue
│   │   ├── LLongValues.cs   # LLongValue, ULLongValue
│   │   ├── StringValue.cs   # String values
│   │   ├── ContainerValue.cs# Nested containers
│   │   └── ArrayValue.cs    # Array/list values
│   │
│   ├── Adapters/            # Format adapters
│   │   └── JsonV2Adapter.cs # JSON v2.0 (C++ compatible)
│   │
│   └── ContainerSystem.csproj
│
├── ContainerSystem.Examples/ # Usage examples
│   ├── BasicUsage.cs        # Comprehensive examples
│   ├── Program.cs           # Entry point
│   └── ContainerSystem.Examples.csproj
│
├── ContainerSystem.Tests/   # Unit tests (1,330 lines)
│   ├── LongRangeCheckingTests.cs      # 39 tests
│   ├── ValueContainerEnhancedTests.cs # Container tests
│   ├── CppCompatibilityTests.cs       # C++ interop tests
│   ├── ValueStoreTests.cs             # Store tests
│   └── ContainerSystem.Tests.csproj
│
├── Examples/                # Additional examples
│   └── JsonV2CompatibilityExample.cs
│
├── benchmarks/              # Performance benchmarks
│   ├── SerializationBenchmark.cs
│   └── ContainerSystem.Benchmarks.csproj
│
├── docs/                    # Documentation
│   ├── README.md            # Documentation index
│   ├── ARCHITECTURE.md      # System architecture
│   ├── FEATURES.md          # Feature documentation
│   ├── API_REFERENCE.md     # API reference
│   ├── PROJECT_STRUCTURE.md # This file
│   ├── CHANGELOG.md         # Version history
│   │
│   ├── guides/              # User guides
│   │   ├── QUICK_START.md
│   │   ├── BUILD_GUIDE.md
│   │   ├── BEST_PRACTICES.md
│   │   ├── TROUBLESHOOTING.md
│   │   └── FAQ.md
│   │
│   ├── advanced/            # Advanced topics
│   │   ├── COMPATIBILITY.md
│   │   └── COMPATIBILITY_UPDATE.md
│   │
│   └── performance/         # Performance docs
│       └── BENCHMARKS.md
│
├── scripts/                 # Build scripts
│   ├── build.sh             # Linux/macOS build
│   ├── build.bat            # Windows CMD build
│   └── build.ps1            # Windows PowerShell build
│
├── .gitignore
├── README.md                # Project README
├── README_KO.md             # Korean README
├── LICENSE                  # BSD 3-Clause
└── dotnet-container-system.sln
```

---

## Core Components

### ContainerSystem/Core/

#### ValueTypes.cs (156 lines)
```csharp
public enum ValueTypes
{
    NullValue = 0,
    BoolValue = 1,
    ShortValue = 2,
    // ... 16 types total
}
```
- Defines all 16 value types
- Conversion helpers
- Type metadata

#### Value.cs (265 lines)
```csharp
public abstract class Value
{
    public string Name { get; }
    public ValueTypes Type { get; }
    public Value? Parent { get; }
    // Type checking, conversion methods
}
```
- Abstract base for all values
- Parent-child relationships
- Type checking methods
- Conversion methods

#### ValueContainer.cs (780 lines)
```csharp
public class ValueContainer
{
    public string MessageType { get; set; }
    public string SourceId { get; }
    public string TargetId { get; }
    // Add, GetValue, Serialize methods
}
```
- High-level message container
- Thread-safe operations
- Serialization/deserialization
- Routing metadata

#### ValueStore.cs (340 lines)
```csharp
public class ValueStore
{
    public bool ThreadSafe { get; }
    public long ReadCount { get; }
    public long WriteCount { get; }
    // Storage and retrieval
}
```
- Key-value storage backend
- Optional thread safety
- Statistics tracking

---

### ContainerSystem/Values/

#### NumericValue.cs (162 lines)
- `IntValue` - 32-bit signed integer
- `LongValue` - 32-bit range (C++ compatible)

#### ShortValues.cs (59 lines)
- `ShortValue` - 16-bit signed
- `UShortValue` - 16-bit unsigned

#### UIntValues.cs (73 lines)
- `UIntValue` - 32-bit unsigned
- `ULongValue` - 32-bit range unsigned

#### LLongValues.cs (61 lines)
- `LLongValue` - 64-bit signed
- `ULLongValue` - 64-bit unsigned

#### StringValue.cs (59 lines)
- String value storage
- UTF-8 encoding

#### ContainerValue.cs (167 lines)
- Nested container support
- Recursive serialization

#### ArrayValue.cs (253 lines)
- Array/list of values
- Item management

---

### ContainerSystem/Adapters/

#### JsonV2Adapter.cs (503 lines)
```csharp
public static class JsonV2Adapter
{
    public static string ToJson(ValueContainer container);
    public static ValueContainer FromJson(string json);
}
```
- JSON v2.0 format adapter
- C++ format compatibility
- Cross-language serialization

---

## Test Structure

### Test Categories

| File | Tests | Coverage |
|------|-------|----------|
| LongRangeCheckingTests.cs | 39 | Long/ULong type policy |
| ValueContainerEnhancedTests.cs | ~20 | Container operations |
| CppCompatibilityTests.cs | ~15 | C++ interoperability |
| ValueStoreTests.cs | ~15 | Store operations |

### Test Naming Convention

```csharp
[Fact]
public void MethodName_Scenario_ExpectedResult()
{
    // Arrange
    // Act
    // Assert
}
```

---

## Documentation Structure

### Core Docs
- `README.md` - Index and navigation
- `ARCHITECTURE.md` - Design and patterns
- `FEATURES.md` - Feature documentation
- `API_REFERENCE.md` - Complete API
- `PROJECT_STRUCTURE.md` - This file
- `CHANGELOG.md` - Version history

### Guides
- `QUICK_START.md` - 5-minute start
- `BUILD_GUIDE.md` - Build instructions
- `BEST_PRACTICES.md` - Usage patterns
- `TROUBLESHOOTING.md` - Problem solving
- `FAQ.md` - Common questions

### Advanced
- `COMPATIBILITY.md` - Cross-language analysis
- `COMPATIBILITY_UPDATE.md` - Implementation status

### Performance
- `BENCHMARKS.md` - Performance metrics

---

## Build Artifacts

```
build/
├── Debug/
│   └── net8.0/
│       ├── ContainerSystem.dll
│       ├── ContainerSystem.pdb
│       └── ContainerSystem.xml (XML docs)
│
└── Release/
    └── net8.0/
        ├── ContainerSystem.dll
        └── ContainerSystem.nupkg (NuGet package)
```

---

## Code Statistics

| Component | Files | Lines |
|-----------|-------|-------|
| Core | 4 | 1,541 |
| Values | 7 | 834 |
| Adapters | 1 | 503 |
| Tests | 4 | 1,330 |
| Examples | 3 | ~500 |
| **Total** | **19+** | **~4,700** |

---

## See Also

- [Architecture](ARCHITECTURE.md) - System design
- [Build Guide](guides/BUILD_GUIDE.md) - Building the project
- [Contributing](CONTRIBUTING.md) - Development guidelines
