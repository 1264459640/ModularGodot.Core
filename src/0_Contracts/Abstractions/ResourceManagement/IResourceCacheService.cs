using MF.Contracts.Abstractions.ResourceLoading;

namespace MF.Contracts.Abstractions.ResourceManagement;

/// <summary>
/// 资源缓存服务接口 - Standard级别
/// 供其他服务使用的统一缓存接口
/// </summary>
public interface IResourceCacheService
{
    /// <summary>
    /// 异步获取缓存资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="key">缓存�?/param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源实例</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// 异步存储资源到缓�?    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="key">缓存�?/param>
    /// <param name="resource">资源实例</param>
    /// <param name="cacheStrategy">缓存策略</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>存储任务</returns>
    Task SetAsync<T>(string key, T resource, ResourceCacheStrategy cacheStrategy = ResourceCacheStrategy.Default, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// 异步移除缓存资源
    /// </summary>
    /// <param name="key">缓存�?/param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>移除任务</returns>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 异步清理过期缓存
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>清理任务</returns>
    Task CleanupAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 检查缓存键是否存在
    /// </summary>
    /// <param name="key">缓存�?/param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
