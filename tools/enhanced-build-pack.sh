#!/bin/bash

# Enhanced build and pack script for Linux
# This script builds and packages the ModularGodot.Core project

set -e  # Exit on any error

# Function to print colored output
print_info() {
    echo -e "\033[36m$1\033[0m"
}

print_success() {
    echo -e "\033[32m$1\033[0m"
}

print_warning() {
    echo -e "\033[33m$1\033[0m"
}

print_error() {
    echo -e "\033[31m$1\033[0m"
}

# Function to load environment variables from .env file
load_env_file() {
    local env_file="${1:-.env}"
    if [ -f "$env_file" ]; then
        print_info "Loading environment variables from $env_file"
        while IFS= read -r line; do
            # Skip comments and empty lines
            if [[ ! "$line" =~ ^[[:space:]]*# ]] && [[ -n "$line" ]]; then
                # Remove leading/trailing whitespace
                line=$(echo "$line" | sed 's/^[[:space:]]*//;s/[[:space:]]*$//')
                if [[ "$line" =~ ^([^=]+)=(.*)$ ]]; then
                    key="${BASH_REMATCH[1]}"
                    value="${BASH_REMATCH[2]}"
                    # Remove quotes if present
                    value=$(echo "$value" | sed "s/^['\"]\(.*\)['\"]$/\1/")
                    export "$key=$value"
                fi
            fi
        done < "$env_file"
    fi
}

# Function to clean directory safely
remove_directory_safely() {
    local path="$1"
    if [ -d "$path" ]; then
        if rm -rf "$path"; then
            [ "$VERBOSE" = "true" ] && echo "  Cleaned directory: $path"
        else
            print_warning "Failed to clean directory $path"
        fi
    fi
}

# Function to check if project exists
test_project_exists() {
    local project_path="$1"
    [ -f "$project_path" ]
}

# Function to build project
invoke_build_project() {
    local project_path="$1"
    local configuration="$2"
    local output_path="$3"

    local build_args=("dotnet" "build" "$project_path" "-c" "$configuration")

    if [ -n "$output_path" ]; then
        build_args+=("-o" "$output_path")
    fi

    if [ "$VERBOSE" != "true" ]; then
        build_args+=("--nologo")
    fi

    [ "$VERBOSE" = "true" ] && echo "  Executing: ${build_args[*]}"

    if [ "$VERBOSE" = "true" ]; then
        "${build_args[@]}"
    else
        "${build_args[@]}" > /dev/null 2>&1
    fi

    return $?
}

# Function to pack project
invoke_pack_project() {
    local project_path="$1"
    local configuration="$2"
    local output_path="$3"
    local include_symbols="$4"

    local pack_args=("dotnet" "pack" "$project_path" "-c" "$configuration" "-o" "$output_path")

    if [ "$include_symbols" = "true" ]; then
        pack_args+=("--include-symbols" "-p:SymbolPackageFormat=snupkg")
    fi

    if [ "$VERBOSE" != "true" ]; then
        pack_args+=("--nologo")
    fi

    [ "$VERBOSE" = "true" ] && echo "  Executing: ${pack_args[*]}"

    if [ "$VERBOSE" = "true" ]; then
        "${pack_args[@]}"
    else
        "${pack_args[@]}" > /dev/null 2>&1
    fi

    return $?
}

# Default values
CONFIGURATION="${BUILD_CONFIGURATION:-Release}"
VERBOSE="${VERBOSE_OUTPUT:-false}"
SKIP_CLEANUP="${CLEANUP_TEMP_FILES:-true}"
OUTPUT_DIRECTORY="${PACKAGE_OUTPUT_DIR:-packages}"
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BUILD_TEMP_DIR_NAME="${BUILD_TEMP_DIR:-build_temp}"
BUILD_TEMP_DIR="$PROJECT_ROOT/$BUILD_TEMP_DIR_NAME"
COLLECTED_DLLS_DIR_NAME="${COLLECTED_DLLS_DIR:-collected_dlls}"
COLLECTED_DLLS_DIR="$BUILD_TEMP_DIR/$COLLECTED_DLLS_DIR_NAME"

# Parse command line arguments
FORCE_REBUILD=false
INCLUDE_SYMBOLS=false
PACKAGES_TO_BUILD=()

while [[ $# -gt 0 ]]; do
    case $1 in
        -c|--configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        -v|--verbose)
            VERBOSE=true
            shift
            ;;
        --skip-cleanup)
            SKIP_CLEANUP=true
            shift
            ;;
        --force-rebuild)
            FORCE_REBUILD=true
            shift
            ;;
        -o|--output)
            OUTPUT_DIRECTORY="$2"
            shift 2
            ;;
        --include-symbols)
            INCLUDE_SYMBOLS=true
            shift
            ;;
        --packages)
            shift
            while [[ $# -gt 0 && "$1" != -* ]]; do
                PACKAGES_TO_BUILD+=("$1")
                shift
            done
            ;;
        -h|--help)
            echo "Usage: $0 [OPTIONS]"
            echo "Options:"
            echo "  -c, --configuration CONFIG     Build configuration (Debug/Release) [default: Release]"
            echo "  -v, --verbose                  Enable verbose output"
            echo "  --skip-cleanup                 Skip cleanup of temporary files"
            echo "  --force-rebuild                Force rebuild all packages"
            echo "  -o, --output DIR               Output directory for packages [default: packages]"
            echo "  --include-symbols              Include symbols in packages"
            echo "  --packages PKG1 PKG2...        Build specific packages only"
            echo "  -h, --help                     Show this help message"
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            exit 1
            ;;
    esac
done

# Convert SKIP_CLEANUP to match the logic from PowerShell script
# In PowerShell: -not [System.Convert]::ToBoolean($env:CLEANUP_TEMP_FILES)
# PowerShell treats "true"/"1"/non-empty as true, so we need to invert
if [ "$SKIP_CLEANUP" = "false" ]; then
    DO_CLEANUP=true
else
    DO_CLEANUP=false
fi

print_success "Starting enhanced build and pack process for Linux..."
print_info "Configuration: $CONFIGURATION"
print_info "Output Directory: $OUTPUT_DIRECTORY"
[ "$FORCE_REBUILD" = "true" ] && print_info "Force Rebuild: Enabled"
[ "$INCLUDE_SYMBOLS" = "true" ] && print_info "Include Symbols: Enabled"
[ "$VERBOSE" = "true" ] && print_info "Verbose Output: Enabled"

# Load environment variables
load_env_file "$PROJECT_ROOT/.env"

# Create necessary directories
mkdir -p "$OUTPUT_DIRECTORY"
mkdir -p "$BUILD_TEMP_DIR"
mkdir -p "$COLLECTED_DLLS_DIR"

# Define packages to build
declare -A ALL_PACKAGES=(
    ["ModularGodot.Core.Contracts"]="$PROJECT_ROOT/src/ModularGodot.Core.Contracts/ModularGodot.Core.Contracts.csproj"
    ["ModularGodot.Core.Contexts"]="$PROJECT_ROOT/src/ModularGodot.Core.Contexts/ModularGodot.Core.Contexts.csproj"
    ["ModularGodot.Core.Infrastructure"]="$PROJECT_ROOT/src/ModularGodot.Core.Infrastructure/ModularGodot.Core.Infrastructure.csproj"
    ["ModularGodot.Core.Repositories"]="$PROJECT_ROOT/src/ModularGodot.Core.Repositories/ModularGodot.Core.Repositories.csproj"
    ["ModularGodot.Core"]="$PROJECT_ROOT/src/ModularGodot.Core/ModularGodot.Core.csproj"
)

# Clean output directory if force rebuild
if [ "$FORCE_REBUILD" = "true" ] && [ "$DO_CLEANUP" = "true" ]; then
    print_info "Cleaning output directory..."
    remove_directory_safely "$OUTPUT_DIRECTORY"
    mkdir -p "$OUTPUT_DIRECTORY"
fi

# Build packages
print_info "Building packages..."

BUILD_SUCCESS=true

# If specific packages requested, filter them
if [ ${#PACKAGES_TO_BUILD[@]} -gt 0 ]; then
    print_info "Building specific packages: ${PACKAGES_TO_BUILD[*]}"
    # Create a temporary associative array with only requested packages
    declare -A FILTERED_PACKAGES
    for pkg in "${PACKAGES_TO_BUILD[@]}"; do
        if [[ -n "${ALL_PACKAGES[$pkg]}" ]]; then
            FILTERED_PACKAGES["$pkg"]="${ALL_PACKAGES[$pkg]}"
        else
            print_warning "Unknown package: $pkg"
        fi
    done
else
    # Use all packages
    declare -A FILTERED_PACKAGES
    for pkg in "${!ALL_PACKAGES[@]}"; do
        FILTERED_PACKAGES["$pkg"]="${ALL_PACKAGES[$pkg]}"
    done
    print_info "Building all packages"
fi

# Build each package
for package_name in "${!FILTERED_PACKAGES[@]}"; do
    project_path="${FILTERED_PACKAGES[$package_name]}"

    print_info "Processing package: $package_name"

    # Check if project file exists
    if ! test_project_exists "$project_path"; then
        print_warning "Project file not found: $project_path"
        BUILD_SUCCESS=false
        continue
    fi

    # Build the project
    print_info "  Building project..."
    if ! invoke_build_project "$project_path" "$CONFIGURATION"; then
        print_error "Project build failed: $package_name"
        BUILD_SUCCESS=false
        continue
    fi

    # Package the project
    print_info "  Creating NuGet package..."
    if invoke_pack_project "$project_path" "$CONFIGURATION" "$OUTPUT_DIRECTORY" "$INCLUDE_SYMBOLS"; then
        print_success "  Package $package_name created successfully"
    else
        print_warning "Package creation completed with warnings or errors: $package_name"
    fi
done

# Display results
if [ "$BUILD_SUCCESS" = "true" ]; then
    print_success "Enhanced build and pack process completed successfully!"
else
    print_warning "Enhanced build and pack process completed with some errors"
fi

# List generated packages
if [ -d "$OUTPUT_DIRECTORY" ]; then
    generated_packages=("$OUTPUT_DIRECTORY"/*.nupkg)
    if [ -f "${generated_packages[0]}" ]; then
        print_info "Generated packages:"
        for package in "${generated_packages[@]}"; do
            if [ -f "$package" ]; then
                package_name=$(basename "$package")
                package_size=$(du -k "$package" | cut -f1)
                echo "  $package_name (Size: ${package_size} KB)"
            fi
        done
    else
        print_warning "No packages were generated"
    fi
else
    print_warning "Output directory does not exist: $OUTPUT_DIRECTORY"
fi

print_info "Packages output directory: $OUTPUT_DIRECTORY"

# Cleanup if not skipped
if [ "$DO_CLEANUP" = "true" ]; then
    print_info "Cleaning up temporary files..."
    remove_directory_safely "$BUILD_TEMP_DIR"
fi

if [ "$BUILD_SUCCESS" = "true" ]; then
    exit 0
else
    exit 1
fi