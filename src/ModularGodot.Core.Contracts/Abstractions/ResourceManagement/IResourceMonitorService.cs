using ModularGodot.Core.Contracts.Abstractions.ResourceManagement.DTOs;

namespace ModularGodot.Core.Contracts.Abstractions.ResourceManagement;

/// <summary>
/// 资源监控查询服务接口 - Standard级别
/// 供管理和监控使用的查询接口
/// </summary>
public interface IResourceMonitorService
{
    /// <summary>
    /// 获取缓存统计信息
    /// </summary>
    /// <returns>缓存统计</returns>
    CacheStatistics GetCacheStatistics();

    /// <summary>
    /// 获取内存使用情况
    /// </summary>
    /// <returns>内存使用情况</returns>
    MemoryUsage GetMemoryUsage();

    /// <summary>
    /// 获取性能报告
    /// </summary>
    /// <param name="period">统计周期</param>
    /// <returns>性能报告</returns>
    PerformanceReport GetPerformanceReport(TimeSpan period);

    /// <summary>
    /// 获取系统配置
    /// </summary>
    /// <returns>系统配置</returns>
    ResourceSystemConfig GetConfiguration();

    /// <summary>
    /// 更新系统配置
    /// </summary>
    /// <param name="config">新配置</param>
    void UpdateConfiguration(ResourceSystemConfig config);
}
