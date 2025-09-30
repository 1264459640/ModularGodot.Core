using Autofac;
using ModularGodot.Contracts;

namespace ModularGodot.Contexts
{
    public class Contexts : IDisposable
    {
        private static readonly Lazy<IContainer> _containerLazy = new Lazy<IContainer>(() =>
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<SingleModule>();
            builder.RegisterModule<MediatorModule>();

            return builder.Build();
        });

        private readonly IContainer _container;

        public Contexts()
        {
            _container = _containerLazy.Value;
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
            // 不实际处置容器，因为它是静态的并在所有实例间共享
        }
    }
}