namespace ModularGodot.Contracts.Enums;

/// <summary>
/// 资源缓存策略枚举
/// </summary>
public enum ResourceCacheStrategy
{
    /// <summary>
    /// 默认策略 - 使用系统默认的缓存行�?    /// </summary>
    Default,
    
    /// <summary>
    /// 不缓�?- 每次都重新加载资�?    /// </summary>
    NoCache,
    
    /// <summary>
    /// 内存缓存 - 将资源缓存在内存�?    /// </summary>
    MemoryCache,
    
    /// <summary>
    /// 磁盘缓存 - 将资源缓存到磁盘
    /// </summary>
    DiskCache,
    
    /// <summary>
    /// 永久缓存 - 资源一旦加载就永久保存在内存中
    /// </summary>
    PermanentCache,
    
    /// <summary>
    /// 弱引用缓�?- 使用弱引用缓存，允许垃圾回收
    /// </summary>
    WeakReference,
    
    /// <summary>
    /// 永久缓存 - 资源一旦加载就永久保存在内存中
    /// </summary>
    Permanent,
    
    /// <summary>
    /// 强制缓存 - 强制将资源缓存，即使缓存已满
    /// </summary>
    ForceCache,
    
    /// <summary>
    /// 临时缓存 - 短期缓存，会较快过期
    /// </summary>
    Temporary
}