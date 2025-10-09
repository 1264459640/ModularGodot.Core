# 实现计划：生产基础设施集成测试

**分支**: `003-mnt-d-godotprojects` | **日期**: 2025-10-08 | **规范**: /mnt/d/GodotProjects/ModularGodot.Core/specs/003-mnt-d-godotprojects/spec.md
**输入**: 来自 `/specs/003-mnt-d-godotprojects/spec.md` 的功能规范

## 执行流程 (/plan 命令范围)
```
1. 从输入路径加载功能规范
   → 如果未找到：错误 "No feature spec at {path}"
2. 填充技术上下文 (扫描 NEEDS CLARIFICATION)
   → 从文件系统结构或上下文中检测项目类型 (web=前端+后端, mobile=应用+API)
   → 基于项目类型设置结构决策
3. 基于宪法文档内容填充宪法检查部分
4. 评估下面的宪法检查部分
   → 如果存在违规：记录在复杂性跟踪中
   → 如果无法证明合理性：错误 "Simplify approach first"
   → 更新进度跟踪：初始宪法检查
5. 执行阶段 0 → research.md
   → 如果仍有 NEEDS CLARIFICATION：错误 "Resolve unknowns"
6. 执行阶段 1 → contracts, data-model.md, quickstart.md, 代理特定模板文件 (例如，Claude Code 的 `CLAUDE.md`，GitHub Copilot 的 `.github/copilot-instructions.md`，Gemini CLI 的 `GEMINI.md`，Qwen Code 的 `QWEN.md`，或其他所有代理的 `AGENTS.md`)。
7. 重新评估宪法检查部分
   → 如果有新违规：重构设计，返回阶段 1
   → 更新进度跟踪：设计后宪法检查
8. 计划阶段 2 → 描述任务生成方法 (不要创建 tasks.md)
9. 停止 - 准备执行 /tasks 命令
```

**重要**: /plan 命令在第 7 步停止。阶段 2-4 由其他命令执行：
- 阶段 2: /tasks 命令创建 tasks.md
- 阶段 3-4: 实现执行 (手动或通过工具)

## 概要
主要需求：在 ModularGodot.Core.Test 目录中创建集成测试，这些测试能够运行完整的生产基础设施，以验证组件通信 (Mediator 和 EventBus) 和包完整性 (所有 NuGet 包) 是否没有错误。测试应仅验证成功场景，并在使用 Godot 场景的开发环境中运行。

## 技术上下文
**语言/版本**: C# (.NET 9.0 或更高版本)
**主要依赖**: Godot.NET.Sdk/4.5.0, Autofac, MediatR, R3
**存储**: 不适用
**测试**: 基于 Godot 场景的集成测试
**目标平台**: 带有 .NET 的 Godot 引擎
**项目类型**: 单一
**性能目标**: 每个测试的执行时间 <100ms
**约束**: 测试必须仅在开发环境中运行，仅验证成功场景，必须使用 Godot 场景进行测试
**规模/范围**: ModularGodot.Core 框架中 5 个 NuGet 包的集成测试

## 宪法检查
*门禁：必须在阶段 0 研究之前通过。在阶段 1 设计后重新检查。*

### 与核心原则的一致性

**I. 插件优先架构**:
- 集成测试必须验证 ModularGodot.Core 的基于插件的架构
- 测试将验证所有组件是否遵循契约/实现模式
- 每个 NuGet 包 (Contracts, Contexts, Infrastructure, Repositories, Core) 必须作为可插拔组件进行测试

**II. 自动化依赖管理**:
- 测试必须验证服务是否通过 `[Injectable]` 属性系统正确注册
- 必须验证通过 Autofac IoC 容器进行的依赖解析
- 必须测试构造函数注入模式

**III. 契约驱动开发**:
- 所有集成测试必须使用接口契约进行组件通信
- 命令和查询必须通过 ICommand/IQuery 接口进行测试
- 事件必须通过事件总线使用类型化事件契约进行测试

**IV. 事件驱动架构**:
- 组件间的通信必须通过 R3 事件系统进行测试
- 必须验证发布者-订阅者解耦
- 必须验证事件的不可变性和语义含义

**V. 用于命令/查询分离的中介者模式**:
- 命令和查询必须通过 IDispatcher 接口通过中介者模式进行测试
- 必须验证业务逻辑与命令/查询处理的分离
- 必须验证单一职责和一致响应

### 架构标准合规性
- 测试将验证每个组件的双包结构
- 将验证依赖顺序 (Repository → Infrastructure → Contexts → Contracts)
- 将测量适用情况下的 <1ms 命令路由性能
- 将包含全面的单元测试

### 开发工作流合规性
- 测试将遵循 .NET 标准命名约定
- 将使用清晰、描述性的测试名称
- 所有测试必须通过自动化.peer review 要求

## 项目结构

### 文档 (此功能)
```
specs/003-mnt-d-godotprojects/
├── plan.md              # 此文件 (/plan 命令输出)
├── research.md          # 阶段 0 输出 (/plan 命令)
├── data-model.md        # 阶段 1 输出 (/plan 命令)
├── quickstart.md        # 阶段 1 输出 (/plan 命令)
└── tasks.md             # 阶段 2 输出 (/tasks 命令 - /plan 不创建)
```

### 源代码 (仓库根目录)
```
src/
├── ModularGodot.Core/
│   ├── ModularGodot.Core.csproj
├── ModularGodot.Core.Contracts/
│   ├── ModularGodot.Core.Contracts.csproj
├── ModularGodot.Core.Contexts/
│   ├── ModularGodot.Core.Contexts.csproj
├── ModularGodot.Core.Infrastructure/
│   ├── ModularGodot.Core.Infrastructure.csproj
├── ModularGodot.Core.Repositories/
│   ├── ModularGodot.Core.Repositories.csproj
├── ModularGodot.Core.Test/
│   ├── ModularGodot.Core.Test.csproj
└── ModularGodot.Core.XUnitTests/
    ├── ModularGodot.Core.XUnitTests.csproj

tests/
├── contract/
├── integration/
└── unit/
```

**结构决策**: 遵循 ModularGodot.Core 框架架构的单一项目结构，包含多个 NuGet 包。集成测试将在新的 ModularGodot.Core.Test 项目中实现。

## 阶段 0: 大纲与研究
1. **从上面的技术上下文中提取未知项**:
   - 所有技术上下文项已通过 /clarify 流程澄清
   - 没有剩余的 NEEDS CLARIFICATION 标记
   - 依赖项已明确定义：Autofac, MediatR, R3
   - 集成模式已建立：中介者模式，事件驱动架构

2. **研究任务**:
   - 任务: "研究在 .NET 中进行集成测试的最佳实践"
   - 任务: "研究用于集成测试的 Autofac 依赖注入模式"
   - 任务: "研究 MediatR 中介者模式测试方法"
   - 任务: "研究 R3 事件系统测试策略"

3. **在 `research.md` 中整合发现** 使用格式:
   - 决策: 使用 ModularGodot.Core.Test 项目结构进行集成测试
   - 理由: 利用现有的测试基础设施并遵循项目约定
   - 考虑的替代方案: 创建单独的测试项目

**输出**: 包含集成测试实现方法的 research.md

## 阶段 1: 设计与契约
*先决条件: research.md 完成*

1. **从功能规范中提取实体** → `data-model.md`:
   - IntegrationTestSuite: 验证系统功能的测试集合
   - ComponentCommunicationTest: 验证 Mediator 和 EventBus 之间通信的测试
   - PackageCompletenessTest: 验证所有必需包是否存在且功能正常的测试
   - TestConfiguration: 集成测试的配置设置
   - TestResult: 集成测试执行的结果

2. **从功能需求生成测试契约**:
   - Mediator 组件通信测试的契约
   - EventBus 组件通信测试的契约
   - 包完整性验证的契约
   - 测试隔离和副作用预防的契约

3. **从测试需求生成测试用例**:
   - MediatorTestScene.cs: 测试 Mediator 通信功能
   - EventBusTestScene.cs: 测试 EventBus 通信功能
   - PackageTestScene.cs: 测试包完整性功能
   - TestIsolationScene.cs: 测试测试隔离功能

4. **从用户故事中提取测试场景**:
   - 场景 1: 在 Godot 场景中运行所有基础设施组件的集成测试
   - 场景 2: 验证 Mediator 和 EventBus 之间的组件通信
   - 场景 3: 验证所有 NuGet 包的包完整性
   - 场景 4: 仅在开发环境中成功执行测试

5. **增量更新代理文件** (O(1) 操作):
   - 运行 `.specify/scripts/bash/update-agent-context.sh claude`
     **重要**: 精确按照上述方式执行。不要添加或删除任何参数。
   - 为 ModularGodot.Core 框架添加集成测试模式
   - 保留标记之间的手动添加内容
   - 更新最近的更改 (保留最后 3 个)
   - 为令牌效率保持在 150 行以下
   - 输出到仓库根目录

**输出**: data-model.md, quickstart.md, CLAUDE.md

## 阶段 2: 任务规划方法
*本节描述 /tasks 命令将执行的操作 - /plan 期间不要执行*

**任务生成策略**:
- 加载 `.specify/templates/tasks-template.md` 作为基础
- 从阶段 1 设计文档 (数据模型, 快速入门) 生成任务
- 每个测试需求 → 集成测试任务 [P]
- 每个实体 → 测试实体创建任务 [P]
- 每个用户故事 → 集成测试实现任务
- 使测试通过的实现任务
- 包完整性验证任务
- 集成测试环境的配置和设置任务

**排序策略**:
- 测试优先顺序: 测试在实现之前
- 依赖顺序: 测试模型在测试实现之前
- 标记 [P] 以并行执行 (独立的测试文件)
- 分组相关任务 (Mediator 测试, EventBus 测试, 包测试)

**预计输出**: tasks.md 中 15-20 个编号的有序任务，涵盖:
- 集成测试实现 (4 个任务)
- 包完整性验证 (3 个任务)
- 测试配置和设置 (3 个任务)
- 测试文档和快速入门 (2 个任务)
- 验证和确认任务 (3 个任务)

**重要**: 此阶段由 /tasks 命令执行，而不是 /plan

## 阶段 3+: 未来实现
*这些阶段超出了 /plan 命令的范围*

**阶段 3**: 任务执行 (/tasks 命令创建 tasks.md)
**阶段 4**: 实现 (执行.tasks.md 遵循宪法原则)
**阶段 5**: 验证 (运行测试, 执行.quickstart.md, 性能验证)

## 复杂性跟踪
*仅当宪法检查有必须证明合理性的违规时才填写*

| 违规 | 为什么需要 | 拒绝的更简单替代方案 |
|-----------|------------|-------------------------------------|
| [例如，第 4 个项目] | [当前需求] | [为什么 3 个项目不够] |
| [例如，存储库模式] | [特定问题] | [为什么直接数据库访问不够] |

## 进度跟踪
*此检查列表在执行流程中更新*

**阶段状态**:
- [x] 阶段 0: 研究完成 (/plan 命令)
- [x] 阶段 1: 设计完成 (/plan 命令)
- [x] 阶段 2: 任务规划完成 (/plan 命令 - 仅描述方法)
- [ ] 阶段 3: 任务生成 (/tasks 命令)
- [ ] 阶段 4: 实现完成
- [ ] 阶段 5: 验证通过

**门禁状态**:
- [x] 初始宪法检查: 通过
- [x] 设计后宪法检查: 通过
- [x] 所有 NEEDS CLARIFICATION 已解决
- [x] 复杂性偏差已记录

---
*基于宪法 v2.1.1 - 参见 `/memory/constitution.md`*