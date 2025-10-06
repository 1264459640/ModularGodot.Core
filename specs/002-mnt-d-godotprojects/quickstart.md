# Quickstart: Event Bus System

## Overview
This quickstart guide demonstrates how to use the fixed Event Bus System in ModularGodot.Core, focusing on the corrected memory leak and thread safety issues.

## Setup

### 1. Installation
The event bus system is part of ModularGodot.Core infrastructure:
```
// No separate installation needed - included with ModularGodot.Core.Infrastructure
```

### 2. Registration
The event bus will be automatically registered via the Autofac container:

```csharp
// In your context configuration
container.RegisterEventBus();
```

### 3. Basic Usage
Here's how to publish and subscribe to events using the corrected event bus:

```csharp
// Constructor injection of event bus
public MyEventHandlingService(IEventBus eventBus)
{
    this.eventBus = eventBus;
}

// Example: Subscribe to an event
public async Task SubscribeToEvents()
{
    // Subscribe with proper cleanup - using ID for unsubscribe
    var subscriptionId = await eventBus.Subscribe<MyGameEvent>(HandleGameEvent);
    Console.WriteLine($"Subscribed with ID: {subscriptionId}");
}

// Example: Publish an event
public async Task PublishExample()
{
    var gameEvent = new MyGameEvent
    {
        EventId = Guid.NewGuid(),
        Timestamp = DateTime.UtcNow,
        Source = "QuickStartExample",
        Data = "Sample data"
    };

    await eventBus.Publish(gameEvent);
}

// One-time subscription (auto-disposes after first event)
public async Task OneTimeSubscription()
{
    await eventBus.SubscribeOnce<MyGameStateChange>(async (state) => {
        Console.WriteLine($"Game state changed once to: {state.Status}");
        // This subscription auto-disposes after first event
    });
}

private async Task HandleGameEvent(MyGameEvent e)
{
    Console.WriteLine($"Received game event: {e.Data}");
    await Task.CompletedTask;
}
```

## Memory Management

### Safe Subscription Practices
The corrected event bus ensures proper resource cleanup:

```csharp
// GOOD - The event bus will properly clean up resources
var subId = await eventBus.Subscribe<MyEvent>(async (e) => {
    // Event handler processing
});

// When you're done, unsubscribe to free resources
eventBus.Unsubscribe(subId);

// Or background tasks auto-cleanup, especially for one-time events:
await eventBus.SubscribeOnce<MyStartupEvent>(async (e) => {
    // This will auto-dispose after first event is received
});
```

### Performance Monitoring
The corrected event bus includes memory pressure indicators:

```csharp
// Check if memory usage is within limits
// The event bus will automatically perform cleanup to stay under 100MB limit
// Use memory monitors for more detailed tracking:

var memoryUsage = MemoryMonitor.GetCurrentUsage();
if (memoryUsage > 80) // 80MB threshold
{
    Console.WriteLine("Memory pressure - event bus cleanup should have occurred");
}
```

## Threading and Concurrency

### Thread-Safe Operations
The corrected event bus handles concurrent operations safely:

```csharp
// Multiple threads can safely subscribe/publish simultaneously
public async Task ConcurrentExample()
{
    var tasks = new List<Task>();

    // Multiple threads can safely subscribe
    for (int i = 0; i < 10; i++)
    {
        int threadId = i; // Capture for closure
        tasks.Add(Task.Run(async () =>
        {
            await eventBus.Subscribe<TestEvent>(async (e) =>
            {
                await Task.Yield(); // Simulate async work
                Console.WriteLine($"Thread {threadId} handling event");
            });
        }));
    }

    await Task.WhenAll(tasks);

    // Multiple threads can safely publish
    for (int i = 0; i < 5; i++)
    {
        int eventId = i;
        tasks.Add(Task.Run(async () =>
        {
            await eventBus.Publish(new TestEvent
            {
                EventId = Guid.NewGuid(),
                Data = $"Event from thread {eventId}",
                Source = "ConcurrentPublisher"
            });
        }));
    }

    await Task.WhenAll(tasks);
}
```

## Event Validation
Events now include basic validation:

```csharp
// Valid events pass format validation
var validEvent = new TestEvent
{
    EventId = Guid.NewGuid(),
    Timestamp = DateTime.UtcNow,
    Source = "Validator",
    Data = "Valid event payload"
};
await eventBus.Publish(validEvent); // OK - passes validation

// Invalid events are logged, not processed
var invalidEvent = new TestEvent
{
    EventId = Guid.Empty // Invalid
    // ... other required fields
};
await eventBus.Publish(invalidEvent); // Logs error, continues operation
```

## Expected Output
When running the quickstart examples, you should see:
- No memory leaks even during extended operation (>24 hours)
- Proper handling of >1,000 events/second with memory usage <100MB
- Thread-safe operations under concurrent load
- One-time subscriptions automatically disposing after first event
- Proper error logging for invalid events without system interruption