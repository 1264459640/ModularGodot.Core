using Xunit;
using ModularGodot.Contracts.Abstractions.Services;
using ModularGodot.Core.Test;
using ModularGodot.Infrastructure.Services;

namespace ModularGodot.Core.XUnitTests.DependencyInjection
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void DITest_ShouldHaveDependencyInjectedCorrectly()
        {
            // Arrange
            var diTest = global::ModularGodot.Contexts.Contexts.Instance.ResolveService<IDITest>();

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
            var testService = global::ModularGodot.Contexts.Contexts.Instance.ResolveService<ITestService>();

            // Assert
            Assert.NotNull(testService);
            Assert.IsType<TestService>(testService);
        }
    }
}