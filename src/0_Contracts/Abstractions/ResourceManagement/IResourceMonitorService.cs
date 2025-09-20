using MF.Contracts.Abstractions.ResourceManagement.DTOs;

namespace MF.Contracts.Abstractions.ResourceManagement;

/// <summary>
/// 资源监控查询服务接口 - Standard级别
/// 供管理和监控使用的查询接�?/// </summary>
public interface IResourceMonitorService
{
    /// <summary>
    /// 异步获取缓存统计信息
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>缓存统计</returns>
    Task<CacheStatistics> GetCacheStatisticsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 异步获取内存使用情况
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>内存使用情况</returns>
    Task<MemoryUsage> GetMemoryUsageAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 异步获取性能报告
    /// </summary>
    /// <param name="period">统计周期</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>性能报告</returns>
    Task<PerformanceReport> GetPerformanceReportAsync(TimeSpan period, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 异步获取系统配置
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>系统配置</returns>
    Task<ResourceSystemConfig> GetConfigurationAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 异步更新系统配置
    /// </summary>
    /// <param name="config">新配�?/param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>更新任务</returns>
    Task UpdateConfigurationAsync(ResourceSystemConfig config, CancellationToken cancellationToken = default);
}
