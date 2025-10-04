#!/bin/bash

# Ombi Development Setup Script
# This script helps new contributors set up their development environment

set -e

echo "🚀 Setting up Ombi development environment..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if we're in the right directory
if [ ! -f "src/Ombi/Ombi.csproj" ]; then
    print_error "Please run this script from the Ombi root directory"
    exit 1
fi

# Check for required tools
print_status "Checking for required tools..."

# Check .NET
if ! command -v dotnet &> /dev/null; then
    print_error ".NET 8.0 SDK is required but not installed"
    print_status "Please install from: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
else
    DOTNET_VERSION=$(dotnet --version)
    print_success "Found .NET version: $DOTNET_VERSION"
fi

# Check Node.js
if ! command -v node &> /dev/null; then
    print_error "Node.js is required but not installed"
    print_status "Please install from: https://nodejs.org/"
    exit 1
else
    NODE_VERSION=$(node --version)
    print_success "Found Node.js version: $NODE_VERSION"
fi

# Check Yarn
if ! command -v yarn &> /dev/null; then
    print_error "Yarn is required but not installed"
    print_status "Please install from: https://yarnpkg.com/"
    exit 1
else
    YARN_VERSION=$(yarn --version)
    print_success "Found Yarn version: $YARN_VERSION"
fi

# Check Git
if ! command -v git &> /dev/null; then
    print_error "Git is required but not installed"
    exit 1
else
    print_success "Found Git"
fi

print_status "All required tools are installed!"

# Restore .NET dependencies
print_status "Restoring .NET dependencies..."
if dotnet restore; then
    print_success ".NET dependencies restored"
else
    print_error "Failed to restore .NET dependencies"
    exit 1
fi

# Install frontend dependencies
print_status "Installing frontend dependencies..."
if yarn --cwd ./src/Ombi/ClientApp install; then
    print_success "Frontend dependencies installed"
else
    print_error "Failed to install frontend dependencies"
    exit 1
fi

# Build the project
print_status "Building the project..."
if dotnet build; then
    print_success "Project built successfully"
else
    print_error "Failed to build project"
    exit 1
fi

# Run tests
print_status "Running tests..."
if dotnet test; then
    print_success "All tests passed"
else
    print_warning "Some tests failed - this might be expected for a fresh setup"
fi

# Create development configuration
print_status "Setting up development configuration..."
if [ ! -f "src/Ombi/appsettings.Development.json" ]; then
    print_warning "Development configuration not found - you may need to create one"
fi

print_success "Development environment setup complete!"
echo ""
echo "🎉 You're ready to start contributing to Ombi!"
echo ""
echo "Next steps:"
echo "1. Read the CONTRIBUTING.md guide"
echo "2. Join our Discord: https://discord.gg/Sa7wNWb"
echo "3. Look for 'good first issue' labels"
echo "4. Start coding!"
echo ""
echo "To run the application:"
echo "  Backend:  dotnet run --project src/Ombi"
echo "  Frontend: yarn --cwd ./src/Ombi/ClientApp start"
echo ""
echo "Happy coding! 🚀"
