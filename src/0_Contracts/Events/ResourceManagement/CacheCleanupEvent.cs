

using MF.Contracts.Abstractions.Messaging;

namespace MF.Contracts.Events.ResourceManagement;

/// <summary>
/// 缓存清理事件
/// </summary>
public class CacheCleanupEvent : EventBase
{
    /// <summary>
    /// 清理原因
    /// </summary>
    public CacheCleanupReason Reason { get; }
    
    /// <summary>
    /// 清理前项目数�?
    /// </summary>
    public int ItemsBeforeCleanup { get; }
    
    /// <summary>
    /// 清理后项目数�?
    /// </summary>
    public int ItemsAfterCleanup { get; }
    
    /// <summary>
    /// 释放的内存大小（字节�?
    /// </summary>
    public long MemoryFreed { get; }
    
    /// <summary>
    /// 构造函�?
    /// </summary>
    /// <param name="reason">清理原因</param>
    /// <param name="itemsBeforeCleanup">清理前项目数�?/param>
    /// <param name="itemsAfterCleanup">清理后项目数�?/param>
    /// <param name="memoryFreed">释放的内存大�?/param>
    public CacheCleanupEvent(CacheCleanupReason reason, int itemsBeforeCleanup, int itemsAfterCleanup, long memoryFreed)
        : base("CacheService")
    {
        Reason = reason;
        ItemsBeforeCleanup = itemsBeforeCleanup;
        ItemsAfterCleanup = itemsAfterCleanup;
        MemoryFreed = memoryFreed;
    }
}

/// <summary>
/// 缓存清理原因
/// </summary>
public enum CacheCleanupReason
{
    /// <summary>
    /// 内存压力
    /// </summary>
    MemoryPressure,
    
    /// <summary>
    /// 定时清理
    /// </summary>
    Scheduled,
    
    /// <summary>
    /// 手动清理
    /// </summary>
    Manual,
    
    /// <summary>
    /// 过期清理
    /// </summary>
    Expiration
}
