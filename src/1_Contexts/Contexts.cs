using Autofac;
using ModularGodot.Contracts;

namespace ModularGodot.Contexts
{
    public class Contexts : LazySingleton<Contexts>,IDisposable
    {
        private readonly IContainer _container;

        public Contexts()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<SingleModule>();
            builder.RegisterModule<MediatorModule>();

            _container = builder.Build();
        }

        public T ResolveService<T>() where T : class
        {
            return _container.Resolve<T>();
        }

        public bool TryResolveService<T>(out T service) where T : class
        {
            return _container.TryResolve(out service);
        }

        public bool IsServiceRegistered<T>() where T : class
        {
            return _container.IsRegistered<T>();
        }

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}