using Autofac;
using ModularGodot.Contracts;

namespace ModularGodot.Contexts
{
    public class Contexts : LazySingleton<Contexts>,IDisposable
    {
        private readonly IContainer _container;
        private bool _disposed;

        public Contexts()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<SingleModule>();
            builder.RegisterModule<MediatorModule>();
            _container = builder.Build();
        }

        public T ResolveService<T>() where T : class
        {
            CheckDisposed();
            return _container.Resolve<T>();
        }

        public bool TryResolveService<T>(out T service) where T : class
        {
            CheckDisposed();
            return _container.TryResolve(out service);
        }

        public bool IsServiceRegistered<T>() where T : class
        {
            CheckDisposed();
            return _container.IsRegistered<T>();
        }

        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Contexts));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _container?.Dispose();
                _disposed = true;
            }
        }
    }
}