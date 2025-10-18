# ModularGodot.Core

## 项目概述

本项目是一个基于 Godot 4.4 的模块化、插件化的游戏开发框架，使用 C# 和 .NET 9 构建。它专为企业级游戏开发而设计，强调清晰的、分层的体系结构和现代设计模式。该框架为构建可扩展和可维护的游戏提供了坚实的基础设施。

**关键技术:**

*   **游戏引擎:** Godot 4.4
*   **核心框架:** .NET 9
*   **依赖注入:** Autofac
*   **消息传递:** MediatR 和 R3 (Reactive Extensions for .NET)
*   **缓存:** Microsoft.Extensions.Caching.Memory

**体系结构:**

该框架遵循严格的分层体系结构，促进了关注点分离：

*   **Contracts:** 定义接口、DTO 和事件。
*   **Contexts:** 处理依赖注入配置。
*   **Infrastructure:** 包含缓存、日志和消息传递等服务的具体实现。
*   **Repositories:** 管理数据访问。

## 构建和运行

### 先决条件

*   .NET 9.0 SDK
*   Godot 4.4

### 构建

可以使用以下命令构建和打包项目：

```bash
# 构建整个解决方案
dotnet build src/ModularGodot.Core.sln

# 将所有项目打包到 NuGet 包中
dotnet pack src/ModularGodot.Core.sln -c Release -o packages
```

该项目还包括 PowerShell 脚本，以简化构建过程：

```bash
# 增强的构建和打包脚本
./tools/enhanced-build-pack.ps1 -Configuration Release

# 清理构建产物
./tools/cleanup.ps1
```

### 运行测试

该框架包括一个全面的测试套件。

**单元测试:**

```bash
# 运行所有单元测试
dotnet test src/ModularGodot.Core.XUnitTests/ModularGodot.Core.XUnitTests.csproj
```

**集成测试:**

集成测试在 Godot 编辑器中运行。

1.  在 Godot 编辑器中打开 `ModularGodot.Core.Test` 项目。
2.  打开一个测试场景 (例如, `MediatorTestScene.tscn`)。
3.  单击场景中的“RunTest”按钮以执行测试。

## 开发约定

### 插件体系结构

该框架围绕插件体系结构构建。每个插件由两个包组成：

*   **Contracts 包:** 包含插件的公共 API（接口、DTO、事件）。
*   **Implementation 包:** 包含插件功能的具体实现。

这种分离确保了插件之间的松散耦合。

### 依赖注入

该框架使用 Autofac 进行依赖注入。服务使用 `[Injectable]` 特性自动注册，该特性还指定了服务的生命周期（Singleton、Scoped 或 Transient）。

### 事件驱动体系结构

该框架使用基于 R3 的事件驱动体系结构。`IEventBus` 接口用于发布和订阅事件。

### 中介者模式

中介者模式用于解耦命令和查询处理。`IDispatcher` 接口发送命令和查询，然后由其各自的处理程序进行处理。