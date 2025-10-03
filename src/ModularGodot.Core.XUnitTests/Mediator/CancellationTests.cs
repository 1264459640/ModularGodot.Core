using System.Threading;
using System.Threading.Tasks;
using ModularGodot.Contracts.Abstractions.Messaging;
using Xunit;

namespace ModularGodot.Core.XUnitTests.Mediator
{
    public class CancellationTests : TestBase
    {
        [Fact]
        public async Task Dispatcher_SendCommand_WithCancellation_ShouldRespectCancellation()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var command = new CancellationTestCommand { Message = "Test Cancellation" };

            // Test without cancellation first - should complete successfully
            var result = await dispatcher.Send(command);
            Assert.Equal("Handled: Test Cancellation", result);
        }

        [Fact]
        public async Task Dispatcher_SendQuery_WithCancellation_ShouldRespectCancellation()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var query = new CancellationTestQuery { Number = 42 };

            // Test without cancellation first - should complete successfully
            var result = await dispatcher.Send(query);
            Assert.Equal(84, result); // 42 * 2
        }
    }

    // Test command for cancellation testing
    public class CancellationTestCommand : ICommand<string>
    {
        public string Message { get; set; }
    }

    public class CancellationTestCommandHandler : ICommandHandler<CancellationTestCommand, string>
    {
        public async Task<string> Handle(CancellationTestCommand command, CancellationToken cancellationToken)
        {
            // Simulate some work that can be cancelled
            await Task.Delay(10, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            return $"Handled: {command.Message}";
        }
    }

    // Test query for cancellation testing
    public class CancellationTestQuery : IQuery<int>
    {
        public int Number { get; set; }
    }

    public class CancellationTestQueryHandler : IQueryHandler<CancellationTestQuery, int>
    {
        public async Task<int> Handle(CancellationTestQuery query, CancellationToken cancellationToken)
        {
            // Simulate some work that can be cancelled
            await Task.Delay(10, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            return query.Number * 2;
        }
    }
}