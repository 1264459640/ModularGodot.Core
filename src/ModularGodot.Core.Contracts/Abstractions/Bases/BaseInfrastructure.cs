namespace ModularGodot.Core.Contracts.Abstractions.Bases;

/// <summary>
/// 基础设施层的抽象基类，提供通用的资源管理和生命周期控制功能
/// </summary>
public abstract class BaseInfrastructure : IDisposable
{
    /// <summary>
    /// 释放标记
    /// </summary>
    protected bool _disposed; 

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
    
    /// <summary>
    /// 实现 IDisposable.Dispose()
    /// </summary>
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

        _disposed = true;
    }
    

    // 终结器（析构函数），用于未显式调用Dispose时的补救
    ~BaseInfrastructure()
    {
        Dispose(false);
    }
}
