#!/bin/bash

# Cleanup script for Linux
# This script cleans up build artifacts and temporary files

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

# Function to log verbose messages
log_verbose() {
    if [ "$VERBOSE" = "true" ]; then
        echo -e "\033[90m$1\033[0m"
    fi
}

# Set paths
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

# Load environment variables from .env file
if [ -f "$PROJECT_ROOT/.env" ]; then
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
    done < "$PROJECT_ROOT/.env"
fi

BUILD_TEMP_DIR_NAME="${BUILD_TEMP_DIR:-build_temp}"
BUILD_TEMP_DIR="$PROJECT_ROOT/$BUILD_TEMP_DIR_NAME"

COLLECTED_DLLS_DIR_NAME="${COLLECTED_DLLS_DIR:-collected_dlls}"
COLLECTED_DLLS_DIR="$BUILD_TEMP_DIR/$COLLECTED_DLLS_DIR_NAME"

PACKAGE_OUTPUT_DIR="${PACKAGE_OUTPUT_DIR:-packages}"
PACKAGE_OUTPUT_PATH="$PROJECT_ROOT/$PACKAGE_OUTPUT_DIR"

# Parse command line arguments
VERBOSE=false

while [[ $# -gt 0 ]]; do
    case $1 in
        -v|--verbose)
            VERBOSE=true
            shift
            ;;
        -h|--help)
            echo "Usage: $0 [OPTIONS]"
            echo "Options:"
            echo "  -v, --verbose    Enable verbose output"
            echo "  -h, --help       Show this help message"
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            exit 1
            ;;
    esac
done

print_success "Starting cleanup process..."

# Clean build temp directory
if [ -d "$BUILD_TEMP_DIR" ]; then
    log_verbose "Removing build temp directory: $BUILD_TEMP_DIR"
    rm -rf "$BUILD_TEMP_DIR"
    print_success "Build temp directory cleaned"
else
    log_verbose "Build temp directory does not exist: $BUILD_TEMP_DIR"
fi

# Clean package output directory
if [ -d "$PACKAGE_OUTPUT_PATH" ]; then
    log_verbose "Removing package output directory: $PACKAGE_OUTPUT_PATH"
    rm -rf "$PACKAGE_OUTPUT_PATH"
    print_success "Package output directory cleaned"
else
    log_verbose "Package output directory does not exist: $PACKAGE_OUTPUT_PATH"
fi

# Clean bin and obj folders in src directory
print_info "Cleaning bin and obj folders in src directory..."
find "$PROJECT_ROOT/src" -type d \( -name "bin" -o -name "obj" \) -exec rm -rf {} + 2>/dev/null || true

print_success "Cleanup process completed!"