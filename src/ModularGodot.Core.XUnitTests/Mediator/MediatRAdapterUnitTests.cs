using MediatR;
using Moq;
using Xunit;
using ModularGodot.Core.Infrastructure.Messaging;

namespace ModularGodot.Core.XUnitTests.Mediator
{
    public class MediatRAdapterUnitTests
    {
        [Fact]
        public async Task SendCommand_WithValidCommand_ShouldCallMediatorSend()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>();
            var adapter = new MediatRAdapter(mockMediator.Object);
            var command = new TestCommand { Message = "Test" };
            var expectedResponse = "Response";

            mockMediator
                .Setup(m => m.Send(It.IsAny<IRequest<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await adapter.Send(command);

            // Assert
            Assert.Equal(expectedResponse, result);
            mockMediator.Verify(m => m.Send(It.IsAny<IRequest<string>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendQuery_WithValidQuery_ShouldCallMediatorSend()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>();
            var adapter = new MediatRAdapter(mockMediator.Object);
            var query = new TestQuery { Number = 42 };
            var expectedResponse = 84;

            mockMediator
                .Setup(m => m.Send(It.IsAny<IRequest<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await adapter.Send(query);

            // Assert
            Assert.Equal(expectedResponse, result);
            mockMediator.Verify(m => m.Send(It.IsAny<IRequest<int>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendCommand_WithCancellation_ShouldPassCancellationToken()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>();
            var adapter = new MediatRAdapter(mockMediator.Object);
            var command = new TestCommand { Message = "Test" };
            var cancellationToken = new CancellationTokenSource().Token;
            var expectedResponse = "Response";

            mockMediator
                .Setup(m => m.Send(It.IsAny<IRequest<string>>(), cancellationToken))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await adapter.Send(command, cancellationToken);

            // Assert
            Assert.Equal(expectedResponse, result);
            mockMediator.Verify(m => m.Send(It.IsAny<IRequest<string>>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task SendQuery_WithCancellation_ShouldPassCancellationToken()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>();
            var adapter = new MediatRAdapter(mockMediator.Object);
            var query = new TestQuery { Number = 42 };
            var cancellationToken = new CancellationTokenSource().Token;
            var expectedResponse = 84;

            mockMediator
                .Setup(m => m.Send(It.IsAny<IRequest<int>>(), cancellationToken))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await adapter.Send(query, cancellationToken);

            // Assert
            Assert.Equal(expectedResponse, result);
            mockMediator.Verify(m => m.Send(It.IsAny<IRequest<int>>(), cancellationToken), Times.Once);
        }
    }
}