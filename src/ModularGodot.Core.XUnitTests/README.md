# ModularGodot.Core 测试框架

本项目使用 xUnit 测试框架提供全面的自动化测试支持。

## 🧪 测试项目结构

```
src/ModularGodot.Core.XUnitTests/
├── ConfigurationTests.cs          # 运行时配置测试
├── TestBase.cs                    # 测试基类
├── DependencyInjection/           # 依赖注入相关测试
│   ├── AdvancedContainerTests.cs  # 高级容器功能测试
│   ├── ContainerTests.cs          # 容器基础功能测试
│   ├── DependencyInjectionTests.cs # 依赖注入测试
│   ├── DITests.cs                 # DI功能测试
│   ├── LifetimeTests.cs           # 生命周期测试
│   ├── OriginalIntegrationTests.cs # 迁移的集成测试
│   └── ServiceFunctionalityTests.cs # 服务功能测试
└── README.md                      # 本文档
```

## 🚀 运行测试

### 使用脚本运行（推荐）

```bash
# 运行所有测试
./run-tests.sh
```

### 使用 dotnet test 命令

```bash
# 运行所有测试
dotnet test src/ModularGodot.Core.XUnitTests/ModularGodot.Core.XUnitTests.csproj

# 运行特定测试类
dotnet test --filter "FullyQualifiedName~ContainerTests"

# 运行特定测试方法
dotnet test --filter "Name=ResolveService_ShouldReturnValidServiceInstance"
```

### 定期清理构建（解决依赖问题）

在运行测试前，如果遇到依赖解析问题，建议使用以下命令清理并重新构建：

```bash
# 清理构建
dotnet clean src/ModularGodot.Core.XUnitTests/ModularGodot.Core.XUnitTests.csproj

# 重新构建
dotnet build src/ModularGodot.Core.XUnitTests/ModularGodot.Core.XUnitTests.csproj
```

## 📊 测试覆盖范围

### 依赖注入测试
- 服务解析验证
- 生命周期管理（Transient、Singleton）
- 依赖注入正确性验证

### 容器功能测试
- 服务注册状态检查
- 服务解析成功/失败情况
- 异常处理验证

### 功能测试
- 同步/异步方法功能验证
- 服务行为正确性检查

### 中介者模式测试
- 命令发送和处理验证
- 查询发送和处理验证
- 取消令牌处理验证

## 🛠️ 测试框架特性

### 丰富的断言支持
```csharp
Assert.Equal(expected, actual);
Assert.NotNull(object);
Assert.Same(instance1, instance2);
Assert.Throws<Exception>(() => method());
```

### 测试生命周期管理
```csharp
public class MyTests : TestBase
{
    public MyTests()
    {
        // 测试初始化
    }

    [Fact]
    public void TestMethod()
    {
        // 测试执行
    }

    public void Dispose()
    {
        // 测试清理
        base.Dispose();
    }
}
```

### 数据驱动测试
```csharp
[Theory]
[InlineData(2, 2, 4)]
[InlineData(1, 1, 2)]
public void AdditionTest(int a, int b, int expected)
{
    Assert.Equal(expected, a + b);
}
```

## 📈 测试最佳实践

### 1. 测试命名规范
- 使用 `MethodName_StateUnderTest_ExpectedBehavior` 格式
- 测试方法名应清晰描述测试目的

### 2. AAA 模式
```csharp
[Fact]
public void TestExample()
{
    // Arrange - 准备测试数据
    var service = new MyService();

    // Act - 执行被测试的方法
    var result = service.DoSomething();

    // Assert - 验证结果
    Assert.NotNull(result);
}
```

### 3. 测试隔离
- 每个测试应独立运行
- 避免测试间的依赖关系
- 使用 TestBase 提供的容器访问

## 🔄 CI/CD 集成

测试项目与持续集成系统无缝集成：

```yaml
# GitHub Actions 示例
- name: 运行测试
  run: dotnet test src/ModularGodot.Core.XUnitTests/ModularGodot.Core.XUnitTests.csproj
```

## 📊 代码覆盖率

生成代码覆盖率报告：

```bash
# 生成覆盖率数据
dotnet test --collect:"XPlat Code Coverage"

# 生成详细报告（需要 coverlet 工具）
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 🤝 贡献测试

添加新测试时请遵循：

1. 在相应的测试类别目录中创建测试类
2. 继承 TestBase 基类以获得容器访问能力
3. 使用清晰的测试命名
4. 遵循 AAA 测试模式
5. 确保测试的独立性和可重复性

## 📞 支持

如有测试相关问题，请：

1. 查看现有测试示例
2. 参考 xUnit 官方文档
3. 创建 Issue 讨论测试策略