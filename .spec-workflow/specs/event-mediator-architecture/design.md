# 设计规范 - 事件系统和中介者模式架构

## 📋 目录

- [设计概述](#设计概述)
- [架构设计](#架构设计)
- [组件设计](#组件设计)
- [接口设计](#接口设计)
- [数据流设计](#数据流设计)
- [安全设计](#安全设计)
- [性能设计](#性能设计)
- [扩展性设计](#扩展性设计)

## 设计概述

### 设计原则

基于"分离抽象与实现"的核心原则，本设计采用以下关键策略：

1. **契约优先设计**：所有组件通过稳定的接口契约交互
2. **适配器模式**：隔离第三方库依赖，保持架构纯净
3. **分层架构**：清晰的职责分离和依赖方向
4. **零分配优化**：关键路径的内存分配优化

### 设计目标

- **稳定性**：核心API长期稳定，避免破坏性变更
- **性能**：事件分发延迟 < 1ms，零内存分配
- **可扩展性**：支持运行时插件加载和实现替换
- **可维护性**：清晰的代码结构和完整的文档

## 架构设计

### 整体架构图

```
┌─────────────────────────────────────────────────────────────┐
│                    Plugin Ecosystem                         │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐        │
│  │   Plugin A  │  │   Plugin B  │  │   Plugin C  │        │
│  └─────────────┘  └─────────────┘  └─────────────┘        │
└─────────────────────┬───────────────────┬───────────────────┘
                      │                   │
                      ▼                   ▼
┌─────────────────────────────────────────────────────────────┐
│                 Contracts Layer                             │
│  ┌─────────────────┐  ┌─────────────────┐                  │
│  │   IEventBus     │  │   IMyMediator   │                  │
│  │   IEventData    │  │   ICommand<T>   │                  │
│  │   ISubscriber   │  │   IQuery<T>     │                  │
│  └─────────────────┘  └─────────────────┘                  │
└─────────────────────┬───────────────────┬───────────────────┘
                      │                   │
                      ▼                   ▼
┌─────────────────────────────────────────────────────────────┐
│                Infrastructure Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐                  │
│  │  R3EventBus     │  │ MediatRAdapter  │                  │
│  │  (Zero Alloc)   │  │ (Anti-Corrupt)  │                  │
│  └─────────────────┘  └─────────────────┘                  │
└─────────────────────┬───────────────────┬───────────────────┘
                      │                   │
                      ▼                   ▼
┌─────────────────────────────────────────────────────────────┐
│                 External Libraries                          │
│  ┌─────────────────┐  ┌─────────────────┐                  │
│  │       R3        │  │    MediatR      │                  │
│  │  (Reactive)     │  │   (CQRS)        │                  │
│  └─────────────────┘  └─────────────────┘                  │
└─────────────────────────────────────────────────────────────┘
```

### 依赖关系

```
Plugins → Contracts ← Infrastructure → External Libraries
```

**关键特征**：
- 所有依赖箭头指向Contracts层
- Infrastructure层可独立更新
- Plugins与External Libraries完全隔离

### 分层职责

#### 1. Contracts Layer (契约层)
- **职责**：定义稳定的API契约
- **包含**：接口定义、数据结构、枚举常量
- **依赖**：零外部依赖
- **版本策略**：极其保守的版本更新

#### 2. Infrastructure Layer (基础设施层)
- **职责**：提供具体实现和适配器
- **包含**：事件总线实现、中介者适配器
- **依赖**：Contracts + 第三方库
- **版本策略**：独立版本，向后兼容

#### 3. Plugin Layer (插件层)
- **职责**：业务逻辑实现
- **包含**：游戏模块、扩展功能
- **依赖**：仅依赖Contracts
- **版本策略**：独立开发和部署

## 组件设计

### 1. 事件系统组件

#### EventBus核心组件

```csharp
// 契约定义
public interface IEventBus
{
    void Publish<T>(T eventData) where T : class;
    IDisposable Subscribe<T>(Action<T> handler) where T : class;
    IObservable<T> GetObservable<T>() where T : class;
}

// R3实现
public class R3EventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, Subject<object>> _subjects;
    private readonly CompositeDisposable _disposables;
    
    public void Publish<T>(T eventData) where T : class
    {
        if (_subjects.TryGetValue(typeof(T), out var subject))
        {
            subject.OnNext(eventData);
        }
    }
    
    public IDisposable Subscribe<T>(Action<T> handler) where T : class
    {
        var subject = _subjects.GetOrAdd(typeof(T), _ => new Subject<object>());
        return subject.OfType<T>().Subscribe(handler);
    }
}
```

#### 事件数据结构

```csharp
// 基础事件接口
public interface IEventData
{
    DateTime Timestamp { get; }
    string EventId { get; }
}

// 游戏事件基类
public abstract class GameEventBase : IEventData
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public string EventId { get; } = Guid.NewGuid().ToString();
}

// 具体事件示例
public class PlayerLoggedInEvent : GameEventBase
{
    public string PlayerId { get; init; }
    public string PlayerName { get; init; }
    public Vector3 Position { get; init; }
}
```

### 2. 中介者系统组件

#### Mediator核心组件

```csharp
// 契约定义
public interface IMyMediator
{
    Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
    Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
    Task Publish<T>(T notification, CancellationToken cancellationToken = default) where T : INotification;
}

// 命令和查询接口
public interface ICommand<out TResponse> { }
public interface IQuery<out TResponse> { }
public interface INotification { }

// 处理器接口
public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface IQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
}
```

#### MediatR适配器

```csharp
public class MediatRAdapter : IMyMediator
{
    private readonly IMediator _mediatR;
    private readonly ILogger<MediatRAdapter> _logger;
    
    public MediatRAdapter(IMediator mediatR, ILogger<MediatRAdapter> logger)
    {
        _mediatR = mediatR;
        _logger = logger;
    }
    
    public async Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        try
        {
            var wrapper = new MediatRCommandWrapper<TResponse>(command);
            return await _mediatR.Send(wrapper, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Command execution failed: {CommandType}", command.GetType().Name);
            throw;
        }
    }
}

// 适配器包装器
internal class MediatRCommandWrapper<TResponse> : IRequest<TResponse>
{
    public ICommand<TResponse> Command { get; }
    
    public MediatRCommandWrapper(ICommand<TResponse> command)
    {
        Command = command;
    }
}
```

### 3. 依赖注入组件

#### 服务注册

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventMediatorArchitecture(this IServiceCollection services)
    {
        // 注册契约实现
        services.AddSingleton<IEventBus, R3EventBus>();
        services.AddScoped<IMyMediator, MediatRAdapter>();
        
        // 注册MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        // 注册处理器发现
        services.AddTransient<IHandlerRegistry, HandlerRegistry>();
        
        return services;
    }
}
```

## 接口设计

### 1. 事件系统接口

#### IEventBus接口

```csharp
/// <summary>
/// 事件总线接口，提供发布订阅功能
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// 发布事件到所有订阅者
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <param name="eventData">事件数据</param>
    void Publish<T>(T eventData) where T : class;
    
    /// <summary>
    /// 订阅特定类型的事件
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <param name="handler">事件处理器</param>
    /// <returns>订阅令牌，用于取消订阅</returns>
    IDisposable Subscribe<T>(Action<T> handler) where T : class;
    
    /// <summary>
    /// 获取事件的可观察流
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <returns>事件流</returns>
    IObservable<T> GetObservable<T>() where T : class;
    
    /// <summary>
    /// 订阅事件（异步处理）
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <param name="handler">异步事件处理器</param>
    /// <returns>订阅令牌</returns>
    IDisposable SubscribeAsync<T>(Func<T, Task> handler) where T : class;
}
```

#### IEventSubscriber接口

```csharp
/// <summary>
/// 事件订阅者接口，用于插件实现
/// </summary>
public interface IEventSubscriber
{
    /// <summary>
    /// 订阅事件
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <param name="handler">处理器</param>
    /// <returns>订阅令牌</returns>
    IDisposable Subscribe<T>(Action<T> handler) where T : class;
    
    /// <summary>
    /// 批量订阅多个事件类型
    /// </summary>
    /// <param name="subscriptions">订阅配置</param>
    /// <returns>复合订阅令牌</returns>
    IDisposable SubscribeMultiple(params EventSubscription[] subscriptions);
}

public class EventSubscription
{
    public Type EventType { get; init; }
    public Delegate Handler { get; init; }
    public bool IsAsync { get; init; }
}
```

### 2. 中介者系统接口

#### IMyMediator接口

```csharp
/// <summary>
/// 中介者接口，实现CQRS模式
/// </summary>
public interface IMyMediator
{
    /// <summary>
    /// 发送命令
    /// </summary>
    /// <typeparam name="TResponse">响应类型</typeparam>
    /// <param name="command">命令对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>命令执行结果</returns>
    Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 发送查询
    /// </summary>
    /// <typeparam name="TResponse">响应类型</typeparam>
    /// <param name="query">查询对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>查询结果</returns>
    Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 发布通知到多个处理器
    /// </summary>
    /// <typeparam name="T">通知类型</typeparam>
    /// <param name="notification">通知对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task Publish<T>(T notification, CancellationToken cancellationToken = default) where T : INotification;
}
```

#### 命令查询接口

```csharp
/// <summary>
/// 命令接口标记
/// </summary>
/// <typeparam name="TResponse">响应类型</typeparam>
public interface ICommand<out TResponse> { }

/// <summary>
/// 查询接口标记
/// </summary>
/// <typeparam name="TResponse">响应类型</typeparam>
public interface IQuery<out TResponse> { }

/// <summary>
/// 通知接口标记
/// </summary>
public interface INotification { }

/// <summary>
/// 命令处理器接口
/// </summary>
/// <typeparam name="TCommand">命令类型</typeparam>
/// <typeparam name="TResponse">响应类型</typeparam>
public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// 查询处理器接口
/// </summary>
/// <typeparam name="TQuery">查询类型</typeparam>
/// <typeparam name="TResponse">响应类型</typeparam>
public interface IQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
}

/// <summary>
/// 通知处理器接口
/// </summary>
/// <typeparam name="T">通知类型</typeparam>
public interface INotificationHandler<in T> where T : INotification
{
    Task Handle(T notification, CancellationToken cancellationToken);
}
```

## 数据流设计

### 1. 事件发布流程

```
Plugin A                EventBus               Plugin B
   │                       │                     │
   │ 1. Publish(Event)     │                     │
   ├──────────────────────►│                     │
   │                       │ 2. Distribute       │
   │                       ├────────────────────►│
   │                       │                     │ 3. Handle(Event)
   │                       │                     ├─────────────────►
   │                       │                     │
   │ 4. Continue           │ 5. Ack              │ 6. Complete
   ◄───────────────────────┼─────────────────────┼─────────────────►
```

### 2. 命令处理流程

```
Plugin                 Mediator              Handler
   │                      │                     │
   │ 1. Send(Command)     │                     │
   ├─────────────────────►│                     │
   │                      │ 2. Route            │
   │                      ├────────────────────►│
   │                      │                     │ 3. Handle
   │                      │                     ├──────────►
   │                      │ 4. Response         │
   │                      ◄────────────────────┤
   │ 5. Result            │                     │
   ◄─────────────────────┤                     │
```

### 3. 错误处理流程

```
Component              Error Handler          Logger
   │                      │                     │
   │ 1. Exception         │                     │
   ├─────────────────────►│                     │
   │                      │ 2. Log Error        │
   │                      ├────────────────────►│
   │                      │ 3. Recover          │
   │                      ├──────────────►      │
   │ 4. Fallback          │                     │
   ◄─────────────────────┤                     │
```

## 安全设计

### 1. 输入验证

```csharp
public class EventValidationMiddleware : IEventMiddleware
{
    public async Task<bool> ProcessAsync<T>(T eventData) where T : class
    {
        // 验证事件数据
        if (eventData == null)
            throw new ArgumentNullException(nameof(eventData));
            
        // 验证事件类型
        if (!IsValidEventType<T>())
            throw new InvalidOperationException($"Invalid event type: {typeof(T).Name}");
            
        // 验证事件内容
        var validationResult = await ValidateEventContent(eventData);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
            
        return true;
    }
}
```

### 2. 权限控制

```csharp
public class EventAuthorizationMiddleware : IEventMiddleware
{
    private readonly IAuthorizationService _authService;
    
    public async Task<bool> ProcessAsync<T>(T eventData) where T : class
    {
        var eventType = typeof(T);
        var currentUser = GetCurrentUser();
        
        var authResult = await _authService.AuthorizeAsync(currentUser, eventData, "EventPublish");
        
        if (!authResult.Succeeded)
        {
            throw new UnauthorizedAccessException($"User {currentUser.Id} not authorized to publish {eventType.Name}");
        }
        
        return true;
    }
}
```

### 3. 数据保护

```csharp
public class EventEncryptionMiddleware : IEventMiddleware
{
    private readonly IDataProtector _protector;
    
    public async Task<bool> ProcessAsync<T>(T eventData) where T : class
    {
        if (eventData is ISensitiveEvent sensitiveEvent)
        {
            // 加密敏感数据
            sensitiveEvent.EncryptSensitiveData(_protector);
        }
        
        return true;
    }
}
```

## 性能设计

### 1. 零分配优化

```csharp
public class ZeroAllocEventBus : IEventBus
{
    private readonly ObjectPool<EventContext> _contextPool;
    private readonly ConcurrentDictionary<Type, FastSubject> _subjects;
    
    public void Publish<T>(T eventData) where T : class
    {
        var context = _contextPool.Get();
        try
        {
            context.EventData = eventData;
            context.EventType = typeof(T);
            
            if (_subjects.TryGetValue(typeof(T), out var subject))
            {
                subject.OnNext(context);
            }
        }
        finally
        {
            _contextPool.Return(context);
        }
    }
}
```

### 2. 批量处理

```csharp
public class BatchEventProcessor : IEventProcessor
{
    private readonly Channel<EventBatch> _channel;
    private readonly Timer _flushTimer;
    
    public async Task ProcessBatchAsync(EventBatch batch)
    {
        var tasks = new Task[batch.Events.Count];
        
        for (int i = 0; i < batch.Events.Count; i++)
        {
            tasks[i] = ProcessSingleEventAsync(batch.Events[i]);
        }
        
        await Task.WhenAll(tasks);
    }
}
```

### 3. 缓存优化

```csharp
public class CachedHandlerRegistry : IHandlerRegistry
{
    private readonly ConcurrentDictionary<Type, HandlerInfo[]> _handlerCache;
    private readonly IMemoryCache _typeCache;
    
    public HandlerInfo[] GetHandlers<T>()
    {
        return _handlerCache.GetOrAdd(typeof(T), type =>
        {
            return DiscoverHandlers(type).ToArray();
        });
    }
}
```

## 扩展性设计

### 1. 插件接口

```csharp
public interface IEventPlugin
{
    string Name { get; }
    Version Version { get; }
    
    Task InitializeAsync(IEventBus eventBus, IServiceProvider services);
    Task ShutdownAsync();
    
    IEnumerable<Type> GetEventTypes();
    IEnumerable<Type> GetHandlerTypes();
}
```

### 2. 中间件管道

```csharp
public interface IEventMiddleware
{
    Task<bool> ProcessAsync<T>(T eventData) where T : class;
}

public class EventMiddlewarePipeline
{
    private readonly List<IEventMiddleware> _middlewares;
    
    public async Task<bool> ExecuteAsync<T>(T eventData) where T : class
    {
        foreach (var middleware in _middlewares)
        {
            if (!await middleware.ProcessAsync(eventData))
                return false;
        }
        return true;
    }
}
```

### 3. 配置系统

```csharp
public class EventBusConfiguration
{
    public int MaxConcurrentEvents { get; set; } = 1000;
    public TimeSpan EventTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnableBatching { get; set; } = true;
    public int BatchSize { get; set; } = 100;
    public TimeSpan BatchTimeout { get; set; } = TimeSpan.FromMilliseconds(10);
    
    public List<Type> MiddlewareTypes { get; set; } = new();
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}
```

---

## 总结

本设计规范基于"分离抽象与实现"的核心原则，通过清晰的分层架构、稳定的接口契约和高效的实现策略，为ModularGodot.Core框架提供了一个健壮、高性能、可扩展的事件系统和中介者模式架构。

**关键设计特征**：
- **契约稳定性**：核心接口设计保持长期稳定
- **实现灵活性**：支持多种实现策略和运行时替换
- **性能优化**：零分配事件处理和批量优化
- **扩展能力**：完善的插件系统和中间件支持

该设计为构建大规模、高性能的模块化游戏系统奠定了坚实的架构基础。