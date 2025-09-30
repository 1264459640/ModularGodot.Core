# Repository Guidelines

## Project Structure & Module Organization

The repository follows a layered architecture with numbered directories for clear dependency flow:

- `src/0_Contracts/` - Interfaces, DTOs, and event definitions
- `src/1_Contexts/` - Dependency injection configuration
- `src/2_Infrastructure/` - Concrete implementations (caching, logging, messaging, etc.)
- `src/3_Repositories/` - Data access layer
- `src/4_UniTests/` - Unit tests
- `packages/` - Generated NuGet packages
- `tools/` - Build and maintenance scripts

Dependencies flow inward only - higher-numbered layers depend on lower-numbered ones.

## Build, Test, and Development Commands

- `dotnet build src/ModularGodot.Core.sln` - Build the entire solution
- `dotnet test src/4_UniTests/ModularGodot.Core.Tests.csproj` - Run unit tests
- `.	ools\manual-merge-pack-final-fixed.ps1` - Build and package all modules
- `.	ools\cleanup-temp-files.ps1` - Clean build artifacts and temporary files

## Coding Style & Naming Conventions

- Use 4-space indentation (no tabs)
- Follow C# naming conventions: PascalCase for public members, camelCase for private members
- Namespace structure mirrors directory structure (e.g., `ModularGodot.Contracts.Abstractions`)
- Interface names prefixed with `I` (e.g., `IEventBus`, `ICacheService`)
- Async methods suffixed with `Async`
- Prefer interfaces over concrete implementations

## Testing Guidelines

- Write unit tests for all new functionality in `src/4_UniTests/`
- Test files should mirror the structure of the code being tested
- Use descriptive test method names that indicate the scenario being tested
- Aim for high test coverage of core functionality
- Run tests with `dotnet test` before submitting pull requests

## Commit & Pull Request Guidelines

- Follow conventional commits format: `type(scope): description`
  - Types: `feat`, `fix`, `refactor`, `chore`, `docs`, `test`
  - Examples: `feat(事件总线): 添加事件基类`, `refactor: 重构项目结构`
- Pull requests must include:
  - Clear description of changes
  - Reference to related issues (if applicable)
  - Updated documentation for new features
  - Passing tests
- Keep PRs focused on a single concern or feature

## Architecture Principles

- Follow SOLID principles and dependency inversion
- Maintain strict layer separation - no circular dependencies
- Use event-driven architecture for cross-cutting concerns
- Implement mediator pattern for command/query handling
- Prioritize performance and memory efficiency for game development context
