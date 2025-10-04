using System.Diagnostics;
using Xunit;
using ModularGodot.Core.Contracts.Abstractions.Messaging;

namespace ModularGodot.Core.XUnitTests.Mediator
{
    public class PerformanceTests : TestBase
    {
        [Fact]
        public async Task Dispatcher_SendCommand_Performance_ShouldBeUnder2msMedian()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var command = new TestCommand { Message = "Performance Test" };

            // Warm up
            for (int i = 0; i < 10; i++)
            {
                await dispatcher.Send(command);
            }

            // Measure performance
            var stopwatch = new Stopwatch();
            var timings = new long[1000];

            for (int i = 0; i < timings.Length; i++)
            {
                stopwatch.Restart();
                await dispatcher.Send(command);
                stopwatch.Stop();
                timings[i] = stopwatch.ElapsedTicks;
            }

            // Calculate median
            Array.Sort(timings);
            var medianTicks = timings[timings.Length / 2];
            var medianMilliseconds = medianTicks / (Stopwatch.Frequency / 1000.0);

            // Assert - should be under 2ms median (more realistic for test environments)
            Assert.True(medianMilliseconds < 2.0,
                $"Median routing time was {medianMilliseconds:F4}ms, which is not under 2ms");
        }

        [Fact]
        public async Task Dispatcher_SendQuery_Performance_ShouldBeUnder2msMedian()
        {
            // Arrange
            var dispatcher = ResolveService<IDispatcher>();
            var query = new TestQuery { Number = 42 };

            // Warm up
            for (int i = 0; i < 10; i++)
            {
                await dispatcher.Send(query);
            }

            // Measure performance
            var stopwatch = new Stopwatch();
            var timings = new long[1000];

            for (int i = 0; i < timings.Length; i++)
            {
                stopwatch.Restart();
                await dispatcher.Send(query);
                stopwatch.Stop();
                timings[i] = stopwatch.ElapsedTicks;
            }

            // Calculate median
            Array.Sort(timings);
            var medianTicks = timings[timings.Length / 2];
            var medianMilliseconds = medianTicks / (Stopwatch.Frequency / 1000.0);

            // Assert - should be under 2ms median (more realistic for test environments)
            Assert.True(medianMilliseconds < 2.0,
                $"Median routing time was {medianMilliseconds:F4}ms, which is not under 2ms");
        }
    }
}