using Xunit;
using ModularGodot.Core.Contracts.Abstractions.Services;

namespace ModularGodot.Core.XUnitTests.DependencyInjection
{
    public class AdvancedContainerTests : TestBase
    {
        [Fact]
        public void IsServiceRegistered_ShouldReturnTrue_ForRegisteredServices()
        {
            // Act
            var isRegistered = TestContext.IsServiceRegistered<ITestService>();

            // Assert
            Assert.True((bool)isRegistered);
        }

        [Fact]
        public void IsServiceRegistered_ShouldReturnFalse_ForNonRegisteredServices()
        {
            // Arrange
            // Using a service interface that we know is not registered
            // IDisposable is a standard .NET interface, not registered in our container

            // Act
            var isRegistered = TestContext.IsServiceRegistered<IDisposable>();

            // Assert
            Assert.False((bool)isRegistered);
        }

        [Fact]
        public void TryResolveService_ShouldReturnTrue_ForRegisteredServices()
        {
            // Arrange
            ITestService service;

            // Act
            var result = TryResolveService(out service);

            // Assert
            Assert.True(result);
            Assert.NotNull(service);
        }

        [Fact]
        public void TryResolveService_ShouldReturnFalse_ForNonRegisteredServices()
        {
            // Arrange
            IDisposable service;

            // Act
            var result = TryResolveService(out service);

            // Assert
            Assert.False(result);
            Assert.Null(service);
        }

    }
}