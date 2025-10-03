# Tasks: Mediator Pattern Adapter Fix Solution

**Input**: Design documents from `/specs/001-mnt-d-godotprojects/`
**Prerequisites**: plan.md (required), research.md, data-model.md, contracts/

## Execution Flow (main)
```
1. Load plan.md from feature directory
   → If not found: ERROR "No implementation plan found"
   → Extract: tech stack, libraries, structure
2. Load optional design documents:
   → data-model.md: Extract entities → model tasks
   → contracts/: Each file → contract test task
   → research.md: Extract decisions → setup tasks
3. Generate tasks by category:
   → Setup: project init, dependencies, linting
   → Tests: contract tests, integration tests
   → Core: models, services, CLI commands
   → Integration: DB, middleware, logging
   → Polish: unit tests, performance, docs
4. Apply task rules:
   → Different files = mark [P] for parallel
   → Same file = sequential (no [P])
   → Tests before implementation (TDD)
5. Number tasks sequentially (T001, T002...)
6. Generate dependency graph
7. Create parallel execution examples
8. Validate task completeness:
   → All contracts have tests?
   → All entities have models?
   → All endpoints implemented?
9. Return: SUCCESS (tasks ready for execution)
```

## Format: `[ID] [P?] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

## Path Conventions
- **Single project**: `src/`, `tests/` at repository root
- **Web app**: `backend/src/`, `frontend/src/`
- **Mobile**: `api/src/`, `ios/src/` or `android/src/`
- Paths shown below assume single project - adjust based on plan.md structure

## Phase 3.1: Setup
- [ ] T001 Verify C# .NET 9.0 project with MediatR, Autofac, R3 dependencies
- [ ] T002 [P] Configure xUnit test framework and dependencies
- [ ] T003 [P] Verify existing project structure matches plan.md

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3
**CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation**
- [ ] T004 [P] Contract test SendCommand functionality in /mnt/d/GodotProjects/ModularGodot.Core/specs/001-mnt-d-godotprojects/contracts/send-command-test.cs
- [ ] T005 [P] Contract test SendQuery functionality in /mnt/d/GodotProjects/ModularGodot.Core/specs/001-mnt-d-godotprojects/contracts/send-query-test.cs
- [ ] T006 [P] Integration test for command cancellation handling in src/ModularGodot.Core.XUnitTests/Mediator/CancellationTests.cs
- [ ] T007 [P] Integration test for query cancellation handling in src/ModularGodot.Core.XUnitTests/Mediator/CancellationTests.cs
- [ ] T008 [P] Integration test for exception propagation in src/ModularGodot.Core.XUnitTests/Mediator/ExceptionTests.cs
- [ ] T009 [P] Integration test for handler not found scenarios in src/ModularGodot.Core.XUnitTests/Mediator/HandlerNotFoundTests.cs

## Phase 3.3: Core Implementation (ONLY after tests are failing)
- [ ] T010 Enhance MediatRAdapter.cs to support full cancellation token handling
- [ ] T011 Improve MediatRAdapter.cs exception propagation
- [ ] T012 [P] Add performance optimization to MediatRAdapter.cs for <1ms routing
- [ ] T013 Update MediatorModule.cs to register adapter as Singleton instead of InstancePerLifetimeScope
- [ ] T014 [P] Enhance CommandWrapper.cs with better type safety
- [ ] T015 [P] Enhance QueryWrapper.cs with better type safety
- [ ] T016 [P] Add HandlerNotFoundException to Contracts layer
- [ ] T017 Refactor CommandHandlerWrapper.cs for improved performance
- [ ] T018 Refactor QueryHandlerWrapper.cs for improved performance

## Phase 3.4: Integration
- [ ] T019 Integrate enhanced MediatRAdapter with existing Mediator tests
- [ ] T020 Verify backward compatibility with existing ICommand/IQuery implementations
- [ ] T021 Test dependency injection configuration with Singleton registration
- [ ] T022 Validate performance meets <1ms median routing time requirement

## Phase 3.5: Polish
- [ ] T023 [P] Add unit tests for MediatRAdapter edge cases
- [ ] T024 [P] Add unit tests for wrapper classes
- [ ] T025 [P] Update documentation in docs/ARCHITECTURE.md with implementation details
- [ ] T026 [P] Update CLAUDE.md with any new implementation details
- [ ] T027 Run performance benchmark tests
- [ ] T028 Verify all existing tests still pass
- [ ] T029 Remove any code duplication
- [ ] T030 Run manual testing with quickstart examples

## Dependencies
- Tests (T004-T009) before implementation (T010-T018)
- T010-T011 block T019
- T013 blocks T021
- T016 blocks T009
- Implementation before polish (T023-T030)

## Parallel Example
```
# Launch T004-T009 together:
Task: "Contract test SendCommand functionality in /mnt/d/GodotProjects/ModularGodot.Core/specs/001-mnt-d-godotprojects/contracts/send-command-test.cs"
Task: "Contract test SendQuery functionality in /mnt/d/GodotProjects/ModularGodot.Core/specs/001-mnt-d-godotprojects/contracts/send-query-test.cs"
Task: "Integration test for command cancellation handling in src/ModularGodot.Core.XUnitTests/Mediator/CancellationTests.cs"
Task: "Integration test for query cancellation handling in src/ModularGodot.Core.XUnitTests/Mediator/CancellationTests.cs"
Task: "Integration test for exception propagation in src/ModularGodot.Core.XUnitTests/Mediator/ExceptionTests.cs"
Task: "Integration test for handler not found scenarios in src/ModularGodot.Core.XUnitTests/Mediator/HandlerNotFoundTests.cs"
```

## Notes
- [P] tasks = different files, no dependencies
- Verify tests fail before implementing
- Commit after each task
- Avoid: vague tasks, same file conflicts

## Task Generation Rules
*Applied during main() execution*

1. **From Contracts**:
   - Each contract file → contract test task [P]
   - Each endpoint → implementation task

2. **From Data Model**:
   - Each entity → model creation task [P]
   - Relationships → service layer tasks

3. **From User Stories**:
   - Each story → integration test [P]
   - Quickstart scenarios → validation tasks

4. **Ordering**:
   - Setup → Tests → Models → Services → Endpoints → Polish
   - Dependencies block parallel execution

## Validation Checklist
*GATE: Checked by main() before returning*

- [x] All contracts have corresponding tests
- [x] All entities have model tasks
- [x] All tests come before implementation
- [x] Parallel tasks truly independent
- [x] Each task specifies exact file path
- [x] No task modifies same file as another [P] task