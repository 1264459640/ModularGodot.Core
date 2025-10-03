using System.Threading;
using System.Threading.Tasks;
using ModularGodot.Contracts.Abstractions.Messaging;
using Xunit;

namespace ModularGodot.Core.XUnitTests.Mediator.Contracts
{
    // Contract test for SendQuery functionality
    // This test should initially FAIL as it tests the contract before implementation
    public class SendQueryContractTest : TestBase
    {
        [Fact]
        public async Task SendQuery_ShouldRouteToCorrectHandler()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var query = new TestQuery { Number = 42 };

            // Act
            var result = await dispatcher.Send(query);

            // Assert
            Assert.Equal(84, result); // 42 * 2
        }

        [Fact]
        public async Task SendQuery_WithCancellation_ShouldRespectCancellation()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var query = new TestQuery { Number = 100 };
            var cancellationToken = new CancellationTokenSource(100); // 100ms timeout

            // Act & Assert
            // Should either complete successfully or be cancelled
            var result = await dispatcher.Send(query, cancellationToken.Token);
            Assert.Equal(200, result); // 100 * 2
        }

        [Fact]
        public async Task SendQuery_WithInvalidQuery_ShouldThrowHandlerNotFoundException()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var invalidQuery = new InvalidQuery { Number = -1 };

            // Act & Assert
            await Assert.ThrowsAsync<HandlerNotFoundException>(() => dispatcher.Send(invalidQuery));
        }
    }

    // Test query for contract validation
    public class TestQuery : IQuery<int>
    {
        public int Number { get; set; }
    }

    public class TestQueryHandler : IQueryHandler<TestQuery, int>
    {
        public Task<int> Handle(TestQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult(query.Number * 2);
        }
    }

    // Invalid query for testing error handling
    public class InvalidQuery : IQuery<int>
    {
        public int Number { get; set; }
    }
}