namespace ModularGodot.Contracts.Abstractions.ResourceManagement.DTOs;

/// <summary>
/// 缓存统计信息
/// </summary>
public class CacheStatistics
{
    /// <summary>
    /// 缓存项总数
    /// </summary>
    public int TotalItems { get; set; }
    
    /// <summary>
    /// 缓存总大小（字节�?
    /// </summary>
    public long TotalSize { get; set; }
    
    /// <summary>
    /// 缓存命中次数
    /// </summary>
    public int HitCount { get; set; }
    
    /// <summary>
    /// 缓存未命中次�?
    /// </summary>
    public int MissCount { get; set; }
    
    /// <summary>
    /// 缓存命中�?
    /// </summary>
    public double HitRate => (HitCount + MissCount) > 0 ? (double)HitCount / (HitCount + MissCount) : 0;
    
    /// <summary>
    /// 过期项数�?
    /// </summary>
    public int ExpiredItems { get; set; }
    
    /// <summary>
    /// 最后更新时�?
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
