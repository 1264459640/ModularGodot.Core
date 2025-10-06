using System;
using ModularGodot.Core.XUnitTests.TestInfrastructure;

namespace ModularGodot.Core.XUnitTests
{
    public class TestBase : IDisposable
    {
        protected TestContext TestContext { get; private set; }
        
        private bool _disposed;

        public TestBase()
        {
            TestContext = new TestContext();
        }

        protected T ResolveService<T>() where T : class
        {
            CheckDisposed();
            return TestContext.ResolveService<T>();
        }

        protected bool TryResolveService<T>(out T service) where T : class
        {
            CheckDisposed();
            return TestContext.TryResolveService(out service);
        }

        protected Mocks.MockGameLogger GetTestLogger()
        {
            CheckDisposed();
            return TestContext.GetMockLogger();
        }

        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TestBase));
        }

        public virtual void Dispose()
        {
            if (!_disposed)
            {
                TestContext?.Dispose();
                _disposed = true;
            }
        }
    }
}