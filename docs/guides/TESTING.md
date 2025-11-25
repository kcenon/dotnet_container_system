# .NET Container System - Testing Guide

> **Last Updated:** 2025-11-26
> **Maintainer:** Container System Development Team

## Overview

This document provides a comprehensive guide to the .NET Container System's testing infrastructure, including unit tests, integration tests, performance benchmarks, and testing best practices.

---

## Test Architecture

The .NET Container System employs a multi-layered testing strategy:

```
┌─────────────────────────────────────────────┐
│           Testing Infrastructure            │
├─────────────────────────────────────────────┤
│  Unit Tests (ContainerSystem.Tests/)        │
│  ├─ Value type tests                        │
│  ├─ Container operations                    │
│  ├─ Serialization formats                   │
│  ├─ Thread safety                           │
│  └─ Long range checking                     │
├─────────────────────────────────────────────┤
│  Integration Tests                          │
│  ├─ Cross-language compatibility            │
│  ├─ End-to-end scenarios                    │
│  └─ Real-world usage patterns               │
├─────────────────────────────────────────────┤
│  Benchmark Tests                            │
│  ├─ Container operations                    │
│  ├─ Serialization performance               │
│  └─ Memory allocation                       │
└─────────────────────────────────────────────┘
```

---

## Test Organization

### Test Project Structure

```
ContainerSystem.Tests/
├── ValueTests/
│   ├── BoolValueTests.cs
│   ├── IntValueTests.cs
│   ├── LongValueTests.cs
│   ├── FloatValueTests.cs
│   ├── DoubleValueTests.cs
│   ├── StringValueTests.cs
│   ├── BytesValueTests.cs
│   └── ArrayValueTests.cs
├── ContainerTests/
│   ├── ValueContainerTests.cs
│   ├── HeaderTests.cs
│   └── CopyTests.cs
├── SerializationTests/
│   ├── BinarySerializationTests.cs
│   ├── JsonSerializationTests.cs
│   └── XmlSerializationTests.cs
├── CompatibilityTests/
│   ├── LongRangeCheckingTests.cs
│   └── CrossLanguageTests.cs
└── PerformanceTests/
    └── BenchmarkTests.cs
```

### Test Categories

#### 1. Value Type Tests

Tests for all 16 supported value types:
- Null, Boolean, Numeric (short, int, long variants)
- Float, Double
- String and Bytes
- Container values (nested structures)
- Array values

```csharp
[Fact]
public void IntValue_Creation_StoresCorrectValue()
{
    // Arrange & Act
    var value = new IntValue("count", 42);

    // Assert
    Assert.Equal("count", value.Name);
    Assert.Equal(42, value.ToInt());
    Assert.Equal(ValueType.Int, value.Type);
}
```

#### 2. Container Operations Tests

```csharp
[Fact]
public void ValueContainer_AddValue_RetrievesCorrectly()
{
    // Arrange
    var container = new ValueContainer();
    var value = new StringValue("name", "Alice");

    // Act
    container.Add(value);
    var retrieved = container.GetValue("name");

    // Assert
    Assert.NotNull(retrieved);
    Assert.Equal("Alice", retrieved?.ToString());
}
```

#### 3. Serialization Tests

```csharp
[Fact]
public void Serialization_Binary_RoundTrip()
{
    // Arrange
    var original = new ValueContainer { MessageType = "test" };
    original.Add(new IntValue("x", 42));
    original.Add(new StringValue("name", "Test"));

    // Act
    var bytes = original.SerializeToBytes();
    var restored = ValueContainer.DeserializeFromBytes(bytes);

    // Assert
    Assert.Equal(original.MessageType, restored.MessageType);
    Assert.Equal(42, restored.GetValue("x")?.ToInt());
    Assert.Equal("Test", restored.GetValue("name")?.ToString());
}
```

#### 4. Thread Safety Tests

```csharp
[Fact]
public async Task ValueContainer_ConcurrentReads_ThreadSafe()
{
    // Arrange
    var container = new ValueContainer();
    for (int i = 0; i < 100; i++)
    {
        container.Add(new IntValue($"key_{i}", i));
    }

    // Act - Multiple concurrent reads
    var tasks = Enumerable.Range(0, 10)
        .Select(_ => Task.Run(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                var value = container.GetValue($"key_{i}");
                Assert.Equal(i, value?.ToInt());
            }
        }));

    // Assert - No exceptions
    await Task.WhenAll(tasks);
}
```

#### 5. Long Range Checking Tests

```csharp
[Fact]
public void LongValue_ExceedsInt32Range_ThrowsException()
{
    // Arrange
    long largeValue = (long)int.MaxValue + 1;

    // Act & Assert
    Assert.Throws<ArgumentOutOfRangeException>(() =>
        new LongValue("large", largeValue));
}

[Fact]
public void LongValue_WithinInt32Range_Succeeds()
{
    // Arrange & Act
    var value = new LongValue("valid", int.MaxValue);

    // Assert
    Assert.Equal(int.MaxValue, value.ToLong());
}
```

---

## Running Tests

### Basic Commands

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run specific test project
dotnet test ContainerSystem.Tests/ContainerSystem.Tests.csproj

# Run specific test class
dotnet test --filter "FullyQualifiedName~ValueContainerTests"

# Run specific test method
dotnet test --filter "ValueContainer_AddValue_RetrievesCorrectly"
```

### With Coverage

```bash
# Using coverlet
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report (requires ReportGenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# Open report
open coveragereport/index.html  # macOS
start coveragereport/index.html # Windows
```

### Filter Tests

```bash
# Run tests containing specific name
dotnet test --filter "FullyQualifiedName~Serialization"

# Run tests with specific trait
dotnet test --filter "Category=Performance"

# Exclude tests
dotnet test --filter "FullyQualifiedName!~SlowTest"

# Combine filters
dotnet test --filter "FullyQualifiedName~Value&Category=Unit"
```

---

## Test Coverage

### Current Coverage Targets

| Component | Target | Status |
|-----------|--------|--------|
| Core (Value, Container) | 90%+ | ✅ |
| Value Types | 95%+ | ✅ |
| Serialization | 85%+ | ✅ |
| Thread Safety | 80%+ | ✅ |
| **Overall** | **85%+** | ✅ |

### Coverage Report

```bash
# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov

# View summary
dotnet test /p:CollectCoverage=true

# Output:
# +-----------------+--------+--------+--------+
# | Module          | Line   | Branch | Method |
# +-----------------+--------+--------+--------+
# | ContainerSystem | 87.5%  | 82.3%  | 91.2%  |
# +-----------------+--------+--------+--------+
```

---

## Writing Tests

### Naming Conventions

```csharp
// Format: MethodName_Scenario_ExpectedResult
[Fact]
public void Add_NullValue_ThrowsArgumentNullException() { }

[Fact]
public void Serialize_EmptyContainer_ReturnsValidBytes() { }

[Fact]
public void GetValue_NonExistentKey_ReturnsNull() { }
```

### Test Structure (AAA Pattern)

```csharp
[Fact]
public void ValueContainer_Serialize_PreservesAllValues()
{
    // Arrange - Set up test data
    var container = new ValueContainer
    {
        SourceId = "client",
        TargetId = "server",
        MessageType = "request"
    };
    container.Add(new StringValue("action", "query"));
    container.Add(new IntValue("limit", 100));

    // Act - Perform the operation
    var json = container.ToJson();
    var restored = ValueContainer.FromJson(json);

    // Assert - Verify results
    Assert.Equal("client", restored.SourceId);
    Assert.Equal("server", restored.TargetId);
    Assert.Equal("request", restored.MessageType);
    Assert.Equal("query", restored.GetValue("action")?.ToString());
    Assert.Equal(100, restored.GetValue("limit")?.ToInt());
}
```

### Theory Tests (Data-Driven)

```csharp
[Theory]
[InlineData(0)]
[InlineData(1)]
[InlineData(-1)]
[InlineData(int.MaxValue)]
[InlineData(int.MinValue)]
public void IntValue_AllRanges_SerializesCorrectly(int value)
{
    // Arrange
    var original = new IntValue("test", value);
    var container = new ValueContainer();
    container.Add(original);

    // Act
    var bytes = container.SerializeToBytes();
    var restored = ValueContainer.DeserializeFromBytes(bytes);

    // Assert
    Assert.Equal(value, restored.GetValue("test")?.ToInt());
}

[Theory]
[MemberData(nameof(GetValueTypeTestCases))]
public void Value_AllTypes_RoundTrip(Value value, object expected)
{
    // Arrange
    var container = new ValueContainer();
    container.Add(value);

    // Act
    var bytes = container.SerializeToBytes();
    var restored = ValueContainer.DeserializeFromBytes(bytes);

    // Assert
    Assert.Equal(expected, GetActualValue(restored.GetValue(value.Name)));
}

public static IEnumerable<object[]> GetValueTypeTestCases()
{
    yield return new object[] { new BoolValue("b", true), true };
    yield return new object[] { new IntValue("i", 42), 42 };
    yield return new object[] { new StringValue("s", "test"), "test" };
    // ... more cases
}
```

### Test Fixtures

```csharp
public class ContainerTestFixture : IDisposable
{
    public ValueContainer TestContainer { get; }
    public string TempDirectory { get; }

    public ContainerTestFixture()
    {
        TestContainer = CreateTestContainer();
        TempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(TempDirectory);
    }

    private static ValueContainer CreateTestContainer()
    {
        var container = new ValueContainer
        {
            SourceId = "test_source",
            TargetId = "test_target",
            MessageType = "test_message"
        };
        container.Add(new StringValue("name", "Test"));
        container.Add(new IntValue("count", 42));
        container.Add(new BoolValue("active", true));
        return container;
    }

    public void Dispose()
    {
        if (Directory.Exists(TempDirectory))
        {
            Directory.Delete(TempDirectory, true);
        }
    }
}

public class ContainerTests : IClassFixture<ContainerTestFixture>
{
    private readonly ContainerTestFixture _fixture;

    public ContainerTests(ContainerTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void TestWithFixture()
    {
        var container = _fixture.TestContainer;
        // Use pre-configured test container
    }
}
```

---

## Performance Testing

### BenchmarkDotNet Usage

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
public class ContainerBenchmarks
{
    private ValueContainer _container = null!;

    [GlobalSetup]
    public void Setup()
    {
        _container = new ValueContainer();
        for (int i = 0; i < 100; i++)
        {
            _container.Add(new IntValue($"key_{i}", i));
        }
    }

    [Benchmark]
    public ValueContainer CreateContainer()
    {
        return new ValueContainer();
    }

    [Benchmark]
    public void AddValue()
    {
        var c = new ValueContainer();
        c.Add(new IntValue("key", 42));
    }

    [Benchmark]
    public byte[] SerializeToBytes()
    {
        return _container.SerializeToBytes();
    }

    [Benchmark]
    public string SerializeToJson()
    {
        return _container.ToJson();
    }
}
```

### Running Benchmarks

```bash
# Run benchmarks
dotnet run -c Release --project ContainerSystem.Benchmarks

# Export results
dotnet run -c Release -- --exporters json html

# Specific benchmark
dotnet run -c Release -- --filter "*Serialize*"
```

### Performance Targets

| Operation | Target | Status |
|-----------|--------|--------|
| Container creation | >500K/s | ✅ |
| Add value | >1M/s | ✅ |
| Get value | >5M/s | ✅ |
| Binary serialize | >300K/s | ✅ |
| JSON serialize | >100K/s | ✅ |

---

## Continuous Integration

### GitHub Actions Workflow

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]
        dotnet: ['8.0.x']

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ matrix.dotnet }}

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Test
        run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"

      - name: Upload Coverage
        uses: codecov/codecov-action@v4
        with:
          files: '**/coverage.cobertura.xml'
```

---

## Debugging Failed Tests

### Common Issues

1. **Flaky Tests**
   ```bash
   # Run test multiple times
   for i in {1..10}; do dotnet test --filter "FlakyTest"; done
   ```

2. **Platform-Specific Failures**
   - Check endianness for binary operations
   - Verify path separators for file operations
   - Check culture settings for string formatting

3. **Thread Safety Issues**
   ```bash
   # Run with more parallelism
   dotnet test --parallel --maxcpucount
   ```

### Debugging Commands

```bash
# Verbose logging
dotnet test --logger "console;verbosity=detailed"

# Blame mode (identifies crashing tests)
dotnet test --blame

# With diagnostics
dotnet test --diag:test_diag.log
```

### Visual Studio Debugging

1. Set breakpoint in test method
2. Right-click test → Debug Test(s)
3. Use Test Explorer for filtering

---

## Test Maintenance

### Regular Tasks

- **Weekly**: Review CI test results
- **Monthly**: Update performance baselines
- **Quarterly**: Review and update coverage targets
- **Per Release**: Verify all tests pass on all platforms

### Quality Metrics

- **Execution Time**: All unit tests < 30 seconds
- **Reliability**: >99% pass rate in CI
- **Coverage**: Maintain >85% line coverage
- **Flakiness**: <1% flaky tests

---

## References

### Related Documentation

- [CONTRIBUTING.md](../CONTRIBUTING.md) - Contribution guidelines
- [BENCHMARKS.md](../performance/BENCHMARKS.md) - Performance benchmarks
- [API_REFERENCE.md](../API_REFERENCE.md) - API documentation

### External Resources

- [xUnit Documentation](https://xunit.net/)
- [BenchmarkDotNet](https://benchmarkdotnet.org/)
- [Coverlet](https://github.com/coverlet-coverage/coverlet)
- [.NET Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

---

**Document Version:** 1.0
**Last Updated:** 2025-11-26
**Contact:** kcenon@naver.com
