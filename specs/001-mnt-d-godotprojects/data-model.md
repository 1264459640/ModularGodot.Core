# Data Model: Mediator Pattern Adapter

## Entities

### Command
**Description**: A request that represents an action to be performed, may change system state
**Fields**:
- Message: string (example field from test command)

### Query
**Description**: A request that represents a query for data, should not change system state
**Fields**:
- Number: int (example field from test query)

### Handler
**Description**: A component that processes commands or queries and returns a response
**Fields**:
- HandleMethod: Task<TResponse> (method that processes the request)

### Dispatcher
**Description**: The mediator component that routes requests to appropriate handlers
**Fields**:
- SendCommand: Task<TResponse> (method to send commands)
- SendQuery: Task<TResponse> (method to send queries)

## Relationships

1. **Command** → **CommandHandler** (1:1)
   - Each command has exactly one corresponding handler
   - Handler implements ICommandHandler<TCommand, TResponse>

2. **Query** → **QueryHandler** (1:1)
   - Each query has exactly one corresponding handler
   - Handler implements IQueryHandler<TQuery, TResponse>

3. **Dispatcher** → **Command/Query** (1:many)
   - Dispatcher routes multiple commands and queries to their handlers

## Validation Rules

1. **Type Safety**: All command and query routing must maintain compile-time type safety
2. **Handler Registration**: Every command and query must have a registered handler
3. **Cancellation Support**: All operations must properly support cancellation tokens
4. **Exception Propagation**: Exceptions from handlers must propagate to callers

## State Transitions

N/A - This is a stateless mediator pattern implementation

## Contracts

### Interface Contracts

1. **IDispatcher**
   ```csharp
   Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
   Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
   ```

2. **ICommand<TResponse>**
   ```csharp
   // Marker interface for commands
   ```

3. **IQuery<TResponse>**
   ```csharp
   // Marker interface for queries
   ```

4. **ICommandHandler<TCommand, TResponse>**
   ```csharp
   Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken);
   ```

5. **IQueryHandler<TQuery, TResponse>**
   ```csharp
   Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
   ```