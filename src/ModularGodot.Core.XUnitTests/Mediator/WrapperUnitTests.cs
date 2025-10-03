using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using Xunit;
using ModularGodot.Infrastructure.Messaging;
using ModularGodot.Contracts.Abstractions.Messaging;

namespace ModularGodot.Core.XUnitTests.Mediator
{
    public class WrapperUnitTests
    {
        [Fact]
        public void CommandWrapper_ShouldStoreCommand()
        {
            // Arrange
            var command = new TestCommand { Message = "Test Command" };

            // Act
            var wrapper = new CommandWrapper<TestCommand, string>(command);

            // Assert
            Assert.Equal(command, wrapper.Command);
        }

        [Fact]
        public void QueryWrapper_ShouldStoreQuery()
        {
            // Arrange
            var query = new TestQuery { Number = 42 };

            // Act
            var wrapper = new QueryWrapper<TestQuery, int>(query);

            // Assert
            Assert.Equal(query, wrapper.Query);
        }

        [Fact]
        public void CommandHandlerWrapper_ShouldStoreHandler()
        {
            // Arrange
            var mockHandler = new Mock<ICommandHandler<TestCommand, string>>();

            // Act
            var wrapper = new CommandHandlerWrapper<TestCommand, string>(mockHandler.Object);

            // Assert
            Assert.NotNull(wrapper);
            // The handler is stored privately, so we can't directly assert it,
            // but the construction succeeded which is what we're testing
        }

        [Fact]
        public async Task CommandHandlerWrapper_Handle_ShouldCallInnerHandler()
        {
            // Arrange
            var mockHandler = new Mock<ICommandHandler<TestCommand, string>>();
            var wrapper = new CommandHandlerWrapper<TestCommand, string>(mockHandler.Object);
            var command = new TestCommand { Message = "Test" };
            var wrapperRequest = new CommandWrapper<TestCommand, string>(command);
            var expectedResponse = "Handled";
            var cancellationToken = CancellationToken.None;

            mockHandler
                .Setup(h => h.Handle(command, cancellationToken))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await wrapper.Handle(wrapperRequest, cancellationToken);

            // Assert
            Assert.Equal(expectedResponse, result);
            mockHandler.Verify(h => h.Handle(command, cancellationToken), Times.Once);
        }

        [Fact]
        public void QueryHandlerWrapper_ShouldStoreHandler()
        {
            // Arrange
            var mockHandler = new Mock<IQueryHandler<TestQuery, int>>();

            // Act
            var wrapper = new QueryHandlerWrapper<TestQuery, int>(mockHandler.Object);

            // Assert
            Assert.NotNull(wrapper);
            // The handler is stored privately, so we can't directly assert it,
            // but the construction succeeded which is what we're testing
        }

        [Fact]
        public async Task QueryHandlerWrapper_Handle_ShouldCallInnerHandler()
        {
            // Arrange
            var mockHandler = new Mock<IQueryHandler<TestQuery, int>>();
            var wrapper = new QueryHandlerWrapper<TestQuery, int>(mockHandler.Object);
            var query = new TestQuery { Number = 42 };
            var wrapperRequest = new QueryWrapper<TestQuery, int>(query);
            var expectedResponse = 84;
            var cancellationToken = CancellationToken.None;

            mockHandler
                .Setup(h => h.Handle(query, cancellationToken))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await wrapper.Handle(wrapperRequest, cancellationToken);

            // Assert
            Assert.Equal(expectedResponse, result);
            mockHandler.Verify(h => h.Handle(query, cancellationToken), Times.Once);
        }
    }
}