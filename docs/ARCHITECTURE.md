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

### 事件总线资源管理

增强的事件总线系统实现了高效资源管理，以防止内存泄漏和确保线程安全：

1. **订阅资源跟踪**: 使用 ConcurrentDictionary 跟踪所有活动订阅，支持基于 ID 的精确管理
2. **自动资源清理**: 通过 Composite Disposal 模式自动清理订阅和主题资源
3. **线程安全保护**: 使用 ReaderWriterLockSlim 实现订阅管理的线程安全控制
4. **一次性订阅**: 提供 SubscribeOnce 方法，处理首个事件后自动清理订阅
5. **主题资源管理**: 支持按类型进行事件主题管理，具有垃圾回收和清理机制
6. **生命周期监控**: 集成 IDisposable 模式，确保在框架关闭时彻底清理所有资源

**实现细节**：
- 每个订阅返回唯一 ID，允许精确的取消订阅控制
- R3EventBus 维护订阅和主题的内部映射表，支持 O(1) 访问和清理
- 使用 Create-Recreate 保护模式确保并行环境下的资源管理一致性
- 实现停止/已处置状态管理，防止生命周期外的资源访问
- 集成性能计数器，跟踪发布/订阅/取消订阅操作以支持监控

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