
using ModularGodot.Core.Contracts.Abstractions.Messaging;
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
            var command = new ContractTestCommand { Message = "Test Contract" };

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
            var command = new ContractTestCommand { Message = "Test Cancellation" };
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
            var invalidCommand = new InvalidContractCommand { Message = "Invalid" };

            // Act & Assert
            await Assert.ThrowsAsync<HandlerNotFoundException>(() => dispatcher.Send(invalidCommand));
        }
    }

    // Test command for contract validation
    public class ContractTestCommand : ICommand<string>
    {
        public string Message { get; set; }
    }

    public class ContractTestCommandHandler : ICommandHandler<ContractTestCommand, string>
    {
        public Task<string> Handle(ContractTestCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Handled: {command.Message}");
        }
    }

    // Invalid command for testing error handling
    public class InvalidContractCommand : ICommand<string>
    {
        public string Message { get; set; }
    }
}