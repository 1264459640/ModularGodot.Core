namespace MF.Contracts.Abstractions.ResourceManagement.DTOs;

/// <summary>
/// 性能报告
/// </summary>
public class PerformanceReport
{
    /// <summary>
    /// 报告周期
    /// </summary>
    public TimeSpan Period { get; set; }
    
    /// <summary>
    /// 缓存统计
    /// </summary>
    public CacheStatistics CacheStats { get; set; } = new();
    
    /// <summary>
    /// 内存统计
    /// </summary>
    public MemoryUsage MemoryStats { get; set; } = new();
    
    /// <summary>
    /// 总请求数
    /// </summary>
    public int TotalRequests { get; set; }
    
    /// <summary>
    /// 平均响应时间
    /// </summary>
    public TimeSpan AverageResponseTime { get; set; }
    
    /// <summary>
    /// 最快响应时�?
    /// </summary>
    public TimeSpan FastestResponseTime { get; set; }
    
    /// <summary>
    /// 最慢响应时�?
    /// </summary>
    public TimeSpan SlowestResponseTime { get; set; }
    
    /// <summary>
    /// 错误数量
    /// </summary>
    public int ErrorCount { get; set; }
    
    /// <summary>
    /// 报告生成时间
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
