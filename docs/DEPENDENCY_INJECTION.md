# 依赖注入文档

## 概述

ModularGodot.Core 框架采用基于 Autofac 的依赖注入机制，通过自动化容器配置简化了服务注册和解析过程。开发者只需通过标注属性关键字，容器即可自动识别并注入依赖，无需手动配置模块。

## 核心概念

### 1. 自动化注入机制

框架采用声明式依赖注入，所有具体实现类只需通过 `[Injectable]` 特性标记，容器会自动扫描并注册到依赖注入容器中。

```csharp
// 通过标记 Injectable 特性自动注册服务
[Injectable(Lifetime.Scoped)]
public class UserService : IUserService
{
    // 构造函数注入自动解析
    public UserService(IUserRepository repository, ILogger logger)
    {
        // 实现代码
    }
}
```

### 2. 生命周期管理

通过 `Lifetime` 枚举定义服务的生命周期：

- **Transient**：瞬态，每次请求都创建新实例
- **Scoped**：作用域，每个作用域内使用单例
- **Singleton**：单例，整个应用生命周期内使用单例

```csharp
// 瞬态服务
[Injectable(Lifetime.Transient)]
public class EmailValidator : IEmailValidator { }

// 作用域服务（推荐用于业务服务）
[Injectable(Lifetime.Scoped)]
public class UserService : IUserService { }

// 单例服务（推荐用于无状态工具类）
[Injectable(Lifetime.Singleton)]
public class DateTimeProvider : IDateTimeProvider { }
```

## 自动识别的组件

### 1. 命令和查询处理器

命令和查询包装器已在中介者模块中声明自动识别，处理器不需要标注属性关键字即可自动注入：

```csharp
// 命令处理器自动识别（无需标注 Injectable 特性）
public class CreateUserHandler : ICommandHandler<CreateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // 处理逻辑
    }
}

// 查询处理器自动识别（无需标注 Injectable 特性）
public class GetUserQueryHandler : IQueryHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        // 查询逻辑
    }
}
```

### 2. 事件处理器

事件处理器通过标记特性自动注册：

```csharp
// 事件处理器自动注册
[Injectable(Lifetime.Scoped)]
public class UserCreatedEventHandler
{
    private readonly IEmailService _emailService;

    public UserCreatedEventHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Handle(UserCreatedEvent evt, CancellationToken cancellationToken)
    {
        // 处理用户创建事件
        await _emailService.SendWelcomeEmailAsync(evt.UserEmail);
    }
}
```

### 3. 服务和基础设施

所有服务和基础设施组件通过特性标记自动注册：

```csharp
// 基础设施服务
[Injectable(Lifetime.Scoped)]
public class SqlUserRepository : IUserRepository
{
    private readonly IDbConnection _connection;

    public SqlUserRepository(IDbConnection connection)
    {
        _connection = connection;
    }
}

// 缓存服务
[Injectable(Lifetime.Singleton)]
public class RedisCacheService : ICacheService
{
    // 实现代码
}
```

## 使用示例

### 1. 服务消费

通过构造函数注入使用服务：

```csharp
[Injectable(Lifetime.Scoped)]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IEventBus _eventBus;

    // 构造函数注入，容器自动解析依赖
    public UserController(IUserService userService, IEventBus eventBus)
    {
        _userService = userService;
        _eventBus = eventBus;
    }

    public async Task<ActionResult<UserDto>> CreateUser(CreateUserRequest request)
    {
        var command = new CreateUserCommand(request.Name, request.Email);
        var user = await _userService.CreateUserAsync(command);

        // 发布事件
        await _eventBus.PublishAsync(new UserCreatedEvent(user.Id, user.Email));

        return Ok(user);
    }
}
```

### 2. 高级注入选项

对于需要自定义注册的服务，可以通过特性参数指定：

```csharp
// 指定服务注册为特定接口
[Injectable(Lifetime.Scoped, typeof(ICustomService))]
public class CustomServiceImpl : ICustomService, IAnotherInterface
{
    // 实现代码
}

// 多接口注册
[Injectable(Lifetime.Scoped, typeof(IServiceA), typeof(IServiceB))]
public class MultiService : IServiceA, IServiceB
{
    // 实现代码
}
```

## 最佳实践

### 1. 设计原则

- **依赖抽象而非具体实现**：始终依赖接口或抽象类
- **构造函数注入优先**：优先使用构造函数注入而非属性注入
- **避免循环依赖**：设计时注意服务间的依赖关系
- **合理选择生命周期**：根据服务特性选择适当的生命周期

### 2. 命名约定

- 接口名称以 `I` 开头
- 实现类名称与接口名称保持一致，去掉 `I` 前缀
- 处理器类名称以 `Handler` 结尾

### 3. 组织结构

```
Services/                 # 业务服务
├── IUserService.cs      # 服务接口
├── UserService.cs       # 服务实现
Handlers/                # 命令/查询处理器
├── CreateUserHandler.cs
├── GetUserQueryHandler.cs
Infrastructure/          # 基础设施实现
├── IUserRepository.cs
├── SqlUserRepository.cs
```

## 故障排除

### 1. 服务未解析

确保：
- 类已标记 `[Injectable]` 特性（除非是自动识别的处理器）
- 服务接口和实现都在可扫描的程序集中
- 没有拼写错误的依赖

### 2. 循环依赖

解决方法：
- 重新设计服务依赖关系
- 使用延迟加载模式
- 考虑引入中介者模式

### 3. 生命周期问题

注意事项：
- 不要在单例服务中注入作用域服务
- 确保生命周期选择符合业务需求

## 扩展性

框架支持自定义扩展：

1. **自定义特性**：可以扩展现有的注入特性
2. **条件注册**：支持基于条件的服务注册
3. **第三方集成**：可以集成其他DI容器或框架

通过这种自动化的依赖注入机制，ModularGodot.Core 框架大大简化了服务配置和管理，让开发者能够专注于业务逻辑的实现。