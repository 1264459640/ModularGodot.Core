using ModularGodot.Core.Contracts.Abstractions.Messaging;
using Xunit;

namespace ModularGodot.Core.XUnitTests.Mediator
{
    public class CancellationTests : TestBase
    {
        [Fact]
        public async Task Dispatcher_SendCommand_WithoutCancellation_ShouldCompleteSuccessfully()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var command = new CancellationTestCommand { Message = "Test Cancellation" };

            // Act
            var result = await dispatcher.Send(command);

            // Assert
            Assert.Equal("Handled: Test Cancellation", result);
        }

        [Fact]
        public async Task Dispatcher_SendCommand_WithCancellation_ShouldRespectCancellation()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var command = new CancellationTestCommand { Message = "Test Cancellation" };
            var cancellationTokenSource = new CancellationTokenSource();

            // Cancel immediately
            cancellationTokenSource.Cancel();

            // Act & Assert
            // TaskCanceledException is thrown when a task is cancelled, which is the expected behavior
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await dispatcher.Send(command, cancellationTokenSource.Token);
            });
        }

        [Fact]
        public async Task Dispatcher_SendCommand_WithCancellationDuringExecution_ShouldCancelOperation()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var command = new LongRunningCancellationTestCommand { Message = "Long Running Test" };
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

            // Act & Assert
            // TaskCanceledException is thrown when a task is cancelled, which is the expected behavior
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await dispatcher.Send(command, cancellationTokenSource.Token);
            });
        }

        [Fact]
        public async Task Dispatcher_SendQuery_WithoutCancellation_ShouldCompleteSuccessfully()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var query = new CancellationTestQuery { Number = 42 };

            // Act
            var result = await dispatcher.Send(query);

            // Assert
            Assert.Equal(84, result); // 42 * 2
        }

        [Fact]
        public async Task Dispatcher_SendQuery_WithCancellation_ShouldRespectCancellation()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var query = new CancellationTestQuery { Number = 42 };
            var cancellationTokenSource = new CancellationTokenSource();

            // Cancel immediately
            cancellationTokenSource.Cancel();

            // Act & Assert
            // TaskCanceledException is thrown when a task is cancelled, which is the expected behavior
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await dispatcher.Send(query, cancellationTokenSource.Token);
            });
        }

        [Fact]
        public async Task Dispatcher_SendQuery_WithCancellationDuringExecution_ShouldCancelOperation()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var query = new LongRunningCancellationTestQuery { Number = 42 };
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

            // Act & Assert
            // TaskCanceledException is thrown when a task is cancelled, which is the expected behavior
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await dispatcher.Send(query, cancellationTokenSource.Token);
            });
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
            await Task.Delay(100, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            return $"Handled: {command.Message}";
        }
    }

    // Long running command for testing cancellation during execution
    public class LongRunningCancellationTestCommand : ICommand<string>
    {
        public string Message { get; set; }
    }

    public class LongRunningCancellationTestCommandHandler : ICommandHandler<LongRunningCancellationTestCommand, string>
    {
        public async Task<string> Handle(LongRunningCancellationTestCommand command, CancellationToken cancellationToken)
        {
            // Simulate long running work that can be cancelled
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(100, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }
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
            await Task.Delay(100, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            return query.Number * 2;
        }
    }

    // Long running query for testing cancellation during execution
    public class LongRunningCancellationTestQuery : IQuery<int>
    {
        public int Number { get; set; }
    }

    public class LongRunningCancellationTestQueryHandler : IQueryHandler<LongRunningCancellationTestQuery, int>
    {
        public async Task<int> Handle(LongRunningCancellationTestQuery query, CancellationToken cancellationToken)
        {
            // Simulate long running work that can be cancelled
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(100, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }
            return query.Number * 2;
        }
    }
}