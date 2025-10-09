# Tasks: 生产基础设施集成测试

**输入**: 来自 `/specs/003-mnt-d-godotprojects/` 的设计文档
**先决条件**: plan.md (必需), research.md, data-model.md

## 执行流程 (主要)
```
1. 加载特性目录中的 plan.md
   → 如果未找到：错误 "No implementation plan found"
   → 提取：技术栈、库、结构
2. 加载可选设计文档：
   → data-model.md：提取实体 → 模型任务
   → research.md：提取决策 → 设置任务
3. 按类别生成任务：
   → 设置：项目初始化、依赖项、代码风格
   → 测试：集成测试
   → 核心：模型、服务、CLI 命令
   → 集成：数据库、中间件、日志记录
   → 完善：单元测试、性能、文档
4. 应用任务规则：
   → 不同文件 = 标记 [P] 以并行执行
   → 同一文件 = 顺序执行 (无 [P])
5. 按顺序编号任务 (T001, T002...)
6. 生成依赖关系图
7. 创建并行执行示例
8. 验证任务完整性：
   → 所有实体都有模型？
   → 所有端点都已实现？
9. 返回：SUCCESS (任务可执行)
```

## 格式: `[ID] [P?] 描述`
- **[P]**: 可以并行执行 (不同文件，无依赖关系)
- 在描述中包含确切的文件路径

## 路径约定
- **单一项目**: `src/`, `tests/` 位于仓库根目录
- **Godot 项目**: `src/ModularGodot.Core.Test/`
- 下面显示的路径假定为 Godot 项目 - 根据 plan.md 结构进行调整

## Phase 3.1: 设置
- [x] T001 创建项目结构，按照实现计划在 src/ModularGodot.Core.Test/ 中
- [x] T002 [P] 初始化 Godot 项目，并配置 Godot.NET.Sdk/4.5.0 依赖项
- [x] T003 [P] 配置代码风格和格式化工具 (如 .editorconfig)

## Phase 3.2: 集成测试场景实现
- [x] T004 [P] TestScene 模型在 src/ModularGodot.Core.Test/Models/TestScene.cs
- [x] T005 [P] TestResult 模型在 src/ModularGodot.Core.Test/Models/TestResult.cs
- [x] T006 创建中介者测试场景文件 src/ModularGodot.Core.Test/Scenes/MediatorTestScene.tscn
- [x] T007 [P] 中介者测试场景实现 (MediatorTestScene.cs)
- [x] T008 创建事件总线测试场景文件 src/ModularGodot.Core.Test/Scenes/EventBusTestScene.tscn
- [x] T009 [P] 事件总线测试场景实现 (EventBusTestScene.cs)
- [x] T010 创建包完整性测试场景文件 src/ModularGodot.Core.Test/Scenes/PackageTestScene.tscn
- [x] T011 [P] 包完整性测试场景实现 (PackageTestScene.cs)
- [x] T012 创建测试隔离场景文件 src/ModularGodot.Core.Test/Scenes/TestIsolationScene.tscn
- [x] T013 [P] 测试隔离场景实现 (TestIsolationScene.cs)

## Phase 3.3: 测试核心功能实现
- [x] T014 实现测试运行方法和验证逻辑
- [x] T015 实现测试配置管理
- [x] T016 实现测试结果报告

## Phase 3.4: 测试集成
- [x] T017 连接测试场景到 Godot 场景树和 MiddlewareProvider
- [x] T018 实现开发环境检查和测试条件执行
- [x] T019 实现场景加载和组件初始化逻辑
- [x] T020 添加测试日志记录和诊断信息

## Phase 3.5: 测试完善
- [ ] T021 [P] 为测试配置管理编写单元测试
- [ ] T022 [P] 为测试运行逻辑编写单元测试
- [ ] T023 性能测试 (<100ms 测试执行时间)
- [ ] T024 实现集成测试的错误报告功能
- [ ] T025 验证仅成功场景测试（无错误处理验证）
- [ ] T026 [P] 更新 docs/INTEGRATION_TESTING.md
- [ ] T027 [P] 更新 README.md 中的测试部分
- [ ] T028 移除重复代码
- [ ] T029 运行 quickstart.md 中的手动测试步骤

## 依赖关系
- 设置任务 (T001-T003) 在测试实现 (T004-T020) 之前
- 测试模型 (T004-T005) 在测试场景实现 (T006-T013) 之前
- 场景文件创建 (T006, T008, T010, T012) 在场景实现 (T007, T009, T011, T013) 之前
- 测试场景实现 (T006-T013) 在核心功能实现 (T014-T016) 之前
- 核心功能实现 (T014-T016) 在测试集成 (T017-T020) 之前
- 测试集成 (T017-T020) 在测试完善 (T021-T029) 之前

## Godot 场景测试说明
由于 Godot 引擎的特性，场景测试有以下特点：
1. **场景文件**: 每个测试场景需要对应的 .tscn 文件
2. **手动执行**: 场景测试需要在 Godot 编辑器中手动逐个执行
3. **[Tool] 属性**: 使用 [Tool] 属性可以在不运行场景的情况下进行测试，这是推荐的做法
4. **并行限制**: 由于场景文件的特性，不能并行执行多个场景测试
5. **编辑器测试**: 测试需要在 Godot 编辑器中启动，通过选择需要测试的场景进行测试

## 并行示例
```
# 同时启动 T004-T005 和 T006-T013（模型和场景文件创建可以并行）：
Task: "TestScene 模型在 src/ModularGodot.Core.Test/Models/TestScene.cs"
Task: "TestResult 模型在 src/ModularGodot.Core.Test/Models/TestResult.cs"
Task: "创建中介者测试场景文件 src/ModularGodot.Core.Test/Scenes/MediatorTestScene.tscn"
Task: "中介者测试场景实现 (MediatorTestScene.cs)"
Task: "创建事件总线测试场景文件 src/ModularGodot.Core.Test/Scenes/EventBusTestScene.tscn"
Task: "事件总线测试场景实现 (EventBusTestScene.cs)"
Task: "创建包完整性测试场景文件 src/ModularGodot.Core.Test/Scenes/PackageTestScene.tscn"
Task: "包完整性测试场景实现 (PackageTestScene.cs)"
Task: "创建测试隔离场景文件 src/ModularGodot.Core.Test/Scenes/TestIsolationScene.tscn"
Task: "测试隔离场景实现 (TestIsolationScene.cs)"

# 同时启动 T021-T022 和 T024-T025（完善任务可以并行）：
Task: "为测试配置管理编写单元测试"
Task: "为测试运行逻辑编写单元测试"
Task: "更新 docs/INTEGRATION_TESTING.md"
Task: "更新 README.md 中的测试部分"
```

## 任务生成规则
1. **来自数据模型**：
   - 每个实体 → 模型创建任务 [P]
   - 关系 → 服务层任务

2. **来自用户故事**：
   - 每个故事 → 集成测试 [P]
   - 快速入门场景 → 验证任务

3. **排序**：
   - 设置 → 模型 → 场景文件 → 场景实现 → 核心功能 → 集成 → 完善
   - 依赖关系阻止并行执行

## 验证清单
- [x] 所有实体都有模型任务 (T004-T005)
- [x] 所有测试场景都有对应的 .tscn 文件 (T006, T008, T010, T012)
- [x] 并行任务真正独立 (不同文件)
- [x] 每个任务都指定了确切的文件路径
- [x] 没有任务修改与另一个 [P] 任务相同的文件
- [x] 所有功能需求都有对应的任务覆盖