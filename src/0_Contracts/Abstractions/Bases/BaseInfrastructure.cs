namespace ModularGodot.Contracts.Abstractions.Bases;

/// <summary>
/// 基础设施层的抽象基类，提供通用的资源管理和生命周期控制功能
/// </summary>
public abstract class BaseInfrastructure : IDisposable
{
    protected bool _disposed; // 释放标记
    protected readonly CancellationTokenSource CancellationTokenSource = new();

    /// <summary>
    /// 获取对象是否已释�?
    /// </summary>
    protected bool IsDisposed => _disposed;

    /// <summary>
    /// 检查对象是否已释放，如果已释放则抛出异�?
    /// </summary>
    /// <exception cref="ObjectDisposedException">对象已释放时抛出</exception>
    protected void CheckDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

    // 实现 IDisposable.Dispose()
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 受保护的虚方法，支持派生类扩�?
    /// </summary>
    /// <param name="disposing">是否正在释放托管资源</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        Unsubscribe();
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();

        _disposed = true;
    }

    /// <summary>
    /// 取消订阅事件，派生类应重写此方法以取消订阅相关事�?
    /// </summary>
    protected virtual void Unsubscribe() { }

    // 终结器（析构函数），用于未显式调用Dispose时的补救
    ~BaseInfrastructure()
    {
        Dispose(false);
    }
}
