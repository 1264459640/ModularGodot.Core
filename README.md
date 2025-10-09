# ModularGodot.Core

一个基于 Godot 4.4 的插件化游戏开发框架，采用模块化架构和现代 C# 设计模式。

## 🎯 项目概述

ModularGodot.Core 是一个为 Godot 游戏开发设计的企业级框架，以插件化架构为核心。框架通过模块化设计和自动依赖注入机制，让开发者能够轻松创建可扩展的功能插件，实现真正的即插即用。

项目提供了完整的基础设施支持，包括：

- **插件化架构**：以插件为核心的设计理念，支持动态扩展
- **自动依赖注入**：基于 Autofac 的 IoC 容器，简化服务注册和解析
- **事件驱动**：基于 R3 的响应式事件系统
- **事件总线**：线程安全、资源高效的事件总线实现
- **中介者模式**：解耦的命令和查询处理
- **资源管理**：智能缓存和内存监控
- **性能监控**：实时性能指标收集

## 🏗️ 插件化架构设计

### 插件架构核心优势

ModularGodot.Core 以插件化架构为核心，提供了强大的扩展能力：

1. **核心可扩展性**：通过插件机制实现功能的动态扩展
2. **松耦合设计**：插件间通过契约接口通信，降低依赖关系
3. **即插即用**：插件可以独立开发、测试和部署
4. **团队协作**：不同团队可以并行开发不同的插件模块
5. **自动集成**：通过依赖注入机制实现插件的自动发现和集成

### 插件结构

插件项目采用双包结构设计（参见[插件架构文档](docs/PLUGIN_ARCHITECTURE.md)）：

```
PluginName/
├── PluginName.Contracts/     # 共享契约包 - 接口、事件、命令定义
│   ├── Commands/            # 命令定义
│   ├── Events/              # 事件定义
│   ├── Interfaces/          # 接口定义
│   └── DTOs/                # 数据传输对象
└── PluginName/              # 本体包 - 具体实现
    ├── Services/            # 服务实现
    ├── Infrastructure/      # 基础设施实现
    └── Handlers/            # 命令和事件处理器
```

### 自动依赖注入机制

框架采用声明式依赖注入，通过自动化容器配置简化了服务管理（参见[依赖注入文档](docs/DEPENDENCY_INJECTION.md)）：

1. **契约自动注册**：插件只需定义接口和实现，框架自动处理依赖关系
2. **生命周期管理**：通过 `Lifetime` 枚举定义服务生命周期（Transient/Scoped/Singleton）
3. **处理器自动发现**：命令和查询处理器无需标记特性即可自动注入
4. **事件自动订阅**：通过构造函数注入实现事件处理器的自动订阅

```csharp
// 插件服务实现 - 自动注册
[Injectable(Lifetime.Scoped)]
public class UserService : IUserService
{
    public UserService(IUserRepository repository)
    {
        // 依赖自动注入
    }
}

// 命令处理器 - 自动发现，无需标记特性
public class CreateUserHandler : ICommandHandler<CreateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // 处理逻辑
    }
}
```

### 事件总线系统

框架提供了基于 R3 的增强型事件总线系统，支持线程安全操作和高效的资源管理：

1. **订阅管理模式**：通过返回订阅 ID 实现精确的订阅管理
2. **自动资源清理**：订阅资源自动管理，防止内存泄漏
3. **线程安全保障**：基于 ReaderWriterLockSlim 实现线程安全
4. **一次订阅功能**：支持处理单次事件并自动清理订阅
5. **性能监控集成**：内置发布/订阅计数器用于监控

```csharp
// 使用注入的事件总线
public class GameService
{
    private readonly IEventBus _eventBus;
    private string _subscriptionId;

    public GameService(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task SubscribeToEvents()
    {
        // 异步订阅，返回订阅ID用于取消订阅
        _subscriptionId = await _eventBus.Subscribe<GameStartedEvent>(async (e) =>
        {
            // 事件处理逻辑
            await HandleGameStart(e);
        });
    }

    public async Task SubscribeOnce()
    {
        // 一次性订阅，处理首次事件后自动清理
        await _eventBus.SubscribeOnce<PlayerWonEvent>(async (e) =>
        {
            // 独立处理逻辑
            await HandleWin(e);
        });
    }

    public async Task PublishEvent()
    {
        var gameStartEvent = new GameStartedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
            Source = "GameService"
        };

        // 发布事件
        await _eventBus.Publish(gameStartEvent);
    }

    public async Task Cleanup()
    {
        // 使用订阅ID进行取消订阅
        await _eventBus.Unsubscribe(_subscriptionId);
    }
}
```

## 🚀 快速开始

### 0. Godot Integration

#### Global Service Node

The project provides a `GodotGlobalService` node for easy access to core services in Godot environments:

1. Add the `GodotGlobalService` node to the root of your scene tree
2. Access core services through the singleton instance:

```csharp
// Get the global service instance
var globalService = GodotGlobalService.Instance;

// Resolve the dispatcher interface
var dispatcher = globalService.ResolveDispatcher();

// Resolve the event bus interface
var eventBus = globalService.ResolveEventBus();
```

For detailed usage examples, see: [ServiceUsageExample.cs](src/ModularGodot.Core/Examples/ServiceUsageExample.cs)

### 1. 环境要求

- .NET 9.0
- Godot 4.4.1
- **Windows**: Visual Studio 2022 或 JetBrains Rider
- **Linux**: VS Code 或 JetBrains Rider

### 2. 核心架构层

框架本身也是一个核心插件，包含以下基础层：

```
src/
├── ModularGodot.Core.Contracts/  # 核心契约层 - 基础接口和事件定义
│   ├── Abstractions/            # 核心抽象接口
│   ├── Attributes/              # 自定义特性
│   └── Events/                 # 核心事件定义
├── ModularGodot.Core.Contexts/   # 核心上下文层 - 依赖注入配置
├── ModularGodot.Core.Infrastructure/  # 核心基础设施层 - 基础服务实现
│   ├── Caching/                # 缓存服务
│   ├── Logging/                # 日志服务
│   ├── Messaging/              # 消息传递
│   ├── Monitoring/             # 性能监控
│   ├── ResourceLoading/        # 资源加载
│   └── ResourceManagement/     # 资源管理
└── ModularGodot.Core.Repositories/  # 核心仓储层 - 数据访问
```

### 3. NuGet 包结构

框架支持灵活的插件化使用方式：

#### 方式一：使用完整框架包（推荐）

作为核心插件使用，提供完整的基础设施：

```xml
<PackageReference Include="ModularGodot.Core" Version="1.0.0" />
```

完整框架包自动包含所有核心依赖：
- `ModularGodot.Core.Contracts` - 核心契约层
- `ModularGodot.Core.Contexts` - 核心上下文层
- `ModularGodot.Core.Infrastructure` - 核心基础设施层
- `ModularGodot.Core.Repositories` - 核心仓储层

#### 方式二：按需使用独立层包

对于需要更精细控制的场景，可以按需引用独立层包：

```xml
<!-- 只需要核心契约层 -->
<PackageReference Include="ModularGodot.Core.Contracts" Version="1.0.0" />

<!-- 需要核心上下文和契约层 -->
<PackageReference Include="ModularGodot.Core.Contexts" Version="1.0.0" />
<PackageReference Include="ModularGodot.Core.Contracts" Version="1.0.0" />
```

### 4. 构建和打包

项目提供了多种构建选项：

#### 构建独立包
```bash
# 构建所有独立包
dotnet pack src/ModularGodot.Core.sln -c Release -o packages

# 或者构建单个包
dotnet pack src/ModularGodot.Core.Contracts/ModularGodot.Core.Contracts.csproj -c Release -o packages
dotnet pack src/ModularGodot.Core.Contexts/ModularGodot.Core.Contexts.csproj -c Release -o packages
dotnet pack src/ModularGodot.Core.Infrastructure/ModularGodot.Core.Infrastructure.csproj -c Release -o packages
dotnet pack src/ModularGodot.Core.Repositories/ModularGodot.Core.Repositories.csproj -c Release -o packages
```

#### 构建完整框架包
```bash
dotnet pack src/ModularGodot.Core/ModularGodot.Core.csproj -c Release -o packages
```

#### 一键打包脚本

项目提供了 PowerShell 脚本以简化构建和打包过程：

```bash
# 使用增强型构建和打包脚本（推荐）
./tools/enhanced-build-pack.ps1 -Configuration Release

# 清理构建产物
./tools/cleanup.ps1
```

### 5. 集成测试

框架提供了完整的集成测试套件，用于验证组件间的协作和通信：

#### 测试项目结构
```
src/ModularGodot.Core.Test/
├── Models/                  # 测试数据模型
├── Scenes/                  # Godot测试场景
├── Services/                # 测试服务
├── Tests/                   # 单元测试
└── MainTestScene.tscn      # 主测试场景
```

#### 运行集成测试

1. **在Godot编辑器中运行**：
   - 打开Godot编辑器并加载`ModularGodot.Core.Test`项目
   - 选择要测试的场景文件（如`MediatorTestScene.tscn`）
   - 点击场景中的"RunTest"按钮执行测试

2. **批量运行所有测试**：
   - 打开`MainTestScene.tscn`
   - 点击"RunAllTests"按钮运行所有测试

3. **通过命令行构建和测试**：
   ```bash
   # 构建测试项目
   dotnet build src/ModularGodot.Core.Test/ModularGodot.Core.Test.csproj

   # 运行单元测试
   dotnet test src/ModularGodot.Core.Test/
   ```

#### 测试类别

- **中介者通信测试**：验证命令和查询处理
- **事件总线通信测试**：验证事件发布和订阅
- **包完整性测试**：验证NuGet包加载和功能
- **测试隔离测试**：验证测试独立执行能力

#### 性能要求

- 单个测试执行时间 < 100ms
- 中介者路由时间 < 1ms
- 事件总线发布时间 < 10ms

详细信息请参阅[集成测试文档](docs/INTEGRATION_TESTING.md)。

## 📚 详细文档

- [架构设计文档](docs/ARCHITECTURE.md) - 详细的架构说明和设计原则
- [NuGet 包文档](docs/NUGET_PACKAGES.md) - NuGet 包结构和使用说明
- [插件架构文档](docs/PLUGIN_ARCHITECTURE.md) - 插件开发和集成指南
- [依赖注入文档](docs/DEPENDENCY_INJECTION.md) - 依赖注入机制和使用说明
- [中介者模式使用指南](docs/MEDIATOR_USAGE.md) - 命令/查询中介模式使用说明
- [事件系统使用指南](docs/EVENT_SYSTEM_USAGE.md) - R3事件系统使用说明

## 🛠️ 技术栈

### 核心框架
- **Godot 4.4.1** - 游戏引擎
- **.NET 9.0** - 运行时平台

### 依赖注入
- **Autofac 8.3.0** - IoC 容器

### 消息传递
- **MediatR 13.0.0** - 中介者模式实现
- **R3 1.3.0** - 响应式编程库

### 缓存
- **Microsoft.Extensions.Caching.Memory 9.0.7** - 内存缓存

### 响应式编程
- **System.Reactive 6.0.0** - Rx.NET

## 🎨 设计原则

### 插件化设计原则
- **关注点分离**：核心框架与业务插件分离
- **契约驱动**：通过接口定义插件间契约
- **自动发现**：插件自动注册和注入
- **松耦合**：插件间通过事件和命令通信

### SOLID 原则
- **单一职责原则**: 每个插件专注于特定功能领域
- **开闭原则**: 对扩展开放，对修改关闭
- **里氏替换原则**: 插件实现可以替换接口定义
- **接口隔离原则**: 客户端不应依赖不需要的接口
- **依赖倒置原则**: 插件依赖抽象而非具体实现

## 🔧 开发指南

### 创建新插件

1. **定义契约** - 在插件的 Contracts 包中定义接口、事件和命令
2. **实现功能** - 在插件的本体包中提供具体实现
3. **配置注入** - 使用 `[Injectable]` 特性标记服务类
4. **编写测试** - 确保插件功能正确性

### 最佳实践

- 优先使用接口而非具体类
- 遵循异步编程模式
- 合理使用事件驱动架构
- 注意内存管理和性能优化
- 编写清晰的文档和注释

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！请确保：

1. 遵循现有的代码风格
2. 添加适当的测试
3. 更新相关文档
4. 提供清晰的提交信息

## 📞 支持

如有问题或建议，请：

1. 查看 [文档](docs/)
2. 搜索现有 [Issues](../../issues)
3. 创建新的 Issue

---

**Happy Coding! 🎮**