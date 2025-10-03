
# Implementation Plan: Mediator Pattern Adapter Fix Solution

**Branch**: `001-mnt-d-godotprojects` | **Date**: 2025-10-03 | **Spec**: /mnt/d/GodotProjects/ModularGodot.Core/specs/001-mnt-d-godotprojects/spec.md
**Input**: Feature specification from `/specs/001-mnt-d-godotprojects/spec.md`

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
The primary requirement is to fix and improve the Mediator pattern adapter implementation in the ModularGodot.Core framework. The current implementation uses MediatR as the underlying mediator but needs improvements to properly handle command and query routing, exception propagation, cancellation token support, and type safety.

Technical approach from research: Implement a robust MediatR adapter that correctly wraps commands and queries, properly handles cancellation tokens with full support including timeouts, propagates exceptions to callers, maintains compile-time type safety through generic constraints, and supports singleton dependency injection through the Contexts layer configuration.

## Technical Context
**Language/Version**: C# .NET 9.0
**Primary Dependencies**: MediatR, Autofac, R3
**Storage**: N/A
**Testing**: xUnit
**Target Platform**: Godot Game Engine
**Project Type**: single - C# class library project
**Performance Goals**: Low-latency message routing (<1ms median)
**Constraints**: Must maintain backward compatibility with existing interfaces
**Scale/Scope**: Part of ModularGodot.Core framework (moderate scale)

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Since the constitution file contains placeholders, the following gates will be applied based on standard software development principles:

1. **Library-First**: Implementation must be structured as a self-contained library component within the ModularGodot.Core framework
2. **Test-First**: All changes must be accompanied by failing tests before implementation
3. **Integration Testing**: Focus on contract tests for the MediatR adapter interface
4. **Simplicity**: Implementation should be minimal and focused on the core requirements
5. **Backward Compatibility**: Must not break existing interfaces or functionality

All gates are expected to PASS.

## Project Structure

### Documentation (this feature)
```
specs/[###-feature]/
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
├── ModularGodot.Core.Contracts/
│   ├── Abstractions/
│   │   ├── Messaging/
│   │   │   ├── IDispatcher.cs
│   │   │   ├── ICommand.cs
│   │   │   ├── IQuery.cs
│   │   │   ├── ICommandHandler.cs
│   │   │   └── IQueryHandler.cs
│   │   └── [Other abstraction interfaces]
│   └── [Other contract elements]
├── ModularGodot.Core.Infrastructure/
│   ├── Messaging/
│   │   ├── MediatRAdapter.cs
│   │   ├── CommandWrapper.cs
│   │   ├── QueryWrapper.cs
│   │   ├── CommandHandlerWrapper.cs
│   │   └── QueryHandlerWrapper.cs
│   └── [Other infrastructure implementations]
├── ModularGodot.Core.Contexts/
│   ├── MediatorModule.cs
│   └── [Dependency injection configuration]
└── ModularGodot.Core.XUnitTests/
    ├── Mediator/
    │   └── MediatorTests.cs
    └── [Other test files]
```

**Structure Decision**: Single project structure - This is a C#/.NET solution with a layered architecture following the ModularGodot.Core framework structure. The implementation will focus on the Messaging components in the Infrastructure layer that interact with the MediatR library.

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
- Refactoring tasks for existing MediatR adapter components

**Ordering Strategy**:
- TDD order: Tests before implementation
- Dependency order: Models before services before UI
- Mark [P] for parallel execution (independent files)
- Priority order: Critical path items first (MediatRAdapter.cs, wrapper classes)

**Estimated Output**: 25-30 numbered, ordered tasks in tasks.md

**Key Task Categories**:
1. **Contract Tests**: Validate MediatR adapter interface contracts
2. **Implementation**: Update MediatRAdapter.cs with clarified requirements
3. **Wrapper Classes**: Enhance CommandWrapper, QueryWrapper, and handler wrappers
4. **Dependency Injection**: Ensure proper singleton registration in MediatorModule
5. **Integration Tests**: Validate end-to-end functionality with existing test suite
6. **Performance Tests**: Verify <1ms median routing time requirement
7. **Backward Compatibility**: Ensure existing functionality remains intact

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
- [x] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS
- [x] All NEEDS CLARIFICATION resolved
- [ ] Complexity deviations documented

---
*Based on Constitution v2.1.1 - See `/memory/constitution.md`*
