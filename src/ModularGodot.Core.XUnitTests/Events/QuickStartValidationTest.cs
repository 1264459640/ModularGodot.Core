using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ModularGodot.Core.Contracts.Abstractions.Messaging;
using ModularGodot.Core.Contracts.Abstractions.Logging;
using ModularGodot.Core.Infrastructure.Messaging;
using Moq;
using Xunit;

namespace ModularGodot.Core.XUnitTests.Events
{
    public class QuickStartValidationTest
    {
        private readonly IEventBus _eventBus;
        private readonly Mock<IGameLogger> _loggerMock;

        public QuickStartValidationTest()
        {
            _loggerMock = new Mock<IGameLogger>();
            _eventBus = new R3EventBus(_loggerMock.Object);
        }

        [Fact]
        public async Task QuickStart_ExampleShouldWork()
        {
            // Test the basic functionality using IEventBus interface
            var receivedEvents = 0;
            string receivedEventId = string.Empty;

            // Subscribe to an event using the IEventBus interface (Returns IDisposable)
            Action<TestQuickEvent> handler = (e) =>
            {
                Interlocked.Increment(ref receivedEvents);
                Interlocked.Exchange(ref receivedEventId, e.EventId);
            };

            IDisposable subscription = _eventBus.Subscribe<TestQuickEvent>(handler);

            Assert.NotNull(subscription);

            // Use a derived test class that allows us to set the Source as public
            var gameEvent = new TestQuickEvent()
            {
                Data = "Sample data"
            };

            // Publish the event
            await _eventBus.PublishAsync(gameEvent, CancellationToken.None);

            // Wait for the event to be processed
            await Task.Delay(50);

            // Verify the event was received
            Assert.Equal(1, Thread.VolatileRead(ref receivedEvents));
            Assert.NotEmpty(receivedEventId);

            subscription.Dispose(); // Proper cleanup
        }

        [Fact]
        public async Task OneTimeSubscription_AutoDisposesAfterFirstEvent()
        {
            // Test that the one-time subscription works with the IEventBus interface pattern
            var receivedEvents = 0;
            string firstEventId = string.Empty;

            Action<TestQuickEvent> handler = (e) =>
            {
                Interlocked.Increment(ref receivedEvents);
                Interlocked.Exchange(ref firstEventId, e.EventId);
            };

            IDisposable subscription = _eventBus.SubscribeOnce<TestQuickEvent>(handler);

            // Publish first event (constructor automatically sets EventId, Timestamp, Source)
            var firstEvent = new TestQuickEvent()
            {
                Data = "First event"
            };

            await _eventBus.PublishAsync(firstEvent, CancellationToken.None);
            await Task.Delay(20); // Give it more time for processing

            Assert.Equal(1, Thread.VolatileRead(ref receivedEvents));
            Assert.NotEmpty(firstEventId);

            // Publish second event - should not be received since subscription auto-disposed
            var secondEvent = new TestQuickEvent()
            {
                Data = "Second event - should not be received"
            };

            await _eventBus.PublishAsync(secondEvent, CancellationToken.None);
            await Task.Delay(20);

            // Since this was a one-time subscription in implementation, it should only receive one event
            Assert.Equal(1, Thread.VolatileRead(ref receivedEvents));
        }

        [Fact]
        public async Task ConcurrentOperations_ThreadSafetyTest()
        {
            // Test concurrent operations as per quickstart example using interface approach
            var receivedEventCount = 0;

            Action<TestQuickEvent> handler = (e) =>
            {
                Interlocked.Increment(ref receivedEventCount);
            };

            IDisposable subscription = _eventBus.Subscribe<TestQuickEvent>(handler);

            var tasks = new List<Task>();

            // Multiple tasks publishing concurrently
            for (int i = 0; i < 5; i++)
            {
                int eventId = i;
                tasks.Add(Task.Run(async () =>
                {
                    await _eventBus.PublishAsync(new TestQuickEvent()
                    {
                        Data = $"Event from task {eventId}",
                    }, CancellationToken.None);
                }));
            }

            await Task.WhenAll(tasks);
            await Task.Delay(50); // Allow events to be processed

            Assert.Equal(5, Thread.VolatileRead(ref receivedEventCount));

            subscription.Dispose();
        }

        [Fact]
        public async Task EventValidation_SupportsValidEvents()
        {
            var receivedEvents = 0;

            Action<TestValidEvent> handler = (e) =>
            {
                Interlocked.Increment(ref receivedEvents);
                // Already validated by interface: EventBase has automatic ID, timestamp, etc.
            };

            IDisposable subscription = _eventBus.Subscribe<TestValidEvent>(handler);

            // Publish an event
            var gameEvent = new TestValidEvent();

            await _eventBus.PublishAsync(gameEvent, CancellationToken.None);
            await Task.Delay(10);

            // should be received since it's valid
            Assert.Equal(1, Thread.VolatileRead(ref receivedEvents));

            subscription.Dispose();
        }

        // Event classes for testing - customize constructors to allow specific Source
        public class TestEventBase : EventBase
        {
            public TestEventBase(string source = "TestEvent") : base(source) { }
        }

        public class TestQuickEvent : TestEventBase
        {
            public string Data { get; set; }

            public TestQuickEvent(string source = "TestQuickEvent") : base(source) { }
        }

        public class TestValidEvent : TestEventBase
        {
            public TestValidEvent(string source = "TestValidEvent") : base(source) { }
        }
    }
}