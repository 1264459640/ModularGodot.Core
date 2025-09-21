# 技术架构文档 - ModularGodot.Core

## 📋 目录

- [技术概述](#技术概述)
- [技术栈](#技术栈)
- [架构决策](#架构决策)
- [设计模式](#设计模式)
- [依赖管理](#依赖管理)
- [性能考量](#性能考量)
- [安全性](#安全性)
- [可扩展性](#可扩展性)
- [技术债务](#技术债务)

## 技术概述

ModularGodot.Core 是一个基于 .NET 9.0 和 Godot 4.4 的企业级游戏开发框架，采用现代 C# 设计模式和响应式编程理念。

### 核心技术理念

- **分层架构**：清晰的职责分离和依赖管理
- **事件驱动**：基于响应式编程的松耦合设计
- **依赖注入**：控制反转提高可测试性
- **中介者模式**：解耦业务逻辑处理
- **资源管理**：智能缓存和内存优化

## 技术栈

### 核心框架

| 技术 | 版本 | 用途 | 选择理由 |
|------|------|------|----------|
| **.NET** | 9.0 | 运行时平台 | 最新LTS版本，性能优化，现代C#特性 |
| **Godot** | 4.4.1 | 游戏引擎 | 开源、轻量级、C#支持完善 |
| **C#** | 12.0 | 编程语言 | 强类型、面向对象、丰富生态 |

### 依赖注入与IoC

| 库 | 版本 | 用途 | 选择理由 |
|-----|------|------|----------|
| **Autofac** | 8.3.0 | IoC容器 | 功能强大、性能优秀、模块化支持 |
| **MediatR** | 12.5.0/13.0.0 | 中介者模式 | 解耦业务逻辑、支持管道行为 |
| **MediatR.Extensions.Autofac** | 12.3.0 | 集成扩展 | 无缝集成Autofac和MediatR |

### 缓存与内存管理

| 库 | 版本 | 用途 | 选择理由 |
|-----|------|------|----------|
| **Microsoft.Extensions.Caching.Memory** | 9.0.7 | 内存缓存 | 官方实现、性能优化、易于使用 |
| **Microsoft.Extensions.Caching.Abstractions** | 9.0.7 | 缓存抽象 | 标准接口、便于扩展 |

### 响应式编程

| 库 | 版本 | 用途 | 选择理由 |
|-----|------|------|----------|
| **System.Reactive** | 6.0.0 | 响应式扩展 | 成熟的Rx实现、丰富的操作符 |
| **R3** | 1.3.0 | 现代响应式库 | 高性能、零分配、Unity/Godot优化 |

## 架构决策

### ADR-001: 分层架构设计

**决策**：采用4层架构模式
- 0_Contracts: 契约层
- 1_Contexts: 上下文层  
- 2_Infrastructure: 基础设施层
- 3_Repositories: 仓储层

**理由**：
- 清晰的职责分离
- 依赖方向单一（向内依赖）
- 便于测试和维护
- 支持模块化开发

**后果**：
- 增加了项目复杂度
- 需要更多的接口定义
- 学习成本较高

### ADR-002: 依赖注入容器选择

**决策**：选择Autofac作为IoC容器

**理由**：
- 功能丰富（模块化、装饰器、拦截器）
- 性能优秀
- 与MediatR集成良好
- 支持复杂的注册场景

**替代方案**：
- Microsoft.Extensions.DependencyInjection：功能相对简单
- Castle Windsor：配置复杂
- Unity：微软已停止维护

### ADR-003: 事件系统设计

**决策**：采用R3 + System.Reactive双重事件系统

**理由**：
- R3：高性能、零分配、适合游戏场景
- System.Reactive：成熟稳定、丰富操作符
- 双重选择满足不同性能需求

**实现**：
```csharp
public interface IEventBus
{
    void Publish<T>(T eventData) where T : class;
    IDisposable Subscribe<T>(Action<T> handler) where T : class;
    IObservable<T> GetObservable<T>() where T : class;
}
```

### ADR-004: 中介者模式实现

**决策**：使用MediatR实现CQRS模式

**理由**：
- 解耦请求发送者和处理者
- 支持管道行为（日志、验证、缓存）
- 便于单元测试
- 符合单一职责原则

**实现**：
```csharp
public interface IMyMediator
{
    Task<TResponse> Send<TResponse>(ICommand<TResponse> command);
    Task<TResponse> Send<TResponse>(IQuery<TResponse> query);
}
```

## 设计模式

### 1. 适配器模式 (Adapter Pattern)

**用途**：隔离第三方依赖，保持接口一致性

```csharp
// MediatR适配器
public class MediatRAdapter : IMyMediator
{
    private readonly IMediator _mediatR;
    
    public Task<TResponse> Send<TResponse>(ICommand<TResponse> command)
    {
        var wrapper = new MediatRRequestAdapter<TResponse>(command);
        return _mediatR.Send(wrapper);
    }
}
```

### 2. 策略模式 (Strategy Pattern)

**用途**：资源加载、缓存策略等可插拔实现

```csharp
public interface IResourceLoader
{
    Task<T> LoadAsync<T>(string path) where T : Resource;
    bool CanLoad(string path);
}
```

### 3. 观察者模式 (Observer Pattern)

**用途**：事件驱动架构的核心

```csharp
public interface IEventSubscriber
{
    IDisposable Subscribe<T>(Action<T> handler) where T : class;
}
```

### 4. 单例模式 (Singleton Pattern)

**用途**：全局服务管理

```csharp
[Injectable]
public class ResourceManager : IResourceManager
{
    // 通过IoC容器管理单例生命周期
}
```

## 依赖管理

### 项目依赖关系

```
┌─────────────────┐
│   Game Logic    │
└─────────┬───────┘
          │
┌─────────▼───────┐    ┌─────────────────┐
│   1_Contexts    │───▶│   0_Contracts   │
└─────────┬───────┘    └─────────────────┘
          │                     ▲
          ▼                     │
┌─────────────────┐             │
│2_Infrastructure │─────────────┘
└─────────┬───────┘
          │
          ▼
┌─────────────────┐
│ 3_Repositories  │
└─────────────────┘
```

### 包版本管理策略

1. **主版本锁定**：避免破坏性变更
2. **次版本更新**：定期更新获取新特性
3. **补丁版本**：及时更新修复安全问题
4. **依赖审计**：定期检查漏洞和过时依赖

### 自动注册机制

```csharp
// 基于特性的自动注册
[Injectable]
public class MemoryCacheService : IMemoryCacheService
{
    // 自动注册为单例
}

// 跳过自动注册
[SkipRegistration]
public class ManualService : IManualService
{
    // 需要手动注册
}
```

## 性能考量

### 1. 内存管理

- **对象池**：复用频繁创建的对象
- **弱引用**：避免内存泄漏
- **及时释放**：实现IDisposable模式
- **GC优化**：减少大对象堆分配

### 2. 事件系统优化

- **R3零分配**：游戏循环中使用R3
- **事件过滤**：避免不必要的处理
- **批量处理**：合并相似事件
- **异步处理**：避免阻塞主线程

### 3. 缓存策略

```csharp
public class CacheConfig
{
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(30);
    public int MaxCacheSize { get; set; } = 1000;
    public bool EnableCompression { get; set; } = true;
}
```

### 4. 资源加载优化

- **异步加载**：避免阻塞
- **预加载**：提前加载关键资源
- **流式加载**：大文件分块处理
- **压缩存储**：减少内存占用

## 安全性

### 1. 输入验证

```csharp
public class ValidatedCommand : ICommand<bool>
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    
    [Range(1, 1000)]
    public int Value { get; set; }
}
```

### 2. 异常处理

```csharp
public class GlobalExceptionHandler : IPipelineBehavior<IRequest<TResponse>, TResponse>
{
    public async Task<TResponse> Handle(IRequest<TResponse> request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            // 记录日志，返回安全响应
            _logger.LogError(ex, "处理请求时发生错误");
            throw;
        }
    }
}
```

### 3. 资源访问控制

- **路径验证**：防止目录遍历攻击
- **文件类型检查**：限制可加载的资源类型
- **大小限制**：防止内存耗尽攻击

## 可扩展性

### 1. 模块化设计

```csharp
public class CustomModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<CustomService>()
               .As<ICustomService>()
               .SingleInstance();
    }
}
```

### 2. 插件架构

- **接口定义**：标准化插件接口
- **动态加载**：运行时加载插件
- **生命周期管理**：插件的启动和停止
- **配置管理**：插件配置的统一管理

### 3. 事件扩展

```csharp
// 自定义事件
public class CustomGameEvent
{
    public string EventType { get; set; }
    public object Data { get; set; }
    public DateTime Timestamp { get; set; }
}

// 事件处理器
public class CustomEventHandler : INotificationHandler<CustomGameEvent>
{
    public Task Handle(CustomGameEvent notification, CancellationToken cancellationToken)
    {
        // 处理自定义事件
        return Task.CompletedTask;
    }
}
```

## 技术债务

### 当前已知问题

1. **测试覆盖率不足**
   - 优先级：高
   - 计划：Q1 2024完成单元测试
   - 影响：代码质量和维护性

2. **文档不完整**
   - 优先级：中
   - 计划：持续更新
   - 影响：开发效率

3. **性能基准测试缺失**
   - 优先级：中
   - 计划：Q2 2024建立基准
   - 影响：性能优化方向不明确

### 技术改进计划

1. **引入代码分析工具**
   - SonarQube：代码质量分析
   - Roslyn Analyzers：编译时检查
   - StyleCop：代码风格统一

2. **CI/CD流水线优化**
   - 自动化测试
   - 代码覆盖率报告
   - 自动化部署

3. **监控和诊断**
   - Application Insights集成
   - 性能计数器
   - 内存泄漏检测

---

## 总结

ModularGodot.Core采用现代.NET技术栈，通过分层架构、依赖注入、事件驱动等设计模式，为Godot游戏开发提供了一个可扩展、可维护、高性能的框架基础。

技术选型注重平衡功能性、性能和可维护性，为未来的扩展和优化留下了充足的空间。