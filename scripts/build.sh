#!/bin/bash

# .NET Container System Build Script
# Usage: ./scripts/build.sh [debug|release] [--test] [--pack]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Default configuration
CONFIG="Release"
RUN_TESTS=false
CREATE_PACKAGE=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        debug|Debug)
            CONFIG="Debug"
            shift
            ;;
        release|Release)
            CONFIG="Release"
            shift
            ;;
        --test|-t)
            RUN_TESTS=true
            shift
            ;;
        --pack|-p)
            CREATE_PACKAGE=true
            shift
            ;;
        --help|-h)
            echo "Usage: $0 [debug|release] [--test] [--pack]"
            echo ""
            echo "Options:"
            echo "  debug|release  Build configuration (default: Release)"
            echo "  --test, -t     Run tests after build"
            echo "  --pack, -p     Create NuGet package"
            echo "  --help, -h     Show this help message"
            exit 0
            ;;
        *)
            echo -e "${RED}Unknown option: $1${NC}"
            exit 1
            ;;
    esac
done

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}.NET Container System Build${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "Configuration: ${YELLOW}$CONFIG${NC}"
echo -e "Project Directory: $PROJECT_DIR"
echo ""

# Change to project directory
cd "$PROJECT_DIR"

# Check .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}Error: .NET SDK not found${NC}"
    echo "Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo -e "Using .NET SDK: ${GREEN}$DOTNET_VERSION${NC}"
echo ""

# Restore dependencies
echo -e "${YELLOW}Restoring dependencies...${NC}"
dotnet restore

# Build
echo ""
echo -e "${YELLOW}Building solution...${NC}"
dotnet build --configuration "$CONFIG" --no-restore

# Run tests if requested
if [ "$RUN_TESTS" = true ]; then
    echo ""
    echo -e "${YELLOW}Running tests...${NC}"
    dotnet test --configuration "$CONFIG" --no-build --verbosity normal
fi

# Create package if requested
if [ "$CREATE_PACKAGE" = true ]; then
    echo ""
    echo -e "${YELLOW}Creating NuGet package...${NC}"
    dotnet pack ContainerSystem/ContainerSystem.csproj --configuration "$CONFIG" --no-build --output ./nupkg
    echo -e "${GREEN}Package created in ./nupkg${NC}"
fi

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Build completed successfully!${NC}"
echo -e "${GREEN}========================================${NC}"

# Show output location
echo ""
echo "Output:"
echo "  Library: ContainerSystem/bin/$CONFIG/net8.0/"
echo "  Examples: ContainerSystem.Examples/bin/$CONFIG/net8.0/"
if [ "$CREATE_PACKAGE" = true ]; then
    echo "  Package: ./nupkg/"
fi
