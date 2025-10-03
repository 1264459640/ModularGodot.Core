using System;
using System.Threading.Tasks;
using Xunit;
using ModularGodot.Contracts.Abstractions.Services;

namespace ModularGodot.Core.XUnitTests.DependencyInjection
{
    public class ServiceFunctionalityTests : TestBase
    {
        private readonly ITestService _testService;

        public ServiceFunctionalityTests()
        {
            _testService = ResolveService<ITestService>();
        }

        public new void Dispose()
        {
            // Cleanup if needed
            base.Dispose();
        }

        [Fact]
        public void PrintMessage_ShouldReturnExpectedResult()
        {
            // Arrange
            var message = "Test Message";

            // Act
            var result = _testService.PrintMessage(message);

            // Assert
            Assert.Equal($"已同步打印: {message}", result);
        }

        [Fact]
        public async Task PrintMessageAsync_ShouldReturnExpectedResult()
        {
            // Arrange
            var message = "Async Test Message";

            // Act
            var result = await _testService.PrintMessageAsync(message);

            // Assert
            Assert.Equal($"已异步打印: {message}", result);
        }
    }
}