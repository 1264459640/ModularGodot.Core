using Xunit;
using ModularGodot.Core.Contracts.Abstractions.Services;

namespace ModularGodot.Core.XUnitTests.DependencyInjection
{
    /// <summary>
    /// These tests replicate the original integration tests from the ModularGodot.Core.Test project
    /// but using the xUnit testing framework for better reporting and automation.
    /// </summary>
    public class OriginalIntegrationTests
    {
        [Fact]
        public void OriginalTestServiceTest_ShouldResolveAndPrintMessage()
        {
            // Arrange
            var service1 = Contexts.Contexts.Instance.ResolveService<ITestService>();

            // Act
            var result = service1.PrintMessage("Hello World!");

            // Assert
            Assert.NotNull(service1);
            Assert.Equal("已同步打印: Hello World!", result);
        }

        [Fact]
        public void OriginalDITest_ShouldResolveAndRun()
        {
            // Arrange
            var service = Contexts.Contexts.Instance.ResolveService<IDITest>();

            // Act & Assert
            Assert.NotNull(service);
            // Note: The original test just called service.Run() which prints to console
            // In a unit test context, we're verifying that the service can be resolved
            // and that calling Run() doesn't throw an exception
            service.Run(); // This should not throw
        }
    }
}