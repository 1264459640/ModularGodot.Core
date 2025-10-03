#!/bin/bash

# Script to run all xUnit tests for the ModularGodot.Core project

echo "Starting xUnit tests for ModularGodot.Core..."

# Navigate to the project root if not already there
cd "$(dirname "$0")"

# Run the tests
dotnet test ../src/ModularGodot.Core.XUnitTests/ModularGodot.Core.XUnitTests.csproj

# Check the exit code
if [ $? -eq 0 ]; then
    echo "All tests passed successfully!"
else
    echo "Some tests failed. Please check the output above."
    exit 1
fi
