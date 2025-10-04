using ModularGodot.Core.Contracts.Abstractions.Services;
using ModularGodot.Core.Contracts.Attributes;

namespace ModularGodot.Core.XUnitTests.DependencyInjection
{
    public interface IDITest
    {
        void Run();
    }

    [Injectable(Lifetime.Singleton)]
    public class DITest : IDITest
    {
        private readonly ITestService _testService;
        public DITest(ITestService testService)
        {
            _testService = testService;
        }

        public void Run()
        {
            _testService.PrintMessage("DI Test");
        }
    }
}