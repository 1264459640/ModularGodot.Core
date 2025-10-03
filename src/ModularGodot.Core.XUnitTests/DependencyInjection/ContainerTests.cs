using Xunit;
using ModularGodot.Contracts.Abstractions.Services;

namespace ModularGodot.Core.XUnitTests.DependencyInjection
{
    public class ContainerTests : TestBase
    {
        [Fact]
        public void ResolveService_ShouldReturnValidServiceInstance()
        {
            // Arrange & Act
            var service = ResolveService<ITestService>();

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void ResolveService_ShouldReturnDifferentInstances_ForTransient()
        {
            // Arrange
            var service1 = ResolveService<ITestService>();

            // Act
            var service2 = ResolveService<ITestService>();

            // Assert
            Assert.NotSame(service1, service2);
        }
    }
}