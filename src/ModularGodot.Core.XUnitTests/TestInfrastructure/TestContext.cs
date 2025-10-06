using Autofac;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ModularGodot.Core.Contracts;
using ModularGodot.Core.Contracts.Abstractions.Logging;
using ModularGodot.Core.Contracts.Abstractions.Messaging;
using System.Reflection;
using ModularGodot.Core.XUnitTests.Mocks;

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

            // Register test-specific implementations
            builder.RegisterType<MockGameLogger>()
                .As<IGameLogger>()
                .SingleInstance();
            
            // 确保注册特别需要的类型
            builder.RegisterType<Infrastructure.Messaging.R3EventBus>()
                .As<IEventBus>()
                .SingleInstance();
            
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