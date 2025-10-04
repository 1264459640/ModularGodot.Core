using Xunit;
using ModularGodot.Core.Contracts.Abstractions.Messaging;

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