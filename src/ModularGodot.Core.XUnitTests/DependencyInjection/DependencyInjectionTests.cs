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
        public void DITest_ShouldHaveDependencyInjectedCorrectly()
        {
            // Arrange
            var diTest = Contexts.Contexts.Instance.ResolveService<IDITest>();

            // Act
            // The DITest class has a constructor dependency on ITestService
            // If the container resolves it successfully, it means DI is working

            // Assert
            Assert.NotNull(diTest);
            // We can't directly access the private field, but we can verify
            // that the Run method works, which indicates the dependency was injected
            diTest.Run(); // Should not throw
        }

        [Fact]
        public void TestService_ShouldBeResolvedThroughContainer()
        {
            // Arrange & Act
            var testService = Contexts.Contexts.Instance.ResolveService<ITestService>();

            // Assert
            Assert.NotNull(testService);
            Assert.IsType<TestService>(testService);
        }

        [Fact]
        public void R3EventBus_ShouldBeResolvedThroughContainer()
        {
            var testService = Contexts.Contexts.Instance.ResolveService<IEventBus>();
            
            Assert.NotNull(testService);
            Assert.IsType<R3EventBus>(testService);
        }

        [Fact]
        public void R3EventBus_ShouldBeResolvedThroughTextContainer()
        {
            var testService = TestContext.ResolveService<IEventBus>();
            Assert.NotNull(testService);
            Assert.IsType<R3EventBus>(testService);
        }
    }
}