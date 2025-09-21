using System.Diagnostics;
using Godot;
using ModularGodot.Contracts.Abstractions.Bases;
using ModularGodot.Contracts.Abstractions.ResourceLoading;
using ModularGodot.Contracts.Abstractions.ResourceManagement;
using ModularGodot.Contracts.Enums;

namespace MF.Infrastructure.ResourceLoading;

/// <summary>
/// Godot 资源加载器实�?- 集成资源缓存服务
/// </summary>
public class GodotResourceLoader : BaseInfrastructure, IResourceLoader
{
    private readonly IResourceCacheService _cacheService;
    private ResourceLoaderStatistics _statistics = new();
    
    public GodotResourceLoader(IResourceCacheService cacheService)
    {
        _cacheService = cacheService;
    }
    
    public async Task<T?> LoadAsync<T>(string path, CancellationToken cancellationToken = default) where T : class
    {
        return await LoadAsync<T>(path, ResourceCacheStrategy.Default, cancellationToken);
    }
    
    public async Task<T?> LoadAsync<T>(string path, ResourceCacheStrategy cacheStrategy, CancellationToken cancellationToken = default) where T : class
    {
        return await LoadAsync<T>(path, null, null, cacheStrategy, cancellationToken);
    }
    
    public async Task<T?> LoadAsync<T>(string path, Action<float>? progressCallback = null, TimeSpan? minLoadTime = null, ResourceCacheStrategy cacheStrategy = ResourceCacheStrategy.Default, CancellationToken cancellationToken = default) where T : class
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _statistics.TotalLoads++;
            _statistics.ActiveLoads++;
            
            T? result = null;
            
            // 1. 根据缓存策略尝试从缓存获�?
            if (cacheStrategy != ResourceCacheStrategy.NoCache)
            {
                result = await _cacheService.GetAsync<T>(path, cancellationToken);
                if (result != null)
                {
                    _statistics.CacheHits++;
                    _statistics.SuccessfulLoads++;
                    return result;
                }
                else
                {
                    _statistics.CacheMisses++;
                }
            }
            
            // 2. 从磁盘加载资�?
            progressCallback?.Invoke(0.1f);
            
            result = await LoadFromDiskAsync<T>(path, progressCallback, cancellationToken);
            
            if (result != null)
            {
                _statistics.SuccessfulLoads++;
                
                // 3. 根据缓存策略存储到缓�?
                await _cacheService.SetAsync(path, result, cacheStrategy, cancellationToken: cancellationToken);
            }
            else
            {
                _statistics.FailedLoads++;
            }
            
            // 4. 确保最小加载时�?
            if (minLoadTime.HasValue)
            {
                var elapsed = stopwatch.Elapsed;
                if (elapsed < minLoadTime.Value)
                {
                    var remainingTime = minLoadTime.Value - elapsed;
                    await Task.Delay(remainingTime, cancellationToken);
                }
            }
            
            progressCallback?.Invoke(1.0f);
            return result;
        }
        catch (Exception ex)
        {
            _statistics.FailedLoads++;
            _statistics.RecentErrors.Add(new ResourceLoadError
            {
                Path = path,
                Message = ex.Message,
                ExceptionType = ex.GetType().Name,
                Timestamp = DateTime.UtcNow
            });
            
            // 限制错误列表大小
            if (_statistics.RecentErrors.Count > 100)
            {
                _statistics.RecentErrors.RemoveAt(0);
            }
            
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _statistics.ActiveLoads--;
            _statistics.TotalLoadTime = _statistics.TotalLoadTime.Add(stopwatch.Elapsed);
            
            // 更新最�?最慢加载时�?
            if (stopwatch.Elapsed < _statistics.FastestLoadTime)
            {
                _statistics.FastestLoadTime = stopwatch.Elapsed;
            }
            if (stopwatch.Elapsed > _statistics.SlowestLoadTime)
            {
                _statistics.SlowestLoadTime = stopwatch.Elapsed;
            }
            
            _statistics.LastUpdated = DateTime.UtcNow;
        }
    }
    
    public T? Load<T>(string path) where T : class
    {
        return Load<T>(path, ResourceCacheStrategy.Default);
    }
    
    public T? Load<T>(string path, ResourceCacheStrategy cacheStrategy) where T : class
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _statistics.TotalLoads++;
            _statistics.ActiveLoads++;
            
            T? result = null;
            
            // 1. 根据缓存策略尝试从缓存获取（同步方式�?
            if (cacheStrategy != ResourceCacheStrategy.NoCache)
            {
                // 注意：这里需要同步版本的缓存获取，如果没有则跳过缓存
                // 为了避免死锁，暂时跳过缓存检�?
                _statistics.CacheMisses++;
            }
            
            // 2. 直接从磁盘加载资源（同步方式�?
            result = LoadFromDiskSync<T>(path);
            
            if (result != null)
            {
                _statistics.SuccessfulLoads++;
                
                // 3. 缓存存储也跳过，避免异步调用
                // TODO: 实现同步缓存存储或使用后台任�?
            }
            else
            {
                _statistics.FailedLoads++;
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _statistics.FailedLoads++;
            _statistics.RecentErrors.Add(new ResourceLoadError
            {
                Path = path,
                Message = ex.Message,
                ExceptionType = ex.GetType().Name,
                Timestamp = DateTime.UtcNow
            });
            
            // 限制错误列表大小
            if (_statistics.RecentErrors.Count > 100)
            {
                _statistics.RecentErrors.RemoveAt(0);
            }
            
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _statistics.ActiveLoads--;
            _statistics.TotalLoadTime = _statistics.TotalLoadTime.Add(stopwatch.Elapsed);
            
            // 更新最�?最慢加载时�?
            if (stopwatch.Elapsed < _statistics.FastestLoadTime)
            {
                _statistics.FastestLoadTime = stopwatch.Elapsed;
            }
            if (stopwatch.Elapsed > _statistics.SlowestLoadTime)
            {
                _statistics.SlowestLoadTime = stopwatch.Elapsed;
            }
            
            _statistics.LastUpdated = DateTime.UtcNow;
        }
    }
    
    public async Task PreloadAsync(string path, CancellationToken cancellationToken = default)
    {
        _statistics.PreloadCount++;
        
        // 检查是否已在缓存中
        if (await _cacheService.ExistsAsync(path, cancellationToken))
        {
            return;
        }
        
        // 预加载资源（假设�?Resource 类型�?
        await LoadAsync<Resource>(path, cancellationToken);
    }
    
    public async Task PreloadBatchAsync(IEnumerable<string> paths, CancellationToken cancellationToken = default)
    {
        var tasks = paths.Select(path => PreloadAsync(path, cancellationToken));
        await Task.WhenAll(tasks);
    }
    
    /// <summary>
    /// 获取加载器统计信�?
    /// </summary>
    /// <returns>统计信息</returns>
    public ResourceLoaderStatistics GetStatistics()
    {
        return _statistics;
    }
    
    /// <summary>
    /// 重置统计信息
    /// </summary>
    public void ResetStatistics()
    {
        _statistics = new ResourceLoaderStatistics();
    }
    
    #region Private Methods
    
    /// <summary>
    /// 同步从磁盘加载资�?
    /// </summary>
    private T? LoadFromDiskSync<T>(string path) where T : class
    {
        try
        {
            // 直接使用 Godot 的同步资源加�?
            var resource = GD.Load(path);
            
            // 类型转换
            if (resource is T typedResource)
            {
                return typedResource;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            // 记录加载错误但不抛出，让上层处理
            GD.PrintErr($"[GodotResourceLoader] 同步加载资源失败: {path}, 错误: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 异步从磁盘加载资�?
    /// </summary>
    private Task<T?> LoadFromDiskAsync<T>(string path, Action<float>? progressCallback, CancellationToken cancellationToken) where T : class
    {
        // 模拟进度更新
        progressCallback?.Invoke(0.3f);
        
        try
        {
            // 使用 Godot 的资源加�?
            var resource = GD.Load(path);
            
            progressCallback?.Invoke(0.8f);
            
            // 类型转换
            if (resource is T typedResource)
            {
                return Task.FromResult<T?>(typedResource);
            }
            
            return Task.FromResult<T?>(null);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[GodotResourceLoader] 异步加载资源失败: {path}, 错误: {ex.Message}");
            return Task.FromResult<T?>(null);
        }
    }
    
    #endregion
}
