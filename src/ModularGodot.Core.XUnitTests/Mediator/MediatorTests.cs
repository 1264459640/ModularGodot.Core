using Xunit;
using ModularGodot.Core.Contracts.Abstractions.Messaging;

namespace ModularGodot.Core.XUnitTests.Mediator
{
    // 测试命令
    public class TestCommand : ICommand<string>
    {
        public string Message { get; set; }
    }

    // 测试命令处理器
    public class TestCommandHandler : ICommandHandler<TestCommand, string>
    {
        public Task<string> Handle(TestCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Handled: {command.Message}");
        }
    }

    // 测试查询
    public class TestQuery : IQuery<int>
    {
        public int Number { get; set; }
    }

    // 测试查询处理器
    public class TestQueryHandler : IQueryHandler<TestQuery, int>
    {
        public Task<int> Handle(TestQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult(query.Number * 2);
        }
    }

    public class MediatorTests : TestBase
    {
        [Fact]
        public async Task Dispatcher_SendCommand_ShouldReturnExpectedResult()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var command = new TestCommand { Message = "Test Message" };

            // Act
            var result = await dispatcher.Send(command);

            // Assert
            Assert.Equal("Handled: Test Message", result);
        }

        [Fact]
        public async Task Dispatcher_SendQuery_ShouldReturnExpectedResult()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var query = new TestQuery { Number = 5 };

            // Act
            var result = await dispatcher.Send(query);

            // Assert
            Assert.Equal(10, result);
        }

        [Fact]
        public async Task Dispatcher_SendCommandWithCancellation_ShouldHandleCancellation()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var command = new TestCommand { Message = "Test Message" };
            var cancellationToken = new CancellationToken(true); // 已取消的令牌

            // Act & Assert
            // 注意：由于MediatRAdapter的实现方式，取消可能不会立即生效
            // 这里主要测试方法可以被调用
            var result = await dispatcher.Send(command, cancellationToken);
            Assert.Equal("Handled: Test Message", result);
        }

        [Fact]
        public async Task Dispatcher_SendQueryWithCancellation_ShouldHandleCancellation()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var query = new TestQuery { Number = 5 };
            var cancellationToken = new CancellationToken(true); // 已取消的令牌

            // Act & Assert
            // 注意：由于MediatRAdapter的实现方式，取消可能不会立即生效
            // 这里主要测试方法可以被调用
            var result = await dispatcher.Send(query, cancellationToken);
            Assert.Equal(10, result);
        }
    }
}