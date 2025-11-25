# .NET Container System Build Script for Windows PowerShell
# Usage: .\scripts\build.ps1 [-Config <Debug|Release>] [-Test] [-Pack]

param(
    [Parameter(Position = 0)]
    [ValidateSet("Debug", "Release")]
    [string]$Config = "Release",

    [switch]$Test,
    [switch]$Pack,
    [switch]$Help
)

# Colors
$Green = "Green"
$Yellow = "Yellow"
$Red = "Red"

function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

if ($Help) {
    Write-Host @"
.NET Container System Build Script

Usage: .\scripts\build.ps1 [-Config <Debug|Release>] [-Test] [-Pack]

Parameters:
  -Config    Build configuration (Debug or Release). Default: Release
  -Test      Run tests after build
  -Pack      Create NuGet package
  -Help      Show this help message

Examples:
  .\scripts\build.ps1                    # Release build
  .\scripts\build.ps1 -Config Debug      # Debug build
  .\scripts\build.ps1 -Test              # Build and test
  .\scripts\build.ps1 -Pack              # Build and create package
  .\scripts\build.ps1 -Test -Pack        # Build, test, and package
"@
    exit 0
}

# Get directories
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Split-Path -Parent $ScriptDir

Write-ColorOutput "========================================" $Green
Write-ColorOutput ".NET Container System Build" $Green
Write-ColorOutput "========================================" $Green
Write-Host ""
Write-Host "Configuration: " -NoNewline
Write-ColorOutput $Config $Yellow
Write-Host "Project Directory: $ProjectDir"
Write-Host ""

# Change to project directory
Set-Location $ProjectDir

# Check .NET SDK
try {
    $dotnetVersion = & dotnet --version 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet command failed"
    }
    Write-Host "Using .NET SDK: " -NoNewline
    Write-ColorOutput $dotnetVersion $Green
    Write-Host ""
}
catch {
    Write-ColorOutput "Error: .NET SDK not found" $Red
    Write-Host "Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download"
    exit 1
}

# Restore dependencies
Write-ColorOutput "Restoring dependencies..." $Yellow
& dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-ColorOutput "Restore failed!" $Red
    exit $LASTEXITCODE
}

# Build
Write-Host ""
Write-ColorOutput "Building solution..." $Yellow
& dotnet build --configuration $Config --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-ColorOutput "Build failed!" $Red
    exit $LASTEXITCODE
}

# Run tests if requested
if ($Test) {
    Write-Host ""
    Write-ColorOutput "Running tests..." $Yellow
    & dotnet test --configuration $Config --no-build --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput "Tests failed!" $Red
        exit $LASTEXITCODE
    }
}

# Create package if requested
if ($Pack) {
    Write-Host ""
    Write-ColorOutput "Creating NuGet package..." $Yellow
    & dotnet pack ContainerSystem/ContainerSystem.csproj --configuration $Config --no-build --output ./nupkg
    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput "Pack failed!" $Red
        exit $LASTEXITCODE
    }
    Write-ColorOutput "Package created in ./nupkg" $Green
}

Write-Host ""
Write-ColorOutput "========================================" $Green
Write-ColorOutput "Build completed successfully!" $Green
Write-ColorOutput "========================================" $Green
Write-Host ""
Write-Host "Output:"
Write-Host "  Library: ContainerSystem/bin/$Config/net8.0/"
Write-Host "  Examples: ContainerSystem.Examples/bin/$Config/net8.0/"
if ($Pack) {
    Write-Host "  Package: ./nupkg/"
}
