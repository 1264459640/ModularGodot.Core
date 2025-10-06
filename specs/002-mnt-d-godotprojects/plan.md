
# Implementation Plan: Event Bus System Fix

**Branch**: `002-mnt-d-godotprojects` | **Date**: 2025-10-05 | **Spec**: [/mnt/d/GodotProjects/ModularGodot.Core/specs/002-mnt-d-godotprojects/spec.md](file:///mnt/d/GodotProjects/ModularGodot.Core/specs/002-mnt-d-godotprojects/spec.md)
**Input**: Feature specification from `/specs/002-mnt-d-godotprojects/spec.md`

## Execution Flow (/plan command scope)
```
1. Load feature spec from Input path
   → If not found: ERROR "No feature spec at {path}"
2. Fill Technical Context (scan for NEEDS CLARIFICATION)
   → Detect Project Type from file system structure or context (web=frontend+backend, mobile=app+api)
   → Set Structure Decision based on project type
3. Fill the Constitution Check section based on the content of the constitution document.
4. Evaluate Constitution Check section below
   → If violations exist: Document in Complexity Tracking
   → If no justification possible: ERROR "Simplify approach first"
   → Update Progress Tracking: Initial Constitution Check
5. Execute Phase 0 → research.md
   → If NEEDS CLARIFICATION remain: ERROR "Resolve unknowns"
6. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent-specific template file (e.g., `CLAUDE.md` for Claude Code, `.github/copilot-instructions.md` for GitHub Copilot, `GEMINI.md` for Gemini CLI, `QWEN.md` for Qwen Code, or `AGENTS.md` for all other agents).
7. Re-evaluate Constitution Check section
   → If new violations: Refactor design, return to Phase 1
   → Update Progress Tracking: Post-Design Constitution Check
8. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)
9. STOP - Ready for /tasks command
```

**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:
- Phase 2: /tasks command creates tasks.md
- Phase 3-4: Implementation execution (manual or via tools)

## Summary
Fix memory leaks and thread safety issues in the existing R3 Event Bus implementation while maintaining contract-driven architecture. The solution will implement proper resource cleanup mechanisms to ensure memory usage stays under 100MB and thread-safe event processing for up to 1,000 events per second. All changes will adhere to ModularGodot.Core's constitutional principles including contract-first development, automated dependency injection, and proper layered architecture.

## Technical Context
**Language/Version**: C# .NET 8.0
**Primary Dependencies**: R3 reactive extensions, MediatR, Autofac IoC container, ModularGodot.Core.Contracts, ModularGodot.Core.Infrastructure
**Storage**: N/A (in-memory event system)
**Testing**: xUnit with Moq for mocking, including stress testing and memory monitoring
**Target Platform**: Cross-platform (Windows, Linux, macOS) game engine framework
**Project Type**: Single project with layered architecture (contracts, infrastructure, contexts, repositories)
**Performance Goals**: Support up to 1,000 events per second with memory usage under 100MB
**Constraints**: <1ms command routing, thread-safe operations, automatic resource cleanup, memory leak prevention
**Scale/Scope**: Component-level fix to existing event bus system with proper cleanup mechanisms

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Alignment with ModularGodot.Core Constitution:

1. **Plugin-First Architecture**: PASS - Event bus system will maintain clear contracts in ModularGodot.Core.Contracts with implementation in ModularGodot.Core.Infrastructure
2. **Automated Dependency Management**: PASS - Event bus will be registered via Autofac container with proper lifetime management
3. **Contract-Driven Development**: PASS - All event bus operations will use typed interfaces (IEventBus, EventBase) with contract-first approach
4. **Event-Driven Architecture**: PASS - Enhancement to existing R3-based event system while maintaining publisher/subscriber decoupling
5. **Mediator Pattern for Command/Query Separation**: PASS - Event bus system will work in conjunction with existing mediator pattern implementation

### Project Structure Compliance:
- Following layered architecture (Contracts → Infrastructure → Contexts → Repositories)
- All changes will maintain proper dependency direction
- Implementation will follow dual-package structure with clear separation of contracts and implementation

## Project Structure

### Documentation (this feature)
```
specs/002-mnt-d-godotprojects/
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 output (/plan command)
├── data-model.md        # Phase 1 output (/plan command)
├── quickstart.md        # Phase 1 output (/plan command)
├── contracts/           # Phase 1 output (/plan command)
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)
```
src/
├── ModularGodot.Core/              # Main framework package
├── ModularGodot.Core.Contracts/    # Contract interfaces and abstractions
│   ├── Abstractions/
│   │   ├── Messaging/            # IEventBus, EventBase interfaces
│   │   ├── Caching/
│   │   └── Logging/
│   ├── Attributes/               # Injectable attribute system
│   └── Events/                   # Event definitions
├── ModularGodot.Core.Infrastructure/ # Implementation layer
│   ├── Messaging/                # R3EventBus implementation, Query/Command wrappers
│   ├── Caching/
│   └── Logging/
├── ModularGodot.Core.Contexts/   # DI configuration and modules
└── ModularGodot.Core.Repositories/ # Data persistence layer

tests/
└── ModularGodot.Core.XUnitTests/ # Unit and integration tests
    ├── Events/                   # Event bus tests
    ├── Mediator/                 # Mediator tests
    └── DependencyInjection/      # DI container tests
```

**Structure Decision**: Making targeted improvements to existing R3EventBus implementation in infrastructure layer while preserving existing contracts.

## Phase 0: Outline & Research
1. **Extract unknowns from Technical Context** above:
   - For each NEEDS CLARIFICATION → research task
   - For each dependency → best practices task
   - For each integration → patterns task

2. **Generate and dispatch research agents**:
   ```
   For each unknown in Technical Context:
     Task: "Research {unknown} for {feature context}"
   For each technology choice:
     Task: "Find best practices for {tech} in {domain}"
   ```

3. **Consolidate findings** in `research.md` using format:
   - Decision: [what was chosen]
   - Rationale: [why chosen]
   - Alternatives considered: [what else evaluated]

**Output**: research.md with all NEEDS CLARIFICATION resolved

## Phase 1: Design & Contracts
*Prerequisites: research.md complete*

1. **Extract entities from feature spec** → `data-model.md`:
   - Entity name, fields, relationships
   - Validation rules from requirements
   - State transitions if applicable

2. **Generate API contracts** from functional requirements:
   - For each user action → endpoint
   - Use standard REST/GraphQL patterns
   - Output OpenAPI/GraphQL schema to `/contracts/`

3. **Generate contract tests** from contracts:
   - One test file per endpoint
   - Assert request/response schemas
   - Tests must fail (no implementation yet)

4. **Extract test scenarios** from user stories:
   - Each story → integration test scenario
   - Quickstart test = story validation steps

5. **Update agent file incrementally** (O(1) operation):
   - Run `.specify/scripts/bash/update-agent-context.sh claude`
     **IMPORTANT**: Execute it exactly as specified above. Do not add or remove any arguments.
   - If exists: Add only NEW tech from current plan
   - Preserve manual additions between markers
   - Update recent changes (keep last 3)
   - Keep under 150 lines for token efficiency
   - Output to repository root

**Output**: data-model.md, /contracts/*, failing tests, quickstart.md, agent-specific file

## Phase 2: Task Planning Approach
*This section describes what the /tasks command will do - DO NOT execute during /plan*

**Task Generation Strategy**:
- Load `.specify/templates/tasks-template.md` as base
- Generate tasks from Phase 1 design docs (contracts, data model, quickstart)
- Each contract → contract test task [P]
- Each entity → model creation task [P] 
- Each user story → integration test task
- Implementation tasks to make tests pass

**Ordering Strategy**:
- TDD order: Tests before implementation 
- Dependency order: Models before services before UI
- Mark [P] for parallel execution (independent files)

**Estimated Output**: 25-30 numbered, ordered tasks in tasks.md

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan

## Phase 3+: Future Implementation
*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution (/tasks command creates tasks.md)  
**Phase 4**: Implementation (execute tasks.md following constitutional principles)  
**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)

## Complexity Tracking
*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |


## Progress Tracking
*This checklist is updated during execution flow*

**Phase Status**:
- [x] Phase 0: Research complete (/plan command)
- [x] Phase 1: Design complete (/plan command)
- [x] Phase 2: Task planning complete (/plan command - describe approach only)
- [ ] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS
- [x] All NEEDS CLARIFICATION resolved
- [ ] Complexity deviations documented

---
*Based on Constitution v2.1.1 - See `/memory/constitution.md`*
