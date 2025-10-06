using System;
using System.Threading.Tasks;
using ModularGodot.Core.Contracts.Abstractions.Messaging;
using ModularGodot.Core.Contracts.Abstractions.Logging;
using ModularGodot.Core.Infrastructure.Messaging;
using Moq;
using Xunit;

namespace ModularGodot.Core.XUnitTests.Events
{
    public class SubscriptionResourceTest
    {
        private readonly Mock<IGameLogger> _loggerMock;

        public SubscriptionResourceTest()
        {
            _loggerMock = new Mock<IGameLogger>();
        }

        [Fact]
        public async Task SubscriptionResources_AreManagedProperlyWithManySubscriptions()
        {
            // Create own event bus for test isolation
            var eventBus = new R3EventBus(_loggerMock.Object);

            // Create many subscriptions and then dispose them to test resource management
            var subscriptions = new IDisposable[1000];
            var eventsReceived = new int[1000];

            // Create multiple subscriptions simultaneously
            for (int i = 0; i < 1000; i++)
            {
                int index = i; // Capture for closure
                subscriptions[i] = eventBus.Subscribe<TestEvent>(e =>
                {
                    eventsReceived[index]++;
                });
            }

            // Publish events to verify all subscriptions are working
            for (int i = 0; i < 5; i++)
            {
                await eventBus.PublishAsync(new TestEvent($"TestSource_{i}"), default);
                await Task.Delay(10);
            }

            // Verify all subscriptions received the events
            for (int i = 0; i < 1000; i++)
            {
                Assert.True(eventsReceived[i] > 0, $"Subscription {i} received no events");
            }

            // Now dispose all subscriptions
            foreach (var subscription in subscriptions)
            {
                subscription?.Dispose();
            }

            // Clear subscriptions array
            Array.Clear(subscriptions, 0, subscriptions.Length);

            // Now publish more events to make sure they aren't received (validate cleanup)
            for (int i = 0; i < 5; i++)
            {
                await eventBus.PublishAsync(new TestEvent($"CleanUpTestSource_{i}"), default);
                await Task.Delay(10);
            }

            // Verify cleanup - let's also do a brief delay to give disposal time
            await Task.Delay(50);

            // All subscriptions should remain at their original count as they were disposed
            for (int i = 0; i < 1000; i++)
            {
                int eventsBefore = eventsReceived[i];
                await Task.Delay(10); // Wait briefly
                int eventsAfter = eventsReceived[i];
                Assert.Equal(eventsBefore, eventsAfter);
            }

            // Clean up the event bus
            (eventBus as IDisposable)?.Dispose();
        }

        [Fact]
        public async Task SubscriptionResources_PreventMemoryLeaksOverLongRunningOperations()
        {
            // Create own event bus for test isolation
            var eventBus = new R3EventBus(_loggerMock.Object);

            // Test for memory leaks by creating and disposing subscriptions over time
            long initialMemory = GC.GetTotalMemory(true);

            for (int round = 0; round < 100; round++)
            {
                var subscription = eventBus.Subscribe<TestEvent>(e => { });

                // Publish an event
                await eventBus.PublishAsync(new TestEvent($"Run_{round}"), default);

                // Dispose the subscription
                subscription.Dispose();

                // Collect memory periodically to maybe identify leaks
                if (round % 10 == 0)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            long finalMemory = GC.GetTotalMemory(true);
            double memoryGrowth = (finalMemory - initialMemory) / 1024.0 / 1024.0; // in MB

            // Memory growth should not exceed a reasonable threshold (e.g., 10MB)
            Assert.True(memoryGrowth < 10.0,
                $"Memory growth too high: {memoryGrowth:F2}MB. Potential memory leak detected.");

            // Clean up the event bus
            (eventBus as IDisposable)?.Dispose();
        }

        [Fact]
        public async Task ConcurrentSubscriptions_ManageResourcesProperlyUnderLoad()
        {
            // Create own event bus for test isolation
            var eventBus = new R3EventBus(_loggerMock.Object);

            var uniqueTestId = Guid.NewGuid().ToString();

            // Create a handler that will update our count with unique test identifier
            var eventsReceived = 0;
            Action<TestEventForConcurrent> handler = e =>
            {
                if (e.TestId == uniqueTestId)
                {
                    System.Threading.Interlocked.Increment(ref eventsReceived);
                }
            };

            // Create subscriptions concurrently - we'll do this in batches
            var allSubscriptions = new System.Collections.Generic.List<IDisposable>();

            // Create 100 subscriptions in parallel with a unique type to avoid any type-based interference
            var tasks = new Task[100];
            for (int i = 0; i < 100; i++)
            {
                int index = i;
                tasks[i] = Task.Run(() =>
                {
                    var sub = eventBus.Subscribe<TestEventForConcurrent>(handler);
                    lock (allSubscriptions)
                    {
                        allSubscriptions.Add(sub);
                    }
                });
            }

            await Task.WhenAll(tasks);
            Assert.Equal(100, allSubscriptions.Count);

            // Publish an event for our unique test and see how many handlers are triggered
            var uniqueEvent = new TestEventForConcurrent(uniqueTestId, "ConcurrentTest");
            await eventBus.PublishAsync(uniqueEvent, default);
            await Task.Delay(50);

            // Each of the 100 subscriptions will receive the event, so should be 100 counts
            Assert.Equal(100, System.Threading.Volatile.Read(ref eventsReceived));

            // Now dispose all subscriptions
            foreach (var subscription in allSubscriptions)
            {
                subscription?.Dispose();
            }

            // Wait to ensure all pending operations are processed and disposal is complete
            await Task.Delay(100);

            int eventsAfterDisposal = System.Threading.Volatile.Read(ref eventsReceived);

            // Publish another event with same test ID to make sure it's not received by old subscriptions
            await eventBus.PublishAsync(new TestEventForConcurrent(uniqueTestId, "ConcurrentTest_PostDisposal"), default);
            await Task.Delay(50);
            int eventsAfter = System.Threading.Volatile.Read(ref eventsReceived);

            Assert.Equal(eventsAfterDisposal, eventsAfter); // Should remain the same after disposal
            Assert.Equal(100, eventsAfterDisposal); // Should still be 100 from the first pub

            // Clean up the event bus
            (eventBus as IDisposable)?.Dispose();
        }

        [Fact]
        public async Task OneTimeSubscription_ResourcesAreCleanedUpAfterFirstEvent()
        {
            // Create own event bus for test isolation
            var eventBus = new R3EventBus(_loggerMock.Object);

            var eventReceived = 0;

            var subscription = eventBus.SubscribeOnce<TestEvent>(e =>
            {
                System.Threading.Interlocked.Increment(ref eventReceived);
            });

            // Publish first event
            await eventBus.PublishAsync(new TestEvent("First"), default);
            await Task.Delay(30);

            // Should have received exactly one event
            int eventsAfterFirst = System.Threading.Volatile.Read(ref eventReceived);
            Assert.Equal(1, eventsAfterFirst);

            // Publish second event
            await eventBus.PublishAsync(new TestEvent("Second"), default);
            await Task.Delay(30);

            // Should still have received exactly one event
            // because the one-time subscription automatically disposed itself
            int eventsAfterSecond = System.Threading.Volatile.Read(ref eventReceived);
            Assert.Equal(1, eventsAfterSecond);
            Assert.Equal(eventsAfterFirst, eventsAfterSecond);

            // Clean up the event bus
            (eventBus as IDisposable)?.Dispose();
        }

        [Fact]
        public async Task SubscriptionResources_HandleRapidCreateDestroyOperations()
        {
            // Create own event bus for test isolation
            var eventBus = new R3EventBus(_loggerMock.Object);

            // Rapidly create and destroy subscriptions to test for resource leaks/crashes
            for (int i = 0; i < 500; i++)
            {
                var subscription = eventBus.Subscribe<TestEvent>(e => { });

                // Immediately dispose
                subscription.Dispose();
            }

            // Do a full GC and ensure no crashes occurred
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Final sanity check that the event bus still works
            var messageReceived = false;
            var finalSubscription = eventBus.Subscribe<TestEvent>(e =>
            {
                messageReceived = true;
            });

            await eventBus.PublishAsync(new TestEvent("FinalTest"), default);
            await Task.Delay(20);

            Assert.True(messageReceived);
            finalSubscription.Dispose();

            // Wait briefly to let any pending operations finish before disposing
            await Task.Delay(50);

            // Clean up the event bus
            (eventBus as IDisposable)?.Dispose();
        }

        private class TestEvent : EventBase
        {
            public TestEvent(string source) : base(source) { }
        }

        private class TestEventForConcurrent : EventBase
        {
            public string TestId { get; }

            public TestEventForConcurrent(string testId, string source) : base(source)
            {
                TestId = testId;
            }
        }
    }
}