# Contributing Guide

> **Language:** **English** | [한국어](CONTRIBUTING_KO.md)

Thank you for your interest in contributing to .NET Container System!

---

## Table of Contents

1. [Code of Conduct](#code-of-conduct)
2. [Getting Started](#getting-started)
3. [Development Workflow](#development-workflow)
4. [Pull Request Process](#pull-request-process)
5. [Coding Standards](#coding-standards)
6. [Testing Guidelines](#testing-guidelines)
7. [Documentation](#documentation)

---

## Code of Conduct

This project follows a simple code of conduct:

- Be respectful and inclusive
- Focus on constructive feedback
- Help others learn and grow
- Maintain a welcoming environment

---

## Getting Started

### Prerequisites

- .NET SDK 8.0 or later
- Git
- IDE (Visual Studio 2022, VS Code, or JetBrains Rider)

### Setup

```bash
# Fork and clone the repository
git clone https://github.com/YOUR_USERNAME/dotnet_container_system.git
cd dotnet_container_system

# Add upstream remote
git remote add upstream https://github.com/kcenon/dotnet_container_system.git

# Build and test
dotnet build
dotnet test
```

---

## Development Workflow

### 1. Create a Branch

```bash
# Sync with upstream
git fetch upstream
git checkout main
git merge upstream/main

# Create feature branch
git checkout -b feature/your-feature-name
```

### 2. Make Changes

- Write code following our coding standards
- Add tests for new functionality
- Update documentation as needed

### 3. Commit Changes

```bash
# Stage changes
git add .

# Commit with descriptive message
git commit -m "feat: add new value type support"
```

**Commit Message Format:**
```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation only
- `style`: Formatting changes
- `refactor`: Code refactoring
- `test`: Adding tests
- `chore`: Maintenance tasks

### 4. Push and Create PR

```bash
git push origin feature/your-feature-name
```

Then create a Pull Request on GitHub.

---

## Pull Request Process

### Before Submitting

- [ ] Code compiles without errors
- [ ] All tests pass (`dotnet test`)
- [ ] New code is covered by tests
- [ ] Documentation is updated
- [ ] Commit messages follow conventions

### PR Description Template

```markdown
## Summary
Brief description of changes.

## Changes
- Change 1
- Change 2

## Testing
How the changes were tested.

## Related Issues
Fixes #123
```

### Review Process

1. Automated CI checks must pass
2. At least one maintainer review required
3. Address all review comments
4. Squash commits if requested

---

## Coding Standards

### C# Style

Follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions):

```csharp
// Use PascalCase for public members
public string MessageType { get; set; }

// Use camelCase for private fields with underscore prefix
private readonly object _lock = new();

// Use meaningful names
public Value? GetValueByName(string name) // Good
public Value? Get(string n)               // Bad

// Use nullable reference types
public Value? GetValue(string key)

// Prefer expression-bodied members for simple methods
public int Count => _store.Count;
```

### File Organization

```csharp
// 1. File header (license)
// BSD 3-Clause License
// Copyright (c) 2025, kcenon

// 2. Using statements (sorted)
using System;
using System.Collections.Generic;
using ContainerSystem.Core;

// 3. Namespace
namespace ContainerSystem.Values;

// 4. Type definition
public class MyValue : Value
{
    // Fields
    // Constructors
    // Properties
    // Methods
}
```

### XML Documentation

```csharp
/// <summary>
/// Represents a container for typed values with metadata.
/// </summary>
/// <remarks>
/// Thread-safe for concurrent read operations.
/// </remarks>
public class ValueContainer
{
    /// <summary>
    /// Gets or sets the message type.
    /// </summary>
    /// <value>The type of the message.</value>
    public string MessageType { get; set; }

    /// <summary>
    /// Adds a value to the container.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is null.
    /// </exception>
    public void Add(Value value)
    {
        // ...
    }
}
```

---

## Testing Guidelines

### Test Structure

```csharp
[Fact]
public void MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var container = new ValueContainer();

    // Act
    container.Add(new IntValue("x", 42));
    var result = container.GetValue("x");

    // Assert
    Assert.NotNull(result);
    Assert.Equal(42, result.ToInt());
}
```

### Test Categories

| Category | Purpose |
|----------|---------|
| Unit Tests | Test individual methods |
| Integration Tests | Test component interactions |
| Performance Tests | Verify performance targets |

### Running Tests

```bash
# All tests
dotnet test

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Specific tests
dotnet test --filter "FullyQualifiedName~LongRangeCheckingTests"
```

---

## Documentation

### When to Update Docs

- Adding new public APIs
- Changing existing behavior
- Adding new features
- Fixing bugs that affect usage

### Documentation Files

| File | Purpose |
|------|---------|
| README.md | Project overview |
| docs/API_REFERENCE.md | API documentation |
| docs/FEATURES.md | Feature documentation |
| docs/CHANGELOG.md | Version history |

### Style Guidelines

- Use clear, concise language
- Include code examples
- Keep formatting consistent
- Provide both English and Korean versions for main docs

---

## Questions?

- Open an issue for bugs or feature requests
- Start a discussion for questions
- Email: kcenon@naver.com

Thank you for contributing!
