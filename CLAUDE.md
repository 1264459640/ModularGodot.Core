# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 🏗️ Architecture Overview

This is a modular Godot framework built with a layered architecture pattern using modern C# design patterns. The architecture follows clean architecture principles with clear separation of concerns:

### Layered Structure

```
src/
├── 0_Contracts/          # Contract layer - interface definitions and DTOs
│   ├── Abstractions/     # Core abstract interfaces
│   ├── Attributes/       # Custom attributes
│   └── Events/          # Event definitions
├── 1_Contexts/          # Context layer - dependency injection configuration
├── 2_Infrastructure/    # Infrastructure layer - concrete implementations
│   ├── Caching/         # Cache services
│   ├── Logging/         # Logging services
│   ├── Messaging/       # Message passing
│   ├── Monitoring/      # Performance monitoring
│   ├── ResourceLoading/ # Resource loading
│   └── ResourceManagement/ # Resource management
├── 3_Repositories/      # Repository layer - data access
└── ModularGodot.Core/   # Core project - package generation
```

## 🚀 Common Commands

### Build Commands

To build the entire solution:
```bash
dotnet build src/ModularGodot.Core.sln
```

To build and package everything using the PowerShell script:
```bash
.\build-and-pack.ps1
```

To build with a specific configuration:
```bash
.\build-and-pack.ps1 -Configuration Release
```

To build with verbose output:
```bash
.\build-and-pack.ps1 -Verbose
```

### Clean Build
```bash
.\build-and-pack.ps1 -SkipCleanup:$false
```

### Manual Merge and Pack (New)
To manually merge and pack using ILRepack from the tools directory:
```bash
cd tools
.\manual-merge-pack-final-fixed.ps1
```

To run with verbose output:
```bash
cd tools
.\manual-merge-pack-final-fixed.ps1 -Verbose
```

To skip output directory cleanup:
```bash
cd tools
.\manual-merge-pack-final-fixed.ps1 -SkipOutputCleanup
```

### Build Individual NuGet Packages
To build individual NuGet packages:
```bash
# Build Contracts package
dotnet pack src/ModularGodot.Core.Contracts/ModularGodot.Core.Contracts.csproj -c Release -o packages

# Build Contexts package
dotnet pack src/ModularGodot.Core.Contexts/ModularGodot.Core.Contexts.csproj -c Release -o packages

# Build Infrastructure package
dotnet pack src/ModularGodot.Core.Infrastructure/ModularGodot.Core.Infrastructure.csproj -c Release -o packages

# Build Repositories package
dotnet pack src/ModularGodot.Core.Repositories/ModularGodot.Core.Repositories.csproj -c Release -o packages

# Build full framework package
dotnet pack src/ModularGodot.Core/ModularGodot.Core.csproj -c Release -o packages
```

### Build All NuGet Packages
To build all NuGet packages at once:
```bash
dotnet pack src/ModularGodot.Core.sln -c Release -o packages
```

Or using the new multi-package build script:
```bash
.\build-multiple-packages.ps1
```

### Cleanup Temporary Files (New)
To cleanup temporary files and directories:
```bash
cd tools
.\cleanup-temp-files.ps1
```

## 🧪 Testing

Integration tests have been moved to a separate test project located at `D:\GodotProjects\ModularGodot.Core.Test`. To run tests:
```bash
cd ../ModularGodot.Core.Test/src/ModularGodot.Core.Test
dotnet test
```

Unit tests are located in the `4_UniTests` directory. To run unit tests:
```bash
dotnet test src/4_UniTests/
```

## 📦 Key Components

### Event System
- **IEventBus**: Event publishing and subscription interface
- **IEventSubscriber**: Subscription-only interface
- **R3EventBus**: High-performance event bus implementation based on R3

### Mediator Pattern
- **IMyMediator**: Custom mediator interface (no MediatR dependency)
- **ICommand/IQuery**: Command and query interfaces
- **MediatRAdapter**: MediatR adapter implementation

### Cache System
- **ICacheService**: Cache service abstraction
- **MemoryCacheService**: Memory cache implementation

### Monitoring System
- **IPerformanceMonitor**: Performance monitoring interface
- **IMemoryMonitor**: Memory monitoring interface

## 🔧 Technology Stack

- **Godot 4.4.1** - Game engine
- **.NET 9.0** - Runtime platform
- **Autofac 8.4.0** - IoC container
- **MediatR 13.0.0** - Mediator pattern implementation
- **R3 1.3.0** - Reactive programming library
- **Microsoft.Extensions.Caching.Memory 9.0.9** - Memory caching
- **System.Reactive 6.0.2** - Rx.NET

## 📁 Project Structure Guidelines

1. **Define interfaces** in `0_Contracts/Abstractions`
2. **Implement functionality** in `2_Infrastructure`
3. **Register services** in `1_Contexts`
4. **Write tests** in `4_UniTests`
5. **Create NuGet packages** in `src/ModularGodot.Core.*` directories
   - Each package project should reference the corresponding implementation project
   - Package projects should define proper dependencies in their .csproj files

## ⚙️ Configuration

The project now supports configuration through a `.env` file located in the project root. This file can be used to customize various aspects of the build and packaging process.

### Configuration Variables

- `BUILD_CONFIGURATION` - Build configuration (Release/Debug)
- `OUTPUT_BASE_DIR` - Base output directory name
- `BUILD_TEMP_DIR` - Build temporary directory name
- `COLLECTED_DLLS_DIR` - Collected DLLs directory name
- `PACKAGE_OUTPUT_DIR` - Package output directory name
- `CLEANUP_TEMP_FILES` - Whether to cleanup temporary files
- `CLEANUP_OUTPUT_DIR` - Whether to cleanup output directory
- `VERBOSE_OUTPUT` - Enable verbose output
- `ILREPACK_PATH_1` - Primary path to ILRepack tool
- `ILREPACK_PATH_2` - Secondary path to ILRepack tool

### Tools Directory

Custom scripts and tools have been moved to the `tools/` directory:
- `manual-merge-pack-final-fixed.ps1` - Manual merge and packaging script
- `cleanup-temp-files.ps1` - Temporary files cleanup script

## 🎯 Development Principles

- Prefer interfaces over concrete classes
- Follow asynchronous programming patterns
- Use event-driven architecture appropriately
- Pay attention to memory management and performance optimization
- Write clear documentation and comments