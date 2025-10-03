using System.Threading;
using System.Threading.Tasks;
using ModularGodot.Contracts.Abstractions.Messaging;
using Xunit;

namespace ModularGodot.Core.XUnitTests.Mediator
{
    public class HandlerNotFoundTests : TestBase
    {
        [Fact]
        public async Task Dispatcher_SendCommand_WithoutRegisteredHandler_ShouldThrowHandlerNotFoundException()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var command = new UnregisteredCommand { Message = "No handler" };

            // Act & Assert
            await Assert.ThrowsAsync<HandlerNotFoundException>(() => dispatcher.Send(command));
        }

        [Fact]
        public async Task Dispatcher_SendQuery_WithoutRegisteredHandler_ShouldThrowHandlerNotFoundException()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var query = new UnregisteredQuery { Number = 123 };

            // Act & Assert
            await Assert.ThrowsAsync<HandlerNotFoundException>(() => dispatcher.Send(query));
        }
    }

    // Command without a registered handler
    public class UnregisteredCommand : ICommand<string>
    {
        public string Message { get; set; }
    }

    // Query without a registered handler
    public class UnregisteredQuery : IQuery<int>
    {
        public int Number { get; set; }
    }
}