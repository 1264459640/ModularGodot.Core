# ModularGodot.Core 中介者模式使用指南

## 概述

ModularGodot.Core 实现了一个复杂的中介系统，该系统支持您基于插件的架构中组件之间的松散耦合。该系统使用 MediatR 作为高性能的消息中介，同时提供自己的抽象以为清晰的关注点分离。

中介系统包括：
- 自定义接口（ICommand、IQuery、ICommandHandler、IQueryHandler）不依赖于 MediatR
- 通过 MediatRAdapter 将自定义接口与强大的 MediatR 库桥接起来
- 用于类型安全中介的命令/查询包装器类
- 用于将处理器与 MediatR 桥接的处理器包装器类

## 核心概念

### 1. ICommand 和 IQuery
这些接口为创建解耦的命令和查询提供基础：

- `ICommand<TResponse>` - 用于更改应用程序状态的操作
- `IQuery<TResponse>` - 用于返回数据的读取操作

### 2. IDispatcher 接口
这是您与中介交互的主要接口：

```csharp
public interface IDispatcher
{
    Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
    Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
```

## 使用示例

### 创建命令

创建实现 `ICommand<T>` 的命令定义：

```csharp
public class CreateUserCommand : ICommand<UserDto>
{
    public string Name { get; }
    public string Email { get; }
    public string Password { get; }

    public CreateUserCommand(string name, string email, string password)
    {
        Name = name;
        Email = email;
        Password = password;
    }
}
```

### 创建命令处理器

创建实现 `ICommandHandler<TCommand, TResponse>` 的处理器：

```csharp
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IPasswordService passwordService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
    }

    public async Task<UserDto> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // 验证输入参数
        if (string.IsNullOrEmpty(command.Name))
            throw new ArgumentException("姓名是必需的", nameof(command.Name));

        // 密码哈希
        var hashedPassword = _passwordService.HashPassword(command.Password);

        // 创建用户
        var user = new User
        {
            Name = command.Name,
            Email = command.Email,
            PasswordHash = hashedPassword,
            CreatedAt = DateTime.UtcNow
        };

        // 保存到数据库
        var savedUser = await _userRepository.CreateAsync(user, cancellationToken);

        // 返回 DTO
        return new UserDto
        {
            Id = savedUser.Id,
            Name = savedUser.Name,
            Email = savedUser.Email,
            CreatedAt = savedUser.CreatedAt
        };
    }
}
```

### 创建查询

创建实现 `IQuery<T>` 的查询定义：

```csharp
public class GetUserByIdQuery : IQuery<UserDto?>
{
    public Guid UserId { get; }

    public GetUserByIdQuery(Guid userId)
    {
        UserId = userId;
    }
}
```

### 创建查询处理器

创建实现 `IQueryHandler<TQuery, TResponse>` 的处理器：

```csharp
public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(query.UserId, cancellationToken);

        if (user == null)
            return null;

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }
}
```

## 发送命令和查询

### 使用依赖注入

首先，注入 `IDispatcher` 接口：

```csharp
[Injectable(Lifetime.Scoped)]
public class UserManagementService
{
    private readonly IDispatcher _dispatcher;

    public UserManagementService(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task<UserDto> CreateUserAsync(string name, string email, string password, CancellationToken cancellationToken = default)
    {
        var command = new CreateUserCommand(name, email, password);
        return await _dispatcher.Send(command, cancellationToken);
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var query = new GetUserByIdQuery(userId);
        return await _dispatcher.Send(query, cancellationToken);
    }
}
```

### 在控制器或服务中使用

```csharp
[Injectable(Lifetime.Scoped)]
public class UsersController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public UsersController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var command = new CreateUserCommand(request.Name, request.Email, request.Password);

        try
        {
            var userDto = await _dispatcher.Send(command, cancellationToken);
            return Ok(userDto);
        }
        catch (Exception ex)
        {
            // 按需处理异常
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new GetUserByIdQuery(id);
        var user = await _dispatcher.Send(query, cancellationToken);

        if (user == null)
            return NotFound();

        return Ok(user);
    }
}
```

## 高级模式

### 使用取消令牌

中介系统完全支持合作取消的取消令牌：

```csharp
public async Task<UserDto> CreateUserWithTimeoutAsync(string name, string email, string password)
{
    using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
    var command = new CreateUserCommand(name, email, password);

    return await _dispatcher.Send(command, cts.Token);
}
```

### 错误处理

MediatR 适配器提供自定义的 `HandlerNotFoundException` 异常：

```csharp
public async Task<UserDto?> GetUserSafelyAsync(Guid userId)
{
    try
    {
        var query = new GetUserByIdQuery(userId);
        return await _dispatcher.Send(query);
    }
    catch (HandlerNotFoundException ex)
    {
        // 记录错误并进行相应处理
        _logger.LogError(ex, "未找到 GetUserByIdQuery 的处理器");
        return null; // 或返回默认值
    }
    catch (Exception ex)
    {
        // 处理其他潜在异常
        _logger.LogError(ex, "获取ID为 {UserId} 的用户时出错", userId);
        throw;
    }
}
```

## 处理器注册

处理器通过依赖注入系统使用 `[Injectable]` 特性自动注册：

```csharp
[Injectable(Lifetime.Scoped)]
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserDto>
{
    // 实现代码
}
```

ModularGodot.Core 中的依赖注入系统在无需额外配置的情况下会自动检测并注册命令和查询处理器。

## 此架构的优势

1. **分层隔离**：您的领域层不直接依赖 MediatR，提高了可维护性和可测试性。
2. **类型安全**：广泛使用泛型以确保编译时类型检查。
3. **性能**：经过优化的实现目标是 <1ms 的命令/查询路由时间。
4. **灵活性**：易于增强横切关注点，如日志记录、缓存等。
5. **可测试性**：易于模拟 `IDispatcher` 并为处理器编写单元测试。

## 最佳实践

1. **命令**：用于修改应用程序状态的操作；命令不应返回数据。
2. **查询**：用于读取数据的操作；不应修改应用程序状态。
3. **不可变性**：尽可能保持命令和查询对象不可变。
4. **验证**：在命令/查询创建时和处理器层都要添加验证。
5. **Async/Await**：在整个处理器中一致地使用 async/await。
6. **取消令牌**：始终将取消令牌传递到处理器中的异步操作。

## 故障排除

如果您遇到 `HandlerNotFoundException`，请确保：
1. 您的处理器类实现 `ICommandHandler<TCommand, TResponse>` 或 `IQueryHandler<TQuery, TResponse>`
2. 命令/查询与处理器之间的泛型类型完全匹配
3. 使用 `[Injectable]` 特性将处理器类注册到依赖注入容器中
4. 正确的程序集已注册到依赖注入容器中