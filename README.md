# 1. 项目名称/标题

ModularGodot.Core

## 2. 项目描述

一个基于 Godot 4.5 的插件化游戏开发框架，采用模块化架构和现代 C# 设计模式。

ModularGodot.Core 是一个为 Godot 游戏开发设计的企业级框架，以插件化架构为核心。框架通过模块化设计和自动依赖注入机制，让开发者能够轻松创建可扩展的功能插件，实现真正的即插即用。

## 3. 主要功能

- **插件化架构**：以插件为核心的设计理念，支持动态扩展
- **自动依赖注入**：基于 Autofac 的 IoC 容器，简化服务注册和解析
- **事件驱动**：基于 R3 的响应式事件系统
- **事件总线**：线程安全、资源高效的事件总线实现
- **中介者模式**：解耦的命令和查询处理
- **资源管理**：智能缓存和内存监控
- **性能监控**：实时性能指标收集

## 4. 快速开始

### 先决条件

- .NET 9.0
- Godot 4.5
- **Windows**: Visual Studio 2022 或 JetBrains Rider
- **Linux**: VS Code 或 JetBrains Rider

### 安装

进入项目目录，执行以下命令安装核心包：

```bash
dotnet add package ModularGodot.Core
```

核心包将自动解析并安装所有必要的依赖项，包括：

- `ModularGodot.Core.Contracts` - 核心契约层
- `ModularGodot.Core.Contexts` - 核心上下文层
- `ModularGodot.Core.Infrastructure` - 核心基础设施层
- `ModularGodot.Core.Repositories` - 核心仓储层

随后你会发现项目目录下会出现“AutoLoads”文件夹，其中包含了 `MiddlewareProvider` 节点。

### 使用方法

#### Godot 集成

该项目提供了一个 `MiddlewareProvider` 节点，用于在 Godot 环境中轻松访问核心服务：

1. 将 `MiddlewareProvider` 节点添加到全局自动加载列表中
2. 通过单例实例访问核心服务：

```csharp
// 获取全局服务实例
var globalService = GodotGlobalService.Instance;

// 解析调度器接口
var dispatcher = globalService.ResolveDispatcher();

// 解析事件总线接口
var eventBus = globalService.ResolveEventBus();
```

## 5. 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 6. 致谢

我们衷心感谢以下开源项目及其贡献者，他们的卓越工作为ModularGodot.Core框架提供了坚实的基础：

- **Godot Engine** - 感谢Godot引擎团队提供了强大、开源的游戏引擎，使本项目得以实现。
- **Autofac** - 感谢Autofac团队提供了功能强大的依赖注入容器，为我们的模块化架构提供了核心支持。
- **MediatR** - 感谢Jimmy Bogard及社区贡献者开发了简洁而强大的中介者模式实现，帮助我们实现了解耦的命令和查询处理。
- **Microsoft.Extensions.Caching** - 感谢微软团队提供了高效的缓存抽象和内存缓存实现，为我们的资源管理功能提供了支持。
- **System.Reactive** - 感谢微软团队和Rx社区提供了响应式编程框架，为我们的事件驱动架构奠定了基础。
- **R3** - 感谢Yoshifumi Kawai及团队提供了高性能的响应式编程库，增强了我们的事件系统。

这些优秀的开源项目不仅提供了技术支持，更体现了开源社区协作共享的精神。我们向所有为这些项目做出贡献的开发者表示最诚挚的感谢！


## 7. 路线图