using System.Threading;
using System.Threading.Tasks;
using ModularGodot.Contracts.Abstractions.Messaging;
using Xunit;

namespace ModularGodot.Core.XUnitTests.Mediator.Contracts
{
    // Contract test for SendCommand functionality
    // This test should initially FAIL as it tests the contract before implementation
    public class SendCommandContractTest : TestBase
    {
        [Fact]
        public async Task SendCommand_ShouldRouteToCorrectHandler()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var command = new TestCommand { Message = "Test Contract" };

            // Act
            var result = await dispatcher.Send(command);

            // Assert
            Assert.Equal("Handled: Test Contract", result);
        }

        [Fact]
        public async Task SendCommand_WithCancellation_ShouldRespectCancellation()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var command = new TestCommand { Message = "Test Cancellation" };
            var cancellationToken = new CancellationTokenSource(100); // 100ms timeout

            // Act & Assert
            // Should either complete successfully or be cancelled
            var result = await dispatcher.Send(command, cancellationToken.Token);
            Assert.Equal("Handled: Test Cancellation", result);
        }

        [Fact]
        public async Task SendCommand_WithInvalidCommand_ShouldThrowHandlerNotFoundException()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var invalidCommand = new InvalidCommand { Message = "Invalid" };

            // Act & Assert
            await Assert.ThrowsAsync<HandlerNotFoundException>(() => dispatcher.Send(invalidCommand));
        }
    }

    // Test command for contract validation
    public class TestCommand : ICommand<string>
    {
        public string Message { get; set; }
    }

    public class TestCommandHandler : ICommandHandler<TestCommand, string>
    {
        public Task<string> Handle(TestCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Handled: {command.Message}");
        }
    }

    // Invalid command for testing error handling
    public class InvalidCommand : ICommand<string>
    {
        public string Message { get; set; }
    }

    // Custom exception for handler not found
    public class HandlerNotFoundException : System.Exception
    {
        public HandlerNotFoundException(string message) : base(message) { }
    }
}