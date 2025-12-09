namespace ModularGodot.Core.Contracts.Abstractions.Bases
{
    /// <summary>
    /// A base class for services.
    /// </summary>
    public abstract class BaseService<TConfig> : IService<TConfig>, IDisposable
    where TConfig : class
    {
        /// <summary>
        /// 释放标记
        /// </summary>
        protected bool _disposed;

        /// <summary>
        /// 获取对象是否已释放?
        /// </summary>
        protected bool IsDisposed => _disposed;

        /// <summary>
        /// 检查对象是否已释放，如果已释放则抛出异常
        /// </summary>
        /// <exception cref="ObjectDisposedException">对象已释放时抛出</exception>
        protected void CheckDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }

        /// <summary>
        /// 实现 IDisposable.Dispose()
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 受保护的虚方法，支持派生类扩展资源释放逻辑
        /// </summary>
        /// <param name="disposing">是否正在释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            _disposed = true;
        }


        // 终结器（析构函数），用于未显式调用Dispose时的补救
        ~BaseService()
        {
            Dispose(false);
        }
        /// <summary>
        /// Initialize the service with the given configuration.
        /// </summary>
        
        public abstract void Initialize(TConfig config);


    }
}
