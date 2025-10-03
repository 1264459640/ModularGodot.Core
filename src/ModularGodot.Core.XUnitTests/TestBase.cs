using System;

namespace ModularGodot.Core.XUnitTests
{
    public class TestBase : IDisposable
    {
        protected global::ModularGodot.Contexts.Contexts TestContext { get; private set; }
        private bool _disposed;

        public TestBase()
        {
            TestContext = global::ModularGodot.Contexts.Contexts.Instance;
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

        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TestBase));
        }

        public virtual void Dispose()
        {
            if (!_disposed)
            {
                // Note: We don't dispose the Context here as it's a singleton
                // and other tests may still need it
                _disposed = true;
            }
        }
    }
}