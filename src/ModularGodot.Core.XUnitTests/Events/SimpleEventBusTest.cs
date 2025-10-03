using System;
using Xunit;
using ModularGodot.Contracts.Abstractions.Messaging;
using ModularGodot.Core.XUnitTests.Events;

namespace ModularGodot.Core.XUnitTests.Events
{
    public class SimpleEventBusTest : TestBase
    {
        [Fact]
        public void EventBus_ResolveService_ShouldNotBeNull()
        {
            // Arrange
            var eventBus = ResolveService<IEventBus>();

            // Assert
            Assert.NotNull(eventBus);
        }
    }
}