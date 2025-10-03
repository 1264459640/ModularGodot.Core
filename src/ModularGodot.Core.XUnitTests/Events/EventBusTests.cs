using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ModularGodot.Contracts.Abstractions.Messaging;
using ModularGodot.Core.XUnitTests.Events;

namespace ModularGodot.Core.XUnitTests.Events
{
    public class EventBusTests : TestBase
    {
        [Fact]
        public void EventBus_PublishAndSubscribe_ShouldReceiveEvent()
        {
            // Arrange
            var eventBus = ResolveService<IEventBus>();
            var receivedEvent = false;
            var receivedMessage = string.Empty;

            // Act
            using var subscription = eventBus.Subscribe<TestEvent>(evt =>
            {
                receivedEvent = true;
                receivedMessage = evt.Message;
            });

            var testEvent = new TestEvent("Hello World", 42);
            eventBus.Publish(testEvent);

            // Assert
            Assert.True(receivedEvent);
            Assert.Equal("Hello World", receivedMessage);
        }

        [Fact]
        public async Task EventBus_PublishAsyncAndSubscribe_ShouldReceiveEvent()
        {
            // Arrange
            var eventBus = ResolveService<IEventBus>();
            var receivedEvent = false;
            var receivedValue = 0;

            // Act
            using var subscription = eventBus.Subscribe<TestEvent>(evt =>
            {
                receivedEvent = true;
                receivedValue = evt.Value;
            });

            var testEvent = new TestEvent("Async Test", 100);
            await eventBus.PublishAsync(testEvent);

            // 等待异步处理完成
            await Task.Delay(100);

            // Assert
            Assert.True(receivedEvent);
            Assert.Equal(100, receivedValue);
        }

        [Fact]
        public void EventBus_SubscribeOnce_ShouldReceiveEventOnlyOnce()
        {
            // Arrange
            var eventBus = ResolveService<IEventBus>();
            var callCount = 0;

            // Act
            using var subscription = eventBus.SubscribeOnce<TestEvent>(evt =>
            {
                callCount++;
            });

            var testEvent1 = new TestEvent("First", 1);
            var testEvent2 = new TestEvent("Second", 2);

            eventBus.Publish(testEvent1);
            eventBus.Publish(testEvent2);

            // Assert
            Assert.Equal(1, callCount);
        }

        [Fact]
        public void EventBus_ConditionalSubscribe_ShouldReceiveFilteredEvents()
        {
            // Arrange
            var eventBus = ResolveService<IEventBus>();
            var receivedEvents = 0;

            // Act
            using var subscription = eventBus.Subscribe<TestEvent>(
                evt => evt.Value > 50,  // 只接收Value大于50的事件
                evt => receivedEvents++
            );

            var testEvent1 = new TestEvent("Low Value", 25);
            var testEvent2 = new TestEvent("High Value", 75);
            var testEvent3 = new TestEvent("High Value 2", 100);

            eventBus.Publish(testEvent1);  // 不应该被接收
            eventBus.Publish(testEvent2);  // 应该被接收
            eventBus.Publish(testEvent3);  // 应该被接收

            // Assert
            Assert.Equal(2, receivedEvents);
        }

        [Fact]
        public async Task EventBus_AsyncSubscribe_ShouldHandleAsyncProcessing()
        {
            // Arrange
            var eventBus = ResolveService<IEventBus>();
            var receivedEvent = false;
            var processedValue = 0;

            // Act
            using var subscription = eventBus.Subscribe<TestEvent>(async (evt, ct) =>
            {
                // 模拟异步处理
                await Task.Delay(50, ct);
                processedValue = evt.Value * 2;
                receivedEvent = true;
            });

            var testEvent = new TestEvent("Async Processing", 21);
            eventBus.Publish(testEvent);

            // 等待异步处理完成
            await Task.Delay(100);

            // Assert
            Assert.True(receivedEvent);
            Assert.Equal(42, processedValue);
        }

        [Fact]
        public void EventBus_MultipleSubscribers_ShouldNotifyAllSubscribers()
        {
            // Arrange
            var eventBus = ResolveService<IEventBus>();
            var subscriber1Received = false;
            var subscriber2Received = false;

            // Act
            using var subscription1 = eventBus.Subscribe<TestEvent>(evt => subscriber1Received = true);
            using var subscription2 = eventBus.Subscribe<TestEvent>(evt => subscriber2Received = true);

            var testEvent = new TestEvent("Multi Subscriber", 123);
            eventBus.Publish(testEvent);

            // Assert
            Assert.True(subscriber1Received);
            Assert.True(subscriber2Received);
        }

        [Fact]
        public void EventBus_SubscriptionDisposal_ShouldStopReceivingEvents()
        {
            // Arrange
            var eventBus = ResolveService<IEventBus>();
            var receivedEvents = 0;

            // Act
            var subscription = eventBus.Subscribe<TestEvent>(evt => receivedEvents++);

            var testEvent1 = new TestEvent("Before Disposal", 1);
            eventBus.Publish(testEvent1);

            subscription.Dispose();  // 取消订阅

            var testEvent2 = new TestEvent("After Disposal", 2);
            eventBus.Publish(testEvent2);

            // Assert
            Assert.Equal(1, receivedEvents);  // 只有第一个事件被接收
        }

        [Fact]
        public void EventBus_SubscriptionCancellation_ShouldStopReceivingEvents()
        {
            // Arrange
            var eventBus = ResolveService<IEventBus>();
            var receivedEvents = 0;

            // Act
            var cancellationTokenSource = new CancellationTokenSource();
            using var subscription = eventBus.Subscribe<TestEvent>(async (evt, ct) =>
            {
                if (!ct.IsCancellationRequested)
                {
                    receivedEvents++;
                }
                await Task.CompletedTask;
            }, cancellationTokenSource.Token);

            var testEvent1 = new TestEvent("Before Cancellation", 1);
            eventBus.Publish(testEvent1);

            cancellationTokenSource.Cancel();  // 取消令牌

            var testEvent2 = new TestEvent("After Cancellation", 2);
            eventBus.Publish(testEvent2);

            // 等待异步处理完成
            Task.Delay(50).Wait();

            // Assert
            Assert.Equal(1, receivedEvents);  // 只有第一个事件被接收
        }
    }
}