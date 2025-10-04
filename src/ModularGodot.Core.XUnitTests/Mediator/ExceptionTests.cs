using ModularGodot.Core.Contracts.Abstractions.Messaging;
using Xunit;

namespace ModularGodot.Core.XUnitTests.Mediator
{
    public class ExceptionTests : TestBase
    {
        [Fact]
        public async Task Dispatcher_SendCommand_WithHandlerException_ShouldPropagateException()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var command = new ExceptionTestCommand { ShouldThrow = true };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => dispatcher.Send(command));
            Assert.Equal("Test exception from handler", exception.Message);
        }

        [Fact]
        public async Task Dispatcher_SendQuery_WithHandlerException_ShouldPropagateException()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var query = new ExceptionTestQuery { ShouldThrow = true };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => dispatcher.Send(query));
            Assert.Equal("Test exception from query handler", exception.Message);
        }
    }

    // Test command for exception testing
    public class ExceptionTestCommand : ICommand<string>
    {
        public bool ShouldThrow { get; set; }
    }

    public class ExceptionTestCommandHandler : ICommandHandler<ExceptionTestCommand, string>
    {
        public Task<string> Handle(ExceptionTestCommand command, CancellationToken cancellationToken)
        {
            if (command.ShouldThrow)
            {
                throw new InvalidOperationException("Test exception from handler");
            }
            return Task.FromResult("Success");
        }
    }

    // Test query for exception testing
    public class ExceptionTestQuery : IQuery<int>
    {
        public bool ShouldThrow { get; set; }
    }

    public class ExceptionTestQueryHandler : IQueryHandler<ExceptionTestQuery, int>
    {
        public Task<int> Handle(ExceptionTestQuery query, CancellationToken cancellationToken)
        {
            if (query.ShouldThrow)
            {
                throw new InvalidOperationException("Test exception from query handler");
            }
            return Task.FromResult(42);
        }
    }
}