using Xunit;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ModularGodot.Contexts;
using ModularGodot.Contracts.Abstractions.Logging;
using ModularGodot.Contracts.Abstractions.Messaging;
using ModularGodot.Contracts.Attributes;

namespace ModularGodot.Core.Tests
{
    [Injectable]
    public class TestCommandHandler : ICommandHandler<TestCommand, Unit>
    {
        private readonly IGameLogger _logger;

        public TestCommandHandler(IGameLogger logger)
        {
            _logger = logger;
        }

        public Task<Unit> Handle(TestCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TestCommand handled with message: {command.Message}");
            Assert.Equal("Hello from Mediator!", command.Message);
            return Task.FromResult(Unit.Value);
        }
    }

    public class TestCommand : ICommand<Unit>
    {
        public string Message { get; set; }
    }

    public class DependencyInjectionTests
    {
        [Fact]
        public async Task TestDependencyInjection()
        {
            try
            {
                var contexts = new ModularGodot.Contexts.Contexts();

                var logger = contexts.ResolveService<IGameLogger>();
                Assert.NotNull(logger);
                logger.LogInformation("Logger resolved successfully.");

                var dispatcher = contexts.ResolveService<IDispatcher>();
                Assert.NotNull(dispatcher);
                await dispatcher.Send(new TestCommand { Message = "Hello from Dispatcher!" });

                logger.LogInformation("Test finished.");
            }
            catch (System.Exception ex)
            {
                Assert.True(false, ex.ToString());
            }
        }
    }
}
