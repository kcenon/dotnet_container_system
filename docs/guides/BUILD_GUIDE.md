# Build Guide

> **Language:** **English** | [한국어](BUILD_GUIDE_KO.md)

Complete guide for building .NET Container System from source.

---

## Prerequisites

### Required
- **.NET SDK 8.0** or later
- **Git** for cloning the repository

### Recommended
- **Visual Studio 2022** (Windows)
- **Visual Studio Code** with C# extension (Cross-platform)
- **JetBrains Rider** (Cross-platform)

---

## Quick Build

### Clone and Build

```bash
# Clone repository
git clone https://github.com/kcenon/dotnet_container_system.git
cd dotnet_container_system

# Restore dependencies
dotnet restore

# Build solution
dotnet build
```

### Using Build Scripts

```bash
# Linux/macOS
./scripts/build.sh

# Windows (PowerShell)
.\scripts\build.ps1

# Windows (CMD)
scripts\build.bat
```

---

## Build Configurations

### Debug Build (Default)

```bash
dotnet build --configuration Debug
```

Features:
- Full debugging symbols
- No optimization
- Additional runtime checks

### Release Build

```bash
dotnet build --configuration Release
```

Features:
- Full optimization
- Smaller binary size
- Better performance

---

## Project Structure

```
dotnet_container_system/
├── ContainerSystem/              # Main library
│   ├── Core/                     # Core abstractions
│   │   ├── ValueTypes.cs         # Value type enumeration
│   │   ├── Value.cs              # Abstract base class
│   │   ├── ValueContainer.cs     # Message container
│   │   └── ValueStore.cs         # Key-value store
│   ├── Values/                   # Concrete value types
│   │   ├── NumericValue.cs       # Int, Long values
│   │   ├── StringValue.cs        # String values
│   │   └── ...
│   └── Adapters/                 # Format adapters
│       └── JsonV2Adapter.cs      # JSON v2.0 adapter
├── ContainerSystem.Examples/     # Usage examples
├── ContainerSystem.Tests/        # Unit tests
└── dotnet-container-system.sln   # Solution file
```

---

## Building Individual Projects

### Main Library

```bash
cd ContainerSystem
dotnet build
```

### Examples

```bash
cd ContainerSystem.Examples
dotnet build
dotnet run
```

### Tests

```bash
cd ContainerSystem.Tests
dotnet build
dotnet test
```

---

## Running Tests

### All Tests

```bash
dotnet test
```

### With Verbose Output

```bash
dotnet test --verbosity detailed
```

### With Code Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~LongRangeCheckingTests"
```

---

## NuGet Package

### Create Package

```bash
cd ContainerSystem
dotnet pack --configuration Release
```

Package will be created at:
```
ContainerSystem/bin/Release/ContainerSystem.1.0.0.nupkg
```

### Local Installation

```bash
dotnet add package ContainerSystem --source ./ContainerSystem/bin/Release
```

---

## IDE Setup

### Visual Studio 2022

1. Open `dotnet-container-system.sln`
2. Set `ContainerSystem.Examples` as startup project
3. Press F5 to build and run

### Visual Studio Code

1. Open folder in VS Code
2. Install C# extension
3. Press Ctrl+Shift+B to build
4. Press F5 to debug

### JetBrains Rider

1. Open `dotnet-container-system.sln`
2. Select run configuration
3. Click Run or Debug

---

## Build Options

### Custom Output Directory

```bash
dotnet build --output ./custom_output
```

### Specific Framework

```bash
dotnet build --framework net8.0
```

### Clean Build

```bash
dotnet clean
dotnet build
```

---

## Continuous Integration

### GitHub Actions

The project includes CI workflow at `.github/workflows/ci.yml`:

```yaml
name: CI
on: [push, pull_request]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet build
      - run: dotnet test
```

---

## Troubleshooting

### SDK Not Found

```bash
# Check installed SDKs
dotnet --list-sdks

# Install .NET 8 SDK from
# https://dotnet.microsoft.com/download
```

### Restore Failed

```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore again
dotnet restore
```

### Build Errors

```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

---

## Platform-Specific Notes

### Linux

```bash
# Ubuntu/Debian - Install .NET SDK
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

### macOS

```bash
# Using Homebrew
brew install --cask dotnet-sdk
```

### Windows

Download from [.NET Download Page](https://dotnet.microsoft.com/download) or use:

```powershell
# Using winget
winget install Microsoft.DotNet.SDK.8
```

---

## Next Steps

- [Quick Start](QUICK_START.md) - Start using the library
- [Best Practices](BEST_PRACTICES.md) - Recommended patterns
- [Project Structure](../PROJECT_STRUCTURE.md) - Detailed code organization
