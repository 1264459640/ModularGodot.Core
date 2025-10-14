# ModularGodot.Core 集成测试项目

这个项目包含ModularGodot.Core框架的集成测试，用于验证各个组件在真实环境中的协作。

## 测试项目结构

```
src/ModularGodot.Core.Test/
├── IntegrationTests/                 # 集成测试
│   ├── FrameworkIntegrationTests.cs   # 框架级集成测试
│   ├── RealtimeEventIntegrationTests.cs # 实时事件集成测试
│   └── MediatorPatternIntegrationTests.cs # Mediator模式集成测试
├── TestHelpers/                      # 测试辅助工具
└── README.md                        # 本文档
```

## 集成测试 vs 单元测试的区别

### 单元测试特点
- **隔离性**: 测试单个类或方法，使用Mock对象隔离依赖
- **速度**: 执行快速，适合频繁运行
- **目的**: 验证业务逻辑正确性
- **依赖**: 使用TestContext模拟的DI容器

### 集成测试特点
- **协作性**: 测试多个组件的真实协作
- **真实性**: 使用真实的依赖注入容器和服务
- **速度**: 相对较慢，需要完整的系统初始化
- **目的**: 验证组件集成的整体功能

## 集成测试编写指南

### 基本原则
1. **使用真实容器**: 集成测试应该使用`Contexts.Contexts.Instance`而不是`TestContext`
2. **测试流程**: 关注端到端的业务流程
3. **异步处理**: 适当等待事件传播和处理完成
4. **资源清理**: 确保订阅和资源的正确清理

### 测试类结构
```csharp
public class MyIntegrationTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly IService _realService; // 真实服务

    public MyIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        var context = Contexts.Contexts.Instance; // 使用真实容器
        _realService = context.ResolveService<IService>();
    }

    [Fact]
    public async Task EndToEndWorkflow_ShouldWork()
    {
        // Arrange - 使用真实依赖

        // Act - 执行完整的业务流程

        // Assert - 验证最终结果
    }

    public void Dispose()
    {
        // 清理资源
    }
}
```

### 关键测试场景

#### 1. 事件系统集成测试
- 验证事件的发布和订阅
- 测试条件订阅和一次性订阅
- 验证事件的自动清理机制

#### 2. Mediator模式集成测试
- 测试命令/查询的完整处理流程
- 验证异常传播和取消机制
- 测试并发和并行处理

#### 3. 资源管理系统集成测试
- 验证内存监控和性能报告
- 测试缓存策略和资源清理
- 验证资源加载事件的触发

## 运行测试

### 运行所有集成测试
```bash
dotnet test src/ModularGodot.Core.Test/ModularGodot.Core.Test.csproj
```

### 运行特定类别的测试
```bash
# 只运行框架集成测试
dotnet test --filter "FullyQualifiedName~FrameworkIntegrationTests"

# 只运行事件集成测试
dotnet test --filter "FullyQualifiedName~RealtimeEventIntegrationTests"

# 只运行Mediator集成测试
dotnet test --filter "FullyQualifiedName~MediatorPatternIntegrationTests"
```

### 查看详细输出
```bash
dotnet test --logger "console;verbosity=normal"
```

## 最佳实践

1. **命名约定**: 使用`FeatureName_Scenario_ExpectedResult`格式
2. **测试数据**: 对于复杂的测试数据，考虑使用测试数据构建器
3. **时间等待**: 适当使用`Task.Delay()`等待异步操作完成
4. **异常测试**: 验证异常的正确传播和错误信息
5. **并行测试**: 测试并发场景下的行为和性能

## 常见陷阱

1. **过度集成**: 不要在集成测试中测试所有的边界条件
2. **不稳定测试**: 避免依赖时间或外部环境的不稳定测试
3. **资源泄漏**: 确保所有事件订阅和资源都被正确清理
4. **长运行时间**: 保持集成测试在合理的时间范围内运行

## 维护建议

- 定期审查集成测试的覆盖率
- 更新测试用例以匹配新的业务需求
- 清理过时或重复的测试用例
- 监控测试运行时间，必要时优化