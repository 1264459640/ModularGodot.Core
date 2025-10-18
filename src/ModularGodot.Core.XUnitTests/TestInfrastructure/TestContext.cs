using Autofac;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ModularGodot.Core.Contracts;
using ModularGodot.Core.Contracts.Abstractions.Logging;
using ModularGodot.Core.Contracts.Abstractions.Messaging;
using ModularGodot.Core.Contracts.Abstractions.Services;
using ModularGodot.Core.Infrastructure.Messaging;
using ModularGodot.Core.Infrastructure.Services;
using ModularGodot.Core.XUnitTests.DependencyInjection;
using ModularGodot.Core.XUnitTests.Mocks;
using System.Reflection;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;

namespace ModularGodot.Core.XUnitTests.TestInfrastructure
{
    /// <summary>
    /// Specialized context for testing that replaces dependencies with test-friendly implementations
    /// </summary>
    public class TestContext : IDisposable
    {
        private readonly IContainer _container;
        private bool _disposed;

        public TestContext()
        {
            var builder = new ContainerBuilder();

            // --- Manual registration replacing SingleModule ---

            // MemoryCache and related
            builder.RegisterInstance(Options.Create(new MemoryCacheOptions())).As<IOptions<MemoryCacheOptions>>().SingleInstance();
            builder.RegisterType<MemoryCache>().As<IMemoryCache>().SingleInstance();

            // Mock Logger for tests
            builder.RegisterType<MockGameLogger>().As<IGameLogger>().SingleInstance();
            
            // EventBus
            builder.RegisterType<R3EventBus>().As<IEventBus>().SingleInstance();

            // --- Manual registration replacing MediatorModule ---

            // Register our custom wrappers first
            builder.RegisterGeneric(typeof(CommandHandlerWrapper<,>)).As(typeof(IRequestHandler<,>)).InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(QueryHandlerWrapper<,>)).As(typeof(IRequestHandler<,>)).InstancePerLifetimeScope();

            // Register MediatR and its handlers from the current test assembly ONLY
            var testAssembly = Assembly.GetExecutingAssembly();
            
            builder.RegisterAssemblyTypes(testAssembly)
                .Where(t => t.IsClosedTypeOf(typeof(ICommandHandler<,>)) ||
                            t.IsClosedTypeOf(typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            var mediatrConfig = MediatRConfigurationBuilder
                .Create("",testAssembly)
                .WithAllOpenGenericHandlerTypesRegistered()
                .Build();
            builder.RegisterMediatR(mediatrConfig);

            // The IDispatcher is our own abstraction, implemented by MediatRAdapter
            builder.RegisterType<MediatRAdapter>().As<IDispatcher>().InstancePerLifetimeScope();

            // Manually register TestService for dependency injection tests
            builder.RegisterType<TestService>().As<ITestService>().InstancePerDependency();

            // Manually register DITest for dependency injection tests
            builder.RegisterType<DITest>().As<IDITest>().SingleInstance();

            _container = builder.Build();
        }

        /// <summary>
        /// 解析服务
        /// </summary>
        public T ResolveService<T>() where T : class
        {
            CheckDisposed();
            return _container.Resolve<T>();
        }

        /// <summary>
        /// 尝试解析服务
        /// </summary>
        public bool TryResolveService<T>(out T service) where T : class
        {
            CheckDisposed();
            return _container.TryResolve(out service);
        }

        /// <summary>
        /// 获取 Mock 日志器
        /// </summary>
        public MockGameLogger GetMockLogger()
        {
            CheckDisposed();
            return _container.Resolve<MockGameLogger>();
        }

        /// <summary>
        /// 检查服务是否已注册
        /// </summary>
        public bool IsServiceRegistered<T>() where T : class
        {
            CheckDisposed();
            return _container.IsRegistered<T>();
        }

        /// <summary>
        /// 检查是否已被处理
        /// </summary>
        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TestContext));
        }

        /// <summary>
        /// 释放资源
        /// </summary>
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