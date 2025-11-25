@echo off
REM .NET Container System Build Script for Windows CMD
REM Usage: scripts\build.bat [debug|release] [--test] [--pack]

setlocal enabledelayedexpansion

REM Default configuration
set CONFIG=Release
set RUN_TESTS=false
set CREATE_PACKAGE=false

REM Parse arguments
:parse_args
if "%~1"=="" goto :start_build
if /i "%~1"=="debug" (
    set CONFIG=Debug
    shift
    goto :parse_args
)
if /i "%~1"=="release" (
    set CONFIG=Release
    shift
    goto :parse_args
)
if /i "%~1"=="--test" (
    set RUN_TESTS=true
    shift
    goto :parse_args
)
if /i "%~1"=="-t" (
    set RUN_TESTS=true
    shift
    goto :parse_args
)
if /i "%~1"=="--pack" (
    set CREATE_PACKAGE=true
    shift
    goto :parse_args
)
if /i "%~1"=="-p" (
    set CREATE_PACKAGE=true
    shift
    goto :parse_args
)
if /i "%~1"=="--help" goto :show_help
if /i "%~1"=="-h" goto :show_help

echo Unknown option: %~1
exit /b 1

:show_help
echo Usage: %~nx0 [debug^|release] [--test] [--pack]
echo.
echo Options:
echo   debug^|release  Build configuration (default: Release)
echo   --test, -t     Run tests after build
echo   --pack, -p     Create NuGet package
echo   --help, -h     Show this help message
exit /b 0

:start_build
REM Get project directory
set SCRIPT_DIR=%~dp0
set PROJECT_DIR=%SCRIPT_DIR%..

echo ========================================
echo .NET Container System Build
echo ========================================
echo.
echo Configuration: %CONFIG%
echo Project Directory: %PROJECT_DIR%
echo.

REM Change to project directory
cd /d "%PROJECT_DIR%"

REM Check .NET SDK
where dotnet >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo Error: .NET SDK not found
    echo Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download
    exit /b 1
)

for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo Using .NET SDK: %DOTNET_VERSION%
echo.

REM Restore dependencies
echo Restoring dependencies...
dotnet restore
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

REM Build
echo.
echo Building solution...
dotnet build --configuration %CONFIG% --no-restore
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

REM Run tests if requested
if "%RUN_TESTS%"=="true" (
    echo.
    echo Running tests...
    dotnet test --configuration %CONFIG% --no-build --verbosity normal
    if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%
)

REM Create package if requested
if "%CREATE_PACKAGE%"=="true" (
    echo.
    echo Creating NuGet package...
    dotnet pack ContainerSystem\ContainerSystem.csproj --configuration %CONFIG% --no-build --output .\nupkg
    if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%
    echo Package created in .\nupkg
)

echo.
echo ========================================
echo Build completed successfully!
echo ========================================
echo.
echo Output:
echo   Library: ContainerSystem\bin\%CONFIG%\net8.0\
echo   Examples: ContainerSystem.Examples\bin\%CONFIG%\net8.0\
if "%CREATE_PACKAGE%"=="true" (
    echo   Package: .\nupkg\
)

endlocal
