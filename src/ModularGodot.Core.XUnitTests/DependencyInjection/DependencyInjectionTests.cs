using ModularGodot.Core.Contracts.Abstractions.Messaging;
using ModularGodot.Core.Contracts.Abstractions.Services;
using ModularGodot.Core.Infrastructure.Messaging;
using ModularGodot.Core.Infrastructure.Services;
using Xunit;


namespace ModularGodot.Core.XUnitTests.DependencyInjection
{
    public class DependencyInjectionTests : TestBase
    {

        [Fact]
        public void TestService_ShouldBeResolvedThroughContainer()
        {
            // Arrange & Act
            var testService = TestContext.ResolveService<ITestService>();

            // Assert
            Assert.NotNull(testService);
            Assert.IsType<TestService>(testService);
        }

        [Fact]
        public void R3EventBus_ShouldBeResolvedThroughContainer()
        {
            var testService = TestContext.ResolveService<IEventBus>();
            
            Assert.NotNull(testService);
            Assert.IsType<R3EventBus>(testService);
        }

    }
}