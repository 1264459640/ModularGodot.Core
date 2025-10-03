# ModularGodot.Core 架构文档

## 项目概述

ModularGodot.Core 是一个为企业级 Godot 游戏开发设计的框架，具有分层架构和现代 C# 设计模式。项目提供了完整的基础设施支持，包括：

- 分层架构，关注点清晰分离
- 基于 R3 的事件驱动系统
- 中介者模式，实现命令和查询的解耦处理
- 资源管理，包含智能缓存和内存监控
- 性能监控，支持实时指标收集
- 基于 Autofac 的依赖注入容器

## 代码架构

项目遵循严格的分层架构：

```
src/
├── 0_Contracts/          # 契约层 - 接口定义和 DTO
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

## 核心架构组件

### 事件系统

事件系统基于 R3 响应式扩展构建：

- `IEventBus`: 事件发布和订阅接口
- `R3EventBus`: 基于 R3 的高性能事件总线实现
- 支持异步/同步发布、条件订阅和一次性订阅

### 中介者模式

中介者模式使用 MediatR 和自定义适配器：

- `IDispatcher`: 自定义中介者接口（无 MediatR 依赖）
- `IMyMediator`: 自定义中介者接口（无 MediatR 依赖）
- `MediatRAdapter`: MediatR 适配器实现，提供：
  - 完整的取消令牌支持（包括超时和协作取消）
  - 异常传播到调用者
  - 编译时类型安全
  - 单例依赖注入配置
  - 处理程序未找到时抛出自定义 HandlerNotFoundException
- `ICommand/IQuery`: 命令和查询接口
- `ICommandHandler/IQueryHandler`: 命令和查询处理器接口
- `CommandWrapper/QueryWrapper`: MediatR 请求包装器，提供类型安全的适配
- `CommandHandlerWrapper/QueryHandlerWrapper`: MediatR 处理器包装器，桥接框架接口和 MediatR

**实现细节**：
- 使用泛型约束确保编译时类型安全
- 通过 Autofac 进行依赖注入，MediatRAdapter 注册为单例
- 支持 <1ms 中位数路由时间的性能优化
- 适配器自动将框架命令/查询包装为 MediatR 请求
- 处理程序自动包装为 MediatR 请求处理器

### 缓存系统

支持可配置策略的资源缓存：

- `ICacheService`: 缓存服务抽象
- `MemoryCacheService`: 内存缓存实现
- 可配置的缓存策略（ResourceCacheStrategy 枚举）

### 监控系统

性能和内存监控：

- `IPerformanceMonitor`: 性能监控接口
- `IMemoryMonitor`: 内存监控接口
- 实时指标收集和报告

## 依赖注入

项目使用 Autofac 进行依赖注入，并带有自定义特性：

- `[Injectable(Lifetime.Singleton/Scoped/Transient)]`: 标记类以进行自动注册
- `Lifetime` 枚举: 定义服务生命周期（Singleton、Scoped、Transient）
- 基于模块的上下文层配置

## NuGet 包结构

项目支持多种使用模式：

1. **独立层包**（推荐）：
   - `ModularGodot.Core.Contracts` - 契约层
   - `ModularGodot.Core.Contexts` - 上下文层
   - `ModularGodot.Core.Infrastructure` - 基础设施层
   - `ModularGodot.Core.Repositories` - 仓储层

2. **完整框架包**：
   - `ModularGodot.Core` - 包含所有层

依赖层次结构遵循架构层：
```
ModularGodot.Core.Repositories
  ↓ 依赖于
ModularGodot.Core.Infrastructure
  ↓ 依赖于
ModularGodot.Core.Contexts
  ↓ 依赖于
ModularGodot.Core.Contracts
```

## 详细文档

有关依赖注入的详细信息，请参阅：
- [依赖注入文档](DEPENDENCY_INJECTION.md) - 依赖注入机制和使用说明