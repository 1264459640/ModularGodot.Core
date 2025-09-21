# ModularGodot.Core

一个基于 Godot 4.4 的模块化游戏开发框架，采用分层架构和现代 C# 设计模式。

## 🎯 项目概述

ModularGodot.Core 是一个为 Godot 游戏开发设计的企业级框架，提供了完整的基础设施支持，包括：

- **分层架构**：清晰的职责分离和依赖管理
- **事件驱动**：基于 R3 的响应式事件系统
- **中介者模式**：解耦的命令和查询处理
- **资源管理**：智能缓存和内存监控
- **性能监控**：实时性能指标收集
- **依赖注入**：基于 Autofac 的 IoC 容器

## 🏗️ 架构设计

### 分层结构

```
src/
├── 0_Contracts/          # 契约层 - 接口定义和数据传输对象
│   ├── Abstractions/     # 核心抽象接口
│   ├── Attributes/       # 自定义特性
│   └── Events/          # 事件定义
├── 1_Contexts/          # 上下文层 - 依赖注入配置
├── 2_Infrastructure/    # 基础设施层 - 具体实现
│   ├── Caching/         # 缓存服务
│   ├── Logging/         # 日志服务
│   ├── Messaging/       # 消息传递
│   ├── Monitoring/      # 性能监控
│   ├── ResourceLoading/ # 资源加载
│   └── ResourceManagement/ # 资源管理
└── 3_Repositories/      # 仓储层 - 数据访问
```

### 核心组件

#### 🔄 事件系统
- **IEventBus**: 事件发布和订阅接口
- **IEventSubscriber**: 仅订阅功能的接口
- **R3EventBus**: 基于 R3 的高性能事件总线实现

#### 📨 中介者模式
- **IMyMediator**: 自定义中介者接口（无 MediatR 依赖）
- **ICommand/IQuery**: 命令和查询接口
- **MediatRAdapter**: MediatR 适配器实现

#### 💾 缓存系统
- **ICacheService**: 缓存服务抽象
- **MemoryCacheService**: 内存缓存实现

#### 📊 监控系统
- **IPerformanceMonitor**: 性能监控接口
- **IMemoryMonitor**: 内存监控接口

## 🚀 快速开始

### 1. 环境要求

- .NET 9.0
- Godot 4.4.1
- Visual Studio 2022 或 JetBrains Rider

### 2. 项目结构

```bash
# 克隆项目
git clone <repository-url>
cd ModularGodot.Core

# 构建解决方案
dotnet build src/ModularGodot.Core.sln
```

### 3. 基本使用

#### 事件系统使用示例

```csharp
// 订阅事件
var eventBus = container.Resolve<IEventBus>();
var subscription = eventBus.Subscribe<ResourceLoadEvent>(evt => 
{
    Console.WriteLine($"资源加载完成: {evt.ResourcePath}");
});

// 发布事件
await eventBus.PublishAsync(new ResourceLoadEvent(
    resourcePath: "res://textures/player.png",
    resourceType: "Texture2D",
    result: ResourceLoadResult.Success,
    loadTime: TimeSpan.FromMilliseconds(50),
    resourceSize: 1024,
    fromCache: false
));

// 取消订阅
subscription.Dispose();
```

#### 中介者模式使用示例

```csharp
// 定义命令
public class LoadResourceCommand : ICommand<Resource>
{
    public string ResourcePath { get; }
    public LoadResourceCommand(string resourcePath) => ResourcePath = resourcePath;
}

// 定义处理器
public class LoadResourceHandler : IMyCommandHandler<LoadResourceCommand, Resource>
{
    public async Task<Resource> Handle(LoadResourceCommand command, CancellationToken cancellationToken)
    {
        // 实现资源加载逻辑
        return await LoadResourceAsync(command.ResourcePath);
    }
}

// 使用中介者
var mediator = container.Resolve<IMyMediator>();
var resource = await mediator.Send(new LoadResourceCommand("res://scenes/main.tscn"));
```

## 📚 详细文档

- [架构设计文档](docs/Architecture.md) - 详细的架构说明和设计原则
- [使用示例](docs/Examples.md) - 完整的使用示例和最佳实践
- [API 参考](docs/API-Reference.md) - 详细的 API 文档

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

### SOLID 原则
- **单一职责原则**: 每个类只有一个变化的理由
- **开闭原则**: 对扩展开放，对修改关闭
- **里氏替换原则**: 子类可以替换父类
- **接口隔离原则**: 客户端不应依赖不需要的接口
- **依赖倒置原则**: 依赖抽象而非具体实现

### 分层架构原则
- **依赖方向**: 只能向内层依赖，不能反向依赖
- **接口隔离**: 通过接口定义层间契约
- **关注点分离**: 每层专注于特定职责

## 🔧 开发指南

### 添加新功能

1. **定义接口** - 在 `0_Contracts/Abstractions` 中定义抽象接口
2. **实现功能** - 在 `2_Infrastructure` 中提供具体实现
3. **配置依赖** - 在 `1_Contexts` 中注册服务
4. **编写测试** - 确保功能正确性

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