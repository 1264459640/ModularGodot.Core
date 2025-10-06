<!--
SYNC IMPACT REPORT:
Version change: N/A → 1.0.0
Added sections: Plugin Development and Architecture Standards, Development Workflow and Quality Standards
Modified principles: All 5 principles added as new
Templates requiring updates:
  - .specify/templates/plan-template.md ✅ updated (Constitution Check section now reflects ModularGodot.Core principles)
  - .specify/templates/spec-template.md ✅ updated (Scope/requirements alignment with plugin architecture)
  - .specify/templates/tasks-template.md ✅ updated (Plugin architecture task categorization reflected)
Follow-up TODOs:
  - TODO(RATIFICATION_DATE): Original adoption date unknown
-->
# ModularGodot.Core Constitution

## Core Principles

### I. Plugin-First Architecture
All functionality is designed as a pluggable component with clear contracts; All services must be module-based with well-defined interfaces; Each plugin must have both a contract layer and an implementation layer to ensure loose coupling and support the framework's modular architecture.
Rationale: Enables modular development, allows for easy extension and replacement of components, promotes better design through clear interfaces. This principle aligns with the project's strong emphasis on plugin-based architecture.

### II. Automated Dependency Management
All services are automatically registered via the `@Injectable` attribute system; Dependencies must be resolved through the Autofac IoC container with clear lifetime management; Service providers declare dependencies through constructor injection with no direct instantiation.
Rationale: Reduces boilerplate code, ensures consistent service registration, eliminates manual container configuration mistakes, and enables cleaner code architecture. This principle reflects the automated dependency injection mechanism in the project.

### III. Contract-Driven Development (NON-NEGOTIABLE)
All communication between components must use interface contracts; All commands and queries define their behavior through the ICommand/IQuery interfaces; Events are published via the event bus using typed event contracts.
Rationale: Ensures loose coupling between components, supports testability via mock implementations, enables better separation of concerns, and provides clear guidelines for extension points. All components must adhere to this contract-first approach.

### IV. Event-Driven Architecture
Communication between loosely coupled components happens through the R3 event system; Publishers should not know about subscribers, and decoupling must be maintained; Events are immutable classes with clear semantic meaning and complete context.
Rationale: Enables loose coupling, supports asynchronous processing, allows for multiple consumers of the same event, and helps maintain clean architectural boundaries between different components of the system.

### V. Mediator Pattern for Command/Query Separation
All commands and queries must use the mediator pattern via the IDispatcher interface; Business logic is separated from command/query handling through dedicated handler implementations; Each command/query pair has a single, focused responsibility and returns consistent responses.
Rationale: Reduces direct dependencies between components, provides a centralized approach to command and query handling, enables cross-cutting concerns, and enforces separation of commands (change state) and queries (return data). This ensures maintainable and testable code.

## Plugin Development and Architecture Standards
All plugins must follow the dual-package structure: PluginName.Contracts (for shared contracts) and PluginName (for implementation).
Follow strict dependency order - Repository → Infrastructure → Contexts → Contracts; Lower layers cannot depend on higher layers.
Implementers must ensure <1ms command routing performance where possible; Memory monitors should automatically manage resources via the provided IMemoryMonitor interface.
All plugins must include comprehensive unit tests, and contracts must include contract tests.

## Development Workflow and Quality Standards
Follow .NET standard naming conventions; use meaningful variable names and comments where complexity is not obvious from code.
Follow clear, descriptive commit messages; reference related issue numbers; use present tense imperative mood.
All contributions require at least one peer review before merging; Focus on architectural alignment with stated principles.
Updates to documentation required when adding or changing significant functionality; Commits should reference documentation updates in TODO comments if planned.

## Governance
The constitution supersedes all other development practices; All changes to library architecture must align with these principles; Use deps/README.md and CONTRIBUTING.md for runtime development guidance; Any violations must be documented with reason and approval from maintainers.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE) | **Last Amended**: 2025-10-05