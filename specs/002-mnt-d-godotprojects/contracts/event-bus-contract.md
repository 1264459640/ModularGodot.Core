# Event Bus API Contract

## Overview
This contract defines the interface and behavior for the R3-based event bus system in ModularGodot.Core. The system handles publishing and subscribing to events while maintaining thread safety and memory efficiency.

## Interface Definition

### IEventBus Interface

```csharp
public interface IEventBus : IDisposable
{
    Task<string> Subscribe<T>(Func<T, Task> handler) where T : EventBase;
    Task<string> SubscribeOnce<T>(Func<T, Task> handler) where T : EventBase;
    Task Unsubscribe(string subscriptionId);
    Task Publish<T>(T @event) where T : EventBase;
}
```

## Method Specifications

### Subscribe<T>(Func<T, Task> handler)

**Description**: Subscribes to events of type T with an asynchronous handler.

**Parameters**:
- `handler`: Function to execute when event of type T is received

**Returns**:
- Unique string identifier for the subscription (GUID-based)

**Pre-conditions**:
- Handler must not be null
- Event type T must inherit from EventBase

**Post-conditions**:
- Handler will be called when events of type T are published
- Returns valid subscription ID that can be used for unsubscription

**Thread Safety**: Must be thread-safe for concurrent subscriptions

### SubscribeOnce<T>(Func<T, Task> handler)

**Description**: Subscribes to the first occurrence of events of type T, then automatically unsubscribes.

**Parameters**:
- `handler`: Function to execute when the first event of type T is received

**Returns**:
- Unique string identifier for the subscription (will auto-expire after first event)

**Pre-conditions**:
- Handler must not be null
- Event type T must inherit from EventBase

**Post-conditions**:
- Handler will be called exactly once when the first event of type T is published
- Subscription will be automatically cleaned up after first event
- No further events of type T will be handled by this subscription

### Publish<T>(T @event)

**Description**: Publishes an event of type T to all active subscribers.

**Parameters**:
- `event`: Event of type T to publish, must inherit from EventBase

**Returns**:
- Task that completes when event is published to all subscribers

**Pre-conditions**:
- Event must not be null
- Event must inherit from EventBase
- All EventBase validation rules must pass

**Post-conditions**:
- All active subscribers receive the event
- If event format is invalid, it's logged and not processed
- No unhandled exceptions should escape from event handlers

**Thread Safety**: Must be thread-safe for concurrent publishing

### Unsubscribe(string subscriptionId)

**Description**: Removes a subscription using its identifier.

**Parameters**:
- `subscriptionId`: Unique identifier returned when subscription was created

**Pre-conditions**:
- subscriptionId must be valid and currently active

**Post-conditions**:
- Handler matching the subscriptionId is removed
- Resources associated with the subscription are freed

## Data Contract

### EventBase (required base class)

```csharp
public abstract class EventBase
{
    public Guid EventId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Source { get; set; }
}
```

### Validation Rules
- EventId: Must be a valid, non-empty GUID
- Timestamp: Must be reasonable (not in far past/future)
- Source: Must be non-null, non-empty string

## Error Handling

### Expected Exceptions
- `EventBusDisposedException`: Thrown when operations are attempted on a disposed event bus
- `ArgumentException`: Thrown when invalid parameters are provided
- Format validation failures are logged as errors but don't throw exceptions

### Resource Management
- All subscriptions must be properly disposed when unsubscribed or on event bus disposal
- Memory usage must remain under 100MB during extended operation
- One-time subscriptions automatically dispose after first event

## Non-Functional Requirements

### Performance
- Support up to 1,000 events/second
- Memory usage under 100MB during sustained operation
- Thread-safe concurrent access

### Reliability
- No subscription leaks after unsubscription or disposal
- Proper cleanup of all resources
- Stable operation over extended periods

### Observability
- Basic logging for critical events
- Error logging without interrupting normal operation
- Minimal metrics collection for performance monitoring