# Tasks: Event Bus System Fix

**Input**: Design documents from `/specs/002-mnt-d-godotprojects/`
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
- [X] T001 Create thread-safe concurrent event management structures in src/ModularGodot.Core.Infrastructure/Messaging/R3EventBus.cs
- [X] T002 Update memory leak prevention mechanisms for subscriptions in src/ModularGodot.Core.Infrastructure/Messaging/R3EventBus.cs
- [X] T003 [P] Set up proper disposal lifecycle for R3 subjects and subscriptions in src/ModularGodot.Core.Infrastructure/Messaging/R3EventBus.cs

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3
**CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation**
- [X] T004 [P] Contract test Subscribe<T> functionality in tests/ModularGodot.Core.XUnitTests/Events/EventBusSubscribeContractTest.cs
- [X] T005 [P] Contract test SubscribeOnce<T> functionality in tests/ModularGodot.Core.XUnitTests/Events/EventBusSubscribeOnceContractTest.cs
- [X] T006 [P] Contract test Publish<T> functionality in tests/ModularGodot.Core.XUnitTests/Events/EventBusPublishContractTest.cs
- [X] T007 [P] Contract test Unsubscribe functionality in tests/ModularGodot.Core.XUnitTests/Events/EventBusUnsubscribeContractTest.cs
- [X] T008 [P] Contract test event validation functionality in tests/ModularGodot.Core.XUnitTests/Events/EventBusValidationContractTest.cs
- [X] T009 [P] Integration test memory leak prevention in tests/ModularGodot.Core.XUnitTests/Events/MemoryLeakPreventionTest.cs
- [X] T010 [P] Integration test thread safety under high concurrent load in tests/ModularGodot.Core.XUnitTests/Events/ThreadSafetyConcurrentTest.cs
- [X] T011 [P] Integration test one-time subscription auto-disposal in tests/ModularGodot.Core.XUnitTests/Events/OneTimeSubscriptionTest.cs
- [X] T012 [P] Integration test resource cleanup after disposal in tests/ModularGodot.Core.XUnitTests/Events/ResourceCleanupTest.cs

## Phase 3.3: Core Implementation (ONLY after tests are failing)
- [X] T013 [P] Implement enhanced EventSubscription model in src/ModularGodot.Core.Infrastructure/Messaging/EventSubscription.cs
- [X] T014 [P] Implement topic subject management with fine-grained locks in src/ModularGodot.Core.Infrastructure/Messaging/TopicSubject.cs
- [X] T015 [P] Implement Subscribe<T> method with thread-safe resource management in src/ModularGodot.Core.Infrastructure/Messaging/R3EventBus.cs
- [X] T016 [P] Implement SubscribeOnce<T> method with automatic disposal in src/ModularGodot.Core.Infrastructure/Messaging/R3EventBus.cs
- [X] T017 [P] Implement Publish<T> method with validation and thread safety in src/ModularGodot.Core.Infrastructure/Messaging/R3EventBus.cs
- [X] T018 [P] Implement Unsubscribe method with resource cleanup in src/ModularGodot.Core.Infrastructure/Messaging/R3EventBus.cs
- [X] T019 [P] Implement event format validation logic in src/ModularGodot.Core.Infrastructure/Messaging/R3EventBus.cs
- [X] T020 [P] Implement composite disposal and resource tracking in src/ModularGodot.Core.Infrastructure/Messaging/R3EventBus.cs
- [X] T021 [P] Implement ReaderWriterLockSlim protection for subscription management in src/ModularGodot.Core.Infrastructure/Messaging/R3EventBus.cs
- [X] T022 [P] Implement stopping/disposed state management for thread safety in src/ModularGodot.Core.Infrastructure/Messaging/R3EventBus.cs

## Phase 3.4: Integration
- [X] T023 Update DI module registration for enhanced event bus in src/ModularGodot.Core.Contexts/MediatorModule.cs
- [X] T024 Connect event bus with logging for observability in src/ModularGodot.Core.Infrastructure/Messaging/R3EventBus.cs
- [X] T025 Update event bus performance metrics collection in src/ModularGodot.Core.Infrastructure/Messaging/R3EventBus.cs
- [X] T026 Ensure proper cleanup of disposed event bus in tests/main test harness

## Phase 3.5: Polish
- [X] T027 [P] Unit tests for subscription resource management in tests/ModularGodot.Core.XUnitTests/Events/SubscriptionResourceTest.cs
- [X] T028 [P] Stress tests exceeding 1,000 events/second with memory monitoring in tests/ModularGodot.Core.XUnitTests/Events/StressTest.cs
- [X] T029 Performance tests ensuring memory usage < 100MB continuous operation
- [X] T030 [P] Update docs/README.md with new event bus usage patterns
- [X] T031 [P] Update docs/ARCHITECTURE.md with resource management details
- [X] T032 Run quickstart validation per specs/002-mnt-d-godotprojects/quickstart.md

## Dependencies
- Tests (T004-T012) before implementation (T013-T022)
- T013 blocks T015, T016, T17, T018, T020, T021, T022
- T015, T016, T017, T018 blocks T023, T024, T025, T020, T021, T022
- Implementation before polish (T027-T032)

## Parallel Example
```
# Launch T004-T012 together:
Task: "Contract test Subscribe<T> functionality in tests/ModularGodot.Core.XUnitTests/Events/EventBusSubscribeContractTest.cs"
Task: "Contract test SubscribeOnce<T> functionality in tests/ModularGodot.Core.XUnitTests/Events/EventBusSubscribeOnceContractTest.cs"
Task: "Contract test Publish<T> functionality in tests/ModularGodot.Core.XUnitTests/Events/EventBusPublishContractTest.cs"
Task: "Contract test Unsubscribe functionality in tests/ModularGodot.Core.XUnitTests/Events/EventBusUnsubscribeContractTest.cs"
Task: "Contract test event validation functionality in tests/ModularGodot.Core.XUnitTests/Events/EventBusValidationContractTest.cs"
Task: "Integration test memory leak prevention in tests/ModularGodot.Core.XUnitTests/Events/MemoryLeakPreventionTest.cs"
Task: "Integration test thread safety under high concurrent load in tests/ModularGodot.Core.XUnitTests/Events/ThreadSafetyConcurrentTest.cs"
Task: "Integration test one-time subscription auto-disposal in tests/ModularGodot.Core.XUnitTests/Events/OneTimeSubscriptionTest.cs"
Task: "Integration test resource cleanup after disposal in tests/ModularGodot.Core.XUnitTests/Events/ResourceCleanupTest.cs"
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

- [ ] All contracts have corresponding tests
- [ ] All entities have model tasks
- [ ] All tests come before implementation
- [ ] Parallel tasks truly independent
- [ ] Each task specifies exact file path
- [ ] No task modifies same file as another [P] task