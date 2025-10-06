# ModularGodot.Core 事件系统使用指南

## 概述

ModularGodot.Core 框架提供了一个高性能的响应式事件系统，该系统建立在 R3（响应式扩展库）之上。这个事件系统通过允许组件通过事件而无需直接依赖来进行通信，实现组件之间的松散耦合。`R3EventBus` 实现支持同步和异步操作、过滤、一次性订阅和适当的错误处理。

## 事件结构

所有事件必须继承自 `EventBase`:

```csharp
public abstract class EventBase
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}
```

## 创建事件

通过继承 `EventBase` 来创建自定义事件：

```csharp
public class UserCreatedEvent : EventBase
{
    public Guid UserId { get; }
    public string UserName { get; }
    public string Email { get; }

    public UserCreatedEvent(Guid userId, string userName, string email)
    {
        UserId = userId;
        UserName = userName;
        Email = email;
    }
}

public class UserUpdatedEvent : EventBase
{
    public Guid UserId { get; }
    public string UpdatedField { get; }

    public UserUpdatedEvent(Guid userId, string updatedField)
    {
        UserId = userId;
        UpdatedField = updatedField;
    }
}
```

## 发布事件

要发布事件，注入 `IEventBus` 并使用同步或异步发布：

### 同步发布
```csharp
[Injectable(Lifetime.Scoped)]
public class UserService
{
    private readonly IEventBus _eventBus;

    public UserService(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        // 处理命令
        var user = new User { /* ... */ };
        // ... 用户创建逻辑

        // 发布同步事件
        _eventBus.Publish(new UserCreatedEvent(user.Id, user.Name, user.Email));

        return new UserDto { /* ... */ };
    }
}
```

### 异步发布
```csharp
public async Task<UserDto> UpdateUserAsync(UpdateUserCommand command, CancellationToken cancellationToken = default)
{
    // 处理命令
    var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
    // ... 更新逻辑

    // 发布异步事件
    await _eventBus.PublishAsync(new UserUpdatedEvent(user.Id, command.Field), cancellationToken);

    return new UserDto { /* ... */ };
}
```

## 订阅事件

### 基本事件订阅
```csharp
[Injectable(Lifetime.Singleton)]
public class EmailNotificationService
{
    public EmailNotificationService(IEventBus eventBus)
    {
        // 订阅 UserCreatedEvent
        eventBus.Subscribe<UserCreatedEvent>(OnUserCreated);
    }

    private void OnUserCreated(UserCreatedEvent @event)
    {
        // 处理用户创建事件
        Console.WriteLine($"向 {@event.Email} 发送欢迎邮件");
        // 邮件发送逻辑在这里
    }
}
```

### 异步事件处理
```csharp
[Injectable(Lifetime.Singleton)]
public class AuditLogService
{
    private readonly IEventBus _eventBus;

    public AuditLogService(IEventBus eventBus)
    {
        _eventBus = eventBus;

        // 订阅异步处理器
        _eventBus.Subscribe<UserCreatedEvent>(HandleUserCreatedAsync);
        _eventBus.Subscribe<UserUpdatedEvent>(HandleUserUpdatedAsync);
    }

    private async Task HandleUserCreatedAsync(UserCreatedEvent @event, CancellationToken cancellationToken)
    {
        await RecordAuditEntryAsync($"用户 {@event.UserId} 已创建", cancellationToken);
    }

    private async Task HandleUserUpdatedAsync(UserUpdatedEvent @event, CancellationToken cancellationToken)
    {
        await RecordAuditEntryAsync($"用户 {@event.UserId} 已更新 ({@event.UpdatedField})", cancellationToken);
    }

    private async Task RecordAuditEntryAsync(string message, CancellationToken cancellationToken)
    {
        // 异步审计日志
    }
}
```

### 基于条件的订阅
```csharp
public class EmailNotificationService
{
    public EmailNotificationService(IEventBus eventBus)
    {
        // 只订阅用户已选择接收邮件的事件
        eventBus.Subscribe<UserCreatedEvent>(
            @event => @event.UserOptedInToEmail, // 过滤条件
            @event => SendWelcomeEmail(@event)); // 处理程序
    }
}
```

### 一次性订阅
```csharp
public class InitializationService
{
    public InitializationService(IEventBus eventBus)
    {
        // 仅在 AppInitializedEvent 发布时执行一次
        eventBus.SubscribeOnce<AppInitializedEvent>(OnAppInitialized);
    }

    private void OnAppInitialized(AppInitializedEvent @event)
    {
        // 只需要发生一次的设置
    }
}
```

## 事件处理器最佳实践

### 同步事件处理

- 当操作简单快捷时使用同步处理器
- 让处理器保持轻量级，避免阻塞事件发布
- 适当处理异常以防止影响其他订阅者

### 异步事件处理

- 对 I/O 操作使用异步处理器（数据库、网络、文件系统）
- 为可以被取消的操作正确处理取消令牌
- 请注意，异步处理器在后台任务中运行，不会阻止发布

### 错误处理

事件总线在内部处理错误，不允许它们传播到其他订阅者：

```csharp
private void OnUserCreated(UserCreatedEvent @event)
{
    try
    {
        // 您的事件处理逻辑在这里
    }
    catch (Exception ex)
    {
        // 事件总线在内部记录错误
        // 其他订阅者不受此异常影响
        _logger.LogError(ex, "处理用户 {@event.UserId} 的 UserCreatedEvent 时出错");
    }
}
```

## 完整示例：玩家成就系统

这里是一个完整示例，展示了使用事件基础设施的玩家成就系统：

**事件:**
```csharp
public class PlayerLevelUpEvent : EventBase
{
    public Guid PlayerId { get; }
    public int NewLevel { get; }
    public int OldLevel { get; }

    public PlayerLevelUpEvent(Guid playerId, int newLevel, int oldLevel)
    {
        PlayerId = playerId;
        NewLevel = newLevel;
        OldLevel = oldLevel;
    }
}

public class AchievementUnlockedEvent : EventBase
{
    public Guid PlayerId { get; }
    public string AchievementId { get; }
    public string AchievementName { get; }

    public AchievementUnlockedEvent(Guid playerId, string achievementId, string achievementName)
    {
        PlayerId = playerId;
        AchievementId = achievementId;
        AchievementName = achievementName;
    }
}
```

**成就处理器:**
```csharp
[Injectable(Lifetime.Singleton)]
public class AchievementService
{
    private readonly IEventBus _eventBus;
    private readonly List<AchievementDefinition> _achievements;

    public AchievementService(IEventBus eventBus)
    {
        _eventBus = eventBus;
        _achievements = LoadAchievements();

        // 订阅相关事件
        _eventBus.Subscribe<PlayerLevelUpEvent>(OnPlayerLevelUp);
    }

    private void OnPlayerLevelUp(PlayerLevelUpEvent @event)
    {
        // 检查等级类成就
        foreach (var achievement in _achievements.Where(a => a.Type == AchievementType.Level))
        {
            if (@event.NewLevel >= achievement.RequirementValue)
            {
                _eventBus.Publish(new AchievementUnlockedEvent(
                    @event.PlayerId,
                    achievement.Id,
                    achievement.Name));
            }
        }
    }
}
```

**通知服务:**
```csharp
[Injectable(Lifetime.Singleton)]
public class AchievementNotificationService
{
    public AchievementNotificationService(IEventBus eventBus)
    {
        eventBus.Subscribe<AchievementUnlockedEvent>(OnAchievementUnlocked);
    }

    private void OnAchievementUnlocked(AchievementUnlockedEvent @event)
    {
        // 向玩家显示UI通知
        Console.WriteLine($"🎉 成就解锁! {@event.AchievementName}");
        // GUI通知逻辑在这里
    }
}
```

## 事件总线生命周期

R3EventBus 实现了适当的处置模式：

- 所有订阅都通过响应式扩展的 `IDisposable` 进行管理
- 资源通过 `CompositeDisposable` 自动管理
- 当总线被处置时，事件和主题被正确处置
- 处置后对 `Publish` 的调用将被安全忽略

## 性能注意事项

- 使用 `PublishAsync` 时，事件以异步方式发布
- 事件在后台线程上处理
- 消费者可以是异步的，而不会阻塞发布者
- <1ms 中位路由时间性能目标
- 具有适当的清理功能，内存效率高

## 常见问题故障排除

**1. 事件没有被发布:**
- 检查 event_bus 是否正确注入
- 验证事件是否继承自 EventBase
- 确保服务注册正确

**2. 事件没有被处理:**
- 验证订阅者是否在应用程序初始化期间注册
- 检查事件类型是否完全匹配
- 确认事件确实已发布

**3. 内存泄漏:**
- 避免在瞬态服务中创建订阅而不进行适当的清理
- 如果需要稍后取消订阅，请保留从 Subscribe 调用返回的 `IDisposable` 值的引用
- 对于只应处理一次的事件使用 `SubscribeOnce`