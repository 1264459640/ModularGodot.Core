# Data Model: Event Bus System

## Core Entities

### Event Bus
- **Name**: R3EventBus
- **Fields**:
  - `subjects: ConcurrentDictionary<Type, object>` - Thread-safe registry of event subjects
  - `subscriptions: Dictionary<string, IDisposable>` - Active subscription handles with unique identifiers
  - `subscriptionsLock: ReaderWriterLockSlim` - Fine-grained lock for subscription management
- **Responsibilities**:
  - Manage event publishing and subscription lifecycle
  - Ensure thread-safe operations during high-frequency event processing
  - Track and clean up resources to maintain memory constraints
- **Validation Rules**:
  - Each subscription must have a unique identifier
  - All disposable resources must be properly disposed when event bus is disposed

### Event Subscription
- **Name**: EventSubscription
- **Fields**:
  - `subscriptionId: string` - Unique identifier for the subscription
  - `disposable: IDisposable` - Subscription resource to be disposed
  - `oneTime: bool` - Flag indicating if this is a one-time subscription
  - `eventType: Type` - The type of event being subscribed to
- **State Transitions**:
  - Active → Disposed (when manually cancelled or upon one-time execution)
- **Validation Rules**:
  - Disposables must implement proper disposal pattern

### Topic Subject
- **Name**: TopicSubject<T>
- **Fields**:
  - `subject: Subject<T>` - The R3 subject for the particular event type
  - `disposed: bool` - Flag indicating if the subject has been disposed
  - `lock: object` - Lock object for thread-safe access
- **Validation Rules**:
  - Cannot be used once disposed
  - Thread-safe publishing mechanisms required

## Event Definitions

### EventBase
- **Name**: EventBase (as defined in ModularGodot.Core.Contracts)
- **Fields**:
  - `EventId: Guid` - Unique identifier for event instance
  - `Timestamp: DateTime` - When event was created
  - `Source: string` - Origin component identifier

### Custom Events (for testing)
- **Name**: TestEvent
- **Fields**:
  - `Payload: string` - Test data payload
- **Usage**: For validation and testing the event bus functionality

## Interface Contracts

### IEventBus
- **Methods**:
  - Publish<T>(T event) - Publish an event of type T with thread-safe processing
  - Subscribe<T>(Func<T, Task> handler) - Subscribe to events of type T, returns subscription ID
  - SubscribeOnce<T>(Func<T, Task> handler) - Subscribe for only the first occurrence of type T, auto-disposes
  - Unsubscribe(string subscriptionId) - Unsubscribe using the identifier
  - Dispose() - Clean up all resources and subscriptions
- **Validation Rules**:
  - All operations must be thread-safe
  - Events must pass format validation before processing

## Data Validation

### Event Format Validation
- All incoming events must inherit from EventBase
- EventId must be a valid GUID
- Timestamp must be current (not way in the past/future)
- Source must be a valid string (not null or empty)

### Performance Constraints
- Memory usage under 100MB at all times
- Support up to 1,000 events per second
- Each operation keeps to <1ms when not under load