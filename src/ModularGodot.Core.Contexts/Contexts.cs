using Autofac;
using ModularGodot.Core.Contracts;
using IContainer = Autofac.IContainer;

namespace ModularGodot.Core.Contexts;

/// <summary>
/// 应用程序上下文管理器，负责依赖注入容器的初始化和管理
/// </summary>
public class Contexts : LazySingleton<Contexts>, IDisposable
{
    private readonly IContainer _container;
    private bool _disposed;

    /// <summary>
    /// 初始化 Contexts 实例，构建 Autofac 依赖注入容器
    /// </summary>
    public Contexts()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<SingleModule>();
        builder.RegisterModule<MediatorModule>();
        _container = builder.Build();
    }

    /// <summary>
    /// 从容器中解析指定类型的服务实例
    /// </summary>
    /// <typeparam name="T">要解析的服务类型</typeparam>
    /// <returns>服务实例</returns>
    /// <exception cref="ObjectDisposedException">当容器已被释放时抛出</exception>
    /// <exception cref="Autofac.Core.Registration.ComponentNotRegisteredException">当请求的服务未注册时抛出</exception>
    public T ResolveService<T>() where T : class
    {
        CheckDisposed();
        
        return _container.Resolve<T>();
    }
    
    /// <summary>
    /// 从容器中解析指定实例类型的服务
    /// </summary>
    /// <param name="type">用于获取目标服务类型的实例</param>
    /// <returns>服务实例</returns>
    /// <exception cref="ObjectDisposedException">当容器已被释放时抛出</exception>
    /// <exception cref="Autofac.Core.Registration.ComponentNotRegisteredException">当请求的服务未注册时抛出</exception>
    public object ResolveService(Type type)
    {
        CheckDisposed();
        
        return _container.Resolve(type);
    }
    /// <summary>
    /// 尝试从容器中解析指定类型的服务实例
    /// </summary>
    /// <typeparam name="T">要解析的服务类型</typeparam>
    /// <param name="service">输出参数，返回解析的服务实例（如果成功）</param>
    /// <returns>如果成功解析服务则返回 true，否则返回 false</returns>
    /// <exception cref="ObjectDisposedException">当容器已被释放时抛出</exception>
    public bool TryResolveService<T>(out T service) where T : class
    {
        CheckDisposed();
        return _container.TryResolve(out service);
    }

    /// <summary>
    /// 检查指定类型的服务是否已在容器中注册
    /// </summary>
    /// <typeparam name="T">要检查的服务类型</typeparam>
    /// <returns>如果服务已注册则返回 true，否则返回 false</returns>
    /// <exception cref="ObjectDisposedException">当容器已被释放时抛出</exception>
    public bool IsServiceRegistered<T>() where T : class
    {
        CheckDisposed();
        return _container.IsRegistered<T>();
    }

    /// <summary>
    /// 检查容器是否已被释放，如果已释放则抛出异常
    /// </summary>
    /// <exception cref="ObjectDisposedException">当容器已被释放时抛出</exception>
    private void CheckDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Contexts));
    }

    /// <summary>
    /// 释放容器资源
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing) {
            _container?.Dispose();
        }
            
        _disposed = true;
    }
}