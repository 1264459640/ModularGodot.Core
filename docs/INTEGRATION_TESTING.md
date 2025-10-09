# ModularGodot.Core 集成测试指南

## 概述

本文档详细说明了ModularGodot.Core框架的集成测试策略、执行方法和最佳实践。集成测试用于验证框架各组件在真实环境中的协作和通信。

## 测试架构

### 测试项目结构
```
src/ModularGodot.Core.Test/
├── Models/                  # 测试数据模型
├── Scenes/                  # Godot测试场景
├── Services/                # 测试服务
├── Tests/                   # 单元测试
├── MainTestScene.tscn      # 主测试场景文件
├── MainTestScene.cs        # 主测试场景脚本
└── README.md              # 测试项目说明
```

### 测试类型

1. **场景测试**: 基于Godot场景的集成测试
2. **组件通信测试**: 验证中介者模式和事件总线通信
3. **包完整性测试**: 验证NuGet包的可用性和完整性
4. **测试隔离测试**: 验证测试执行的独立性和资源清理

## 测试场景说明

### 中介者通信测试 (MediatorTestScene)

验证中介者模式的命令和查询处理：

- **测试内容**: 通过`MiddlewareProvider`解析中介者服务
- **验证点**: 命令路由、查询处理、服务解析
- **预期结果**: 中介者服务成功解析并能处理测试命令

### 事件总线通信测试 (EventBusTestScene)

验证事件总线的发布和订阅功能：

- **测试内容**: 通过`MiddlewareProvider`解析事件总线服务
- **验证点**: 事件发布、订阅处理、资源管理
- **预期结果**: 事件总线服务成功解析并能处理测试事件

### 包完整性测试 (PackageTestScene)

验证所有必需NuGet包的存在和功能：

- **测试内容**: 检查ModularGodot.Core相关包的加载
- **验证点**: 程序集加载、类型解析、服务可用性
- **预期结果**: 所有核心包正确加载且功能正常

### 测试隔离测试 (TestIsolationScene)

验证测试执行的独立性和资源清理：

- **测试内容**: 测试状态管理和资源清理
- **验证点**: 测试重置、状态隔离、副作用预防
- **预期结果**: 测试可重复执行且无状态残留

## 执行测试

### Godot编辑器中运行

1. 打开Godot编辑器
2. 加载`ModularGodot.Core.Test`项目
3. 在场景面板中选择要测试的场景文件：
   - `MediatorTestScene.tscn`
   - `EventBusTestScene.tscn`
   - `PackageTestScene.tscn`
   - `TestIsolationScene.tscn`
4. 点击场景中的"RunTest"方法按钮执行测试

### 批量运行测试

使用主测试场景运行所有测试：

1. 在Godot编辑器中打开`MainTestScene.tscn`
2. 点击"RunAllTests"按钮
3. 查看测试结果输出

### 命令行构建

```bash
# 构建测试项目
dotnet build src/ModularGodot.Core.Test/ModularGodot.Core.Test.csproj

# 运行单元测试
dotnet test src/ModularGodot.Core.Test/
```

## 测试配置

### 环境设置

集成测试配置为仅在开发环境中运行：

```csharp
// TestConfiguration.cs
public bool ShouldRunTests()
{
    return IsDevelopmentEnvironment();
}

public bool IsDevelopmentEnvironment()
{
    var environment = GetValue<string>("Environment", "Production");
    return environment.Equals("Development", StringComparison.OrdinalIgnoreCase);
}
```

### 配置项

| 配置项 | 默认值 | 说明 |
|--------|--------|------|
| Environment | "Development" | 运行环境 |
| TestTimeoutMs | 5000 | 测试超时时间(毫秒) |
| EnableDetailedLogging | true | 启用详细日志 |
| MaxParallelTests | 1 | 最大并行测试数 |
| TestResultSavePath | "res://TestResults/" | 测试结果保存路径 |
| EnablePerformanceMonitoring | true | 启用性能监控 |

## 测试报告

### 报告生成

测试完成后会自动生成报告：

```csharp
var reporter = new TestReporter();
var report = reporter.GenerateReport(testResults);
var detailedReport = reporter.GenerateDetailedReport(testResults);
```

### 报告内容

1. **概览信息**: 总测试数、通过率、执行时间
2. **详细结果**: 每个测试的执行状态和消息
3. **性能分析**: 执行时间统计和性能问题识别
4. **环境信息**: 测试执行环境配置

## 性能要求

### 执行时间标准

- **单个测试执行时间**: < 100ms
- **中介者路由时间**: < 1ms
- **事件总线发布时间**: < 10ms
- **测试间内存累积**: 最小化

### 性能监控

```csharp
// TestReporter.cs
private List<PerformanceIssue> IdentifyPerformanceIssues(List<TestResult> testResults)
{
    var issues = new List<PerformanceIssue>();
    foreach (var result in testResults)
    {
        if (result.ExecutionTimeMs > 100)
        {
            issues.Add(new PerformanceIssue
            {
                SceneName = result.SceneName,
                ExecutionTimeMs = result.ExecutionTimeMs,
                IssueType = "执行时间过长",
                Description = $"测试执行时间({result.ExecutionTimeMs}ms)超过了100ms的预期上限"
            });
        }
    }
    return issues;
}
```

## 最佳实践

### 测试开发

1. **使用[Tool]属性**: 使测试场景可在编辑器中运行而不需完整运行
2. **遵循命名约定**: `FeatureNameTestScene`
3. **实现标准接口**: `RunTest()`, `GetTestResult()`, `ResetTest()`
4. **环境检查**: 在执行前检查是否为开发环境

### 场景设计

1. **独立性**: 每个测试场景应独立运行
2. **资源管理**: 正确清理订阅和资源
3. **状态重置**: 实现`ResetTest()`方法支持重复执行
4. **详细日志**: 记录关键执行步骤和结果

### 结果验证

1. **时间验证**: 确保执行时间符合性能要求
2. **状态检查**: 验证测试结果状态(Passed/Failed/Skipped)
3. **消息记录**: 提供清晰的测试结果描述
4. **错误处理**: 适当处理和报告异常情况

## 故障排除

### 常见问题

1. **测试未运行**: 检查环境配置是否为"Development"
2. **服务解析失败**: 验证MiddlewareProvider配置和依赖注入
3. **性能问题**: 检查执行时间是否超过100ms限制
4. **资源泄漏**: 确保所有订阅和资源正确清理

### 诊断工具

1. **详细日志**: 启用`EnableDetailedLogging`查看详细执行信息
2. **性能监控**: 通过报告识别性能瓶颈
3. **环境检查**: 验证配置项是否正确设置

## 维护指南

### 添加新测试

1. 创建新的测试场景类继承自`Node`
2. 实现`RunTest()`, `GetTestResult()`, `ResetTest()`方法
3. 创建对应的`.tscn`场景文件
4. 在`MainTestScene`中注册新测试

### 更新现有测试

1. 根据功能变更调整测试逻辑
2. 更新测试描述和预期结果
3. 验证性能要求仍然满足
4. 更新相关文档说明

## 相关文档

- [快速开始指南](../specs/003-mnt-d-godotprojects/quickstart.md)
- [架构设计文档](docs/ARCHITECTURE.md)
- [中介者模式使用指南](docs/MEDIATOR_USAGE.md)
- [事件系统使用指南](docs/EVENT_SYSTEM_USAGE.md)