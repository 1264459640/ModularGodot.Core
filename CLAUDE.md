# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ModularGodot.Core is an enterprise-level framework designed for Godot game development, featuring a layered architecture and modern C# design patterns. The project provides complete infrastructure support including:

- Layered architecture with clear separation of concerns
- Event-driven system based on R3 reactive event system
- Mediator pattern for decoupled command and query processing
- Resource management with intelligent caching and memory monitoring
- Performance monitoring with real-time metrics collection
- Dependency injection based on Autofac IoC container

## Development Commands

### Build Commands

```bash
# Build the entire solution
dotnet build src/ModularGodot.Core.sln

# Build with specific configuration
dotnet build src/ModularGodot.Core.sln -c Release

# Build individual projects
dotnet build src/ModularGodot.Core.Contracts/ModularGodot.Core.Contracts.csproj
dotnet build src/ModularGodot.Core.Contexts/ModularGodot.Core.Contexts.csproj
dotnet build src/ModularGodot.Core.Infrastructure/ModularGodot.Core.Infrastructure.csproj
dotnet build src/ModularGodot.Core.Repositories/ModularGodot.Core.Repositories.csproj
```

### Test Commands

```bash
# Run Godot scene-based integration tests
# Open the project in Godot editor and run test scenes
# Select test scenes and click RunTest button

# Build the test project
dotnet build src/ModularGodot.Core.Test/ModularGodot.Core.Test.csproj
```

### NuGet Package Commands

```bash
# Pack all individual packages
dotnet pack src/ModularGodot.Core.sln -c Release -o packages

# Pack individual packages
dotnet pack src/ModularGodot.Core.Contracts/ModularGodot.Core.Contracts.csproj -c Release -o packages
dotnet pack src/ModularGodot.Core.Contexts/ModularGodot.Core.Contexts.csproj -c Release -o packages
dotnet pack src/ModularGodot.Core.Infrastructure/ModularGodot.Core.Infrastructure.csproj -c Release -o packages
dotnet pack src/ModularGodot.Core.Repositories/ModularGodot.Core.Repositories.csproj -c Release -o packages

# Pack complete framework package
dotnet pack src/ModularGodot.Core/ModularGodot.Core.csproj -c Release -o packages
```

### PowerShell Scripts

```bash
# Enhanced build and pack with cleanup
./tools/enhanced-build-pack.ps1 -Configuration Release

# Cleanup build artifacts
./tools/cleanup.ps1
```

## Key Architectural Components

### Event System

The event system is built on R3 reactive extensions:

- `IEventBus`: Event publishing and subscription interface
- `R3EventBus`: High-performance event bus implementation based on R3
- Supports async/sync publishing, conditional subscriptions, and one-time subscriptions

### Mediator Pattern

The mediator pattern uses MediatR with custom adapters:

- `IDispatcher`: Custom mediator interface (no MediatR dependency)
- `IMyMediator`: Custom mediator interface (no MediatR dependency)
- `MediatRAdapter`: MediatR adapter implementation with:
  - Full cancellation token support (including timeouts and cooperative cancellation)
  - Exception propagation to callers
  - Compile-time type safety through generic constraints
  - Singleton dependency injection configuration
  - Custom HandlerNotFoundException for missing handlers
- `ICommand/IQuery`: Command and query interfaces
- `ICommandHandler/IQueryHandler`: Command and query handler interfaces
- `CommandWrapper/QueryWrapper`: MediatR request wrappers for type-safe adaptation
- `CommandHandlerWrapper/QueryHandlerWrapper`: MediatR handler wrappers that bridge framework interfaces and MediatR

**Implementation Details**:
- Uses generic constraints for compile-time type safety
- Configured through Autofac dependency injection with Singleton registration
- Optimized for <1ms median routing time performance
- Automatically wraps framework commands/queries as MediatR requests
- Automatically wraps handlers as MediatR request handlers

### Cache System

Resource caching with configurable strategies:

- `ICacheService`: Cache service abstraction
- `MemoryCacheService`: Memory cache implementation
- Configurable cache strategies (ResourceCacheStrategy enum)

### Monitoring System

Performance and memory monitoring:

- `IPerformanceMonitor`: Performance monitoring interface
- `IMemoryMonitor`: Memory monitoring interface
- Real-time metrics collection and reporting

## Dependency Injection

The project uses Autofac for dependency injection with custom attributes:

- `[Injectable(Lifetime.Singleton/Scoped/Transient)]`: Marks classes for automatic registration
- `Lifetime` enum: Defines service lifecycles (Singleton, Scoped, Transient)
- Module-based configuration in the Contexts layer

## Godot Integration

### Global Service Node

The project provides a `GodotGlobalService` node for easy access to core services in Godot environments:

1. Add the `GodotGlobalService` node to the root of your scene tree
2. Access core services through the singleton instance:

```csharp
// Get the global service instance
var globalService = GodotGlobalService.Instance;

// Resolve the dispatcher interface
var dispatcher = globalService.ResolveDispatcher();

// Resolve the event bus interface
var eventBus = globalService.ResolveEventBus();
```

### Key Components

- `GodotGlobalService`: Global singleton node for service resolution
- Located in `src/ModularGodot.Core/GlobalServices/`
- Provides access to `IDispatcher` and `IEventBus` interfaces
- Manages the lifecycle of the dependency injection container

## Integration Testing

The project now includes comprehensive Godot scene-based integration tests to verify component communication and package completeness:

### Test Structure
- **Mediator Communication Tests**: Verify command and query routing through the Mediator pattern using Godot scenes
- **EventBus Communication Tests**: Validate event publishing and subscription through the R3 EventBus using Godot scenes
- **Package Completeness Tests**: Ensure all NuGet packages are present and functional using Godot scenes

### Test Execution
- Integration tests are implemented as Godot scenes in the ModularGodot.Core.Test project
- Tests depend only on ModularGodot.Core and ModularGodot.Core.Contracts packages
- Tests access mediator and event bus through MiddlewareProvider AutoLoad singleton
- Tests can be run directly in the Godot editor by selecting scenes and clicking RunTest buttons
- Tests are designed to run only in development environments
- Only successful scenarios are validated (no error handling tests)

### Godot Scene Testing
Integration tests use Godot scenes to simulate production-like scenarios within the Godot engine environment. Tests leverage the MiddlewareProvider AutoLoad to access framework services.

## NuGet Package Structure

The project supports multiple usage patterns:

1. **Individual Layer Packages** (Recommended):
   - `ModularGodot.Core.Contracts` - Contracts layer
   - `ModularGodot.Core.Contexts` - Contexts layer
   - `ModularGodot.Core.Infrastructure` - Infrastructure layer
   - `ModularGodot.Core.Repositories` - Repositories layer

2. **Complete Framework Package**:
   - `ModularGodot.Core` - Contains all layers

The dependency hierarchy follows the architecture layers:
```
ModularGodot.Core.Repositories
  ↓ Depends on
ModularGodot.Core.Infrastructure
  ↓ Depends on
ModularGodot.Core.Contexts
  ↓ Depends on
ModularGodot.Core.Contracts
```

## Documentation

For detailed architecture information, see [ARCHITECTURE.md](docs/ARCHITECTURE.md)

For NuGet package documentation, see:
- [NuGet Packages](docs/NUGET_PACKAGES.md)

For plugin architecture documentation, see:
- [Plugin Architecture](docs/PLUGIN_ARCHITECTURE.md)

For dependency injection documentation, see:
- [Dependency Injection](docs/DEPENDENCY_INJECTION.md)
