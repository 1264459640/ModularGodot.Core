# Quickstart: Mediator Pattern Adapter

## Overview

This quickstart guide demonstrates how to use the Mediator Pattern Adapter in the ModularGodot.Core framework. The adapter provides a clean, decoupled way to handle commands and queries in your Godot game modules.

## Prerequisites

- ModularGodot.Core framework installed
- .NET 9.0 SDK
- Basic understanding of C# and Mediator pattern

## Quick Setup

1. **Define a Command**
```csharp
public class CreateUserCommand : ICommand<string>
{
    public string Username { get; set; }
    public string Email { get; set; }
}
```

2. **Create a Command Handler**
```csharp
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, string>
{
    public Task<string> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Implement your business logic here
        return Task.FromResult($"User {command.Username} created successfully");
    }
}
```

3. **Send the Command**
```csharp
// Get the dispatcher from your context
var dispatcher = Contexts.Instance.ResolveService<IDispatcher>();

// Create and send the command
var command = new CreateUserCommand
{
    Username = "john_doe",
    Email = "john@example.com"
};

var result = await dispatcher.Send(command);
Console.WriteLine(result); // "User john_doe created successfully"
```

## Quick Query Example

1. **Define a Query**
```csharp
public class GetUserQuery : IQuery<UserDto>
{
    public string UserId { get; set; }
}
```

2. **Create a Query Handler**
```csharp
public class GetUserQueryHandler : IQueryHandler<GetUserQuery, UserDto>
{
    public Task<UserDto> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        // Implement your query logic here
        var user = new UserDto
        {
            Id = query.UserId,
            Name = "John Doe"
        };
        return Task.FromResult(user);
    }
}
```

3. **Execute the Query**
```csharp
// Get the dispatcher
var dispatcher = Contexts.Instance.ResolveService<IDispatcher>();

// Create and send the query
var query = new GetUserQuery { UserId = "123" };
var user = await dispatcher.Send(query);
```

## Cancellation Support

The mediator adapter fully supports cancellation tokens:

```csharp
var cancellationToken = new CancellationTokenSource(5000).Token; // 5 second timeout
try
{
    var result = await dispatcher.Send(command, cancellationToken);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation was cancelled");
}
```

## Error Handling

Exceptions thrown by handlers are propagated to the caller:

```csharp
try
{
    var result = await dispatcher.Send(command);
}
catch (ValidationException ex)
{
    Console.WriteLine($"Validation error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}
```

## Dependency Injection

The MediatR adapter is automatically registered through the Contexts layer:

```csharp
// In your module configuration
public class MediatorModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // The adapter is automatically registered as Singleton
        // No additional configuration needed
    }
}
```

## Testing

Contract tests ensure the adapter works correctly:

```csharp
[Fact]
public async Task Dispatcher_SendCommand_ShouldReturnExpectedResult()
{
    // Arrange
    var dispatcher = ResolveService<IDispatcher>();
    var command = new TestCommand { Message = "Test" };

    // Act
    var result = await dispatcher.Send(command);

    // Assert
    Assert.Equal("Handled: Test", result);
}
```

## Performance

The adapter is optimized for low-latency message routing with a target of <1ms median routing time. For high-performance scenarios, consider:

1. Minimizing handler complexity
2. Using appropriate cancellation tokens
3. Handling exceptions efficiently

## Next Steps

1. Review the full API documentation in `/docs/ARCHITECTURE.md`
2. Check the contract tests in `/src/ModularGodot.Core.XUnitTests/Mediator/`
3. Explore advanced patterns in the example implementations