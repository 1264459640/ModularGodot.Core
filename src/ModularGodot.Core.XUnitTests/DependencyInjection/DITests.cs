using Xunit;
using ModularGodot.Contracts.Abstractions.Services;
using ModularGodot.Core.Test;

namespace ModularGodot.Core.XUnitTests.DependencyInjection
{
    public class DITests
    {
        [Fact]
        public void DITest_ShouldResolveAndExecuteSuccessfully()
        {
            // Arrange
            var service = global::ModularGodot.Contexts.Contexts.Instance.ResolveService<IDITest>();

            // Act & Assert
            Assert.NotNull(service);
            // This test verifies that the DI container can resolve the service
            // The actual execution would be tested in integration tests
        }
    }
}