namespace ModularGodot.Core.Contracts.Abstractions.Caching;

/// <summary>
/// 缓存服务抽象接口 - Critical级别
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// 获取缓存值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <returns>缓存值，如果不存在则返回null</returns>
    T? Get<T>(string key) where T : class;

    /// <summary>
    /// 设置缓存值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">缓存值</param>
    /// <param name="expiration">过期时间</param>
    void Set<T>(string key, T value, TimeSpan? expiration = null) where T : class;

    /// <summary>
    /// 检查缓存键是否存在
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <returns>是否存在</returns>
    bool Exists(string key);

    /// <summary>
    /// 移除缓存键
    /// </summary>
    /// <param name="key">缓存键</param>
    void Remove(string key);

    /// <summary>
    /// 清空所有缓存
    /// </summary>
    void Clear();

}
