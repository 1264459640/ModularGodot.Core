using ModularGodot.Core.Contracts.Enums;

namespace ModularGodot.Core.Contracts.Abstractions.ResourceManagement;

/// <summary>
/// 资源缓存服务接口 - Standard级别
/// 供其他服务使用的统一缓存接口
/// </summary>
public interface IResourceCacheService
{
    /// <summary>
    /// 获取缓存资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <returns>资源实例</returns>
    T? Get<T>(string key) where T : class;

    /// <summary>
    /// 存储资源到缓存
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="resource">资源实例</param>
    /// <param name="cacheStrategy">缓存策略</param>
    void Set<T>(string key, T resource, ResourceCacheStrategy cacheStrategy = ResourceCacheStrategy.Default) where T : class;

    /// <summary>
    /// 移除缓存资源
    /// </summary>
    /// <param name="key">缓存键</param>
    void Remove(string key);

    /// <summary>
    /// 清理过期缓存
    /// </summary>
    void Cleanup();

    /// <summary>
    /// 检查缓存键是否存在
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <returns>是否存在</returns>
    bool Exists(string key);
}
