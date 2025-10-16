using System.Collections.Concurrent;
using System.Diagnostics;
using ModularGodot.Core.Contracts.Abstractions.Bases;
using ModularGodot.Core.Contracts.Abstractions.Caching;
using ModularGodot.Core.Contracts.Abstractions.Messaging;
using ModularGodot.Core.Contracts.Abstractions.Monitoring;
using ModularGodot.Core.Contracts.Abstractions.ResourceManagement;
using ModularGodot.Core.Contracts.Abstractions.ResourceManagement.DTOs;
using ModularGodot.Core.Contracts.Attributes;
using ModularGodot.Core.Contracts.Enums;
using ModularGodot.Core.Contracts.Events.ResourceManagement;
using MemoryPressureLevel = ModularGodot.Core.Contracts.Events.ResourceManagement.MemoryPressureLevel;

namespace ModularGodot.Core.Infrastructure.ResourceManagement;

/// <summary>
/// 资源管理器 - 系统核心协调组件
/// </summary>
[Injectable(Lifetime.Singleton)]
public class ResourceManager : BaseInfrastructure, IResourceCacheService, IResourceMonitorService
{
    private readonly ICacheService _cacheService;
    private readonly IMemoryMonitor _memoryMonitor;
    private readonly IPerformanceMonitor _performanceMonitor;
    private readonly IEventBus _eventBus;
    private readonly ResourceSystemConfig _config;
    
    private readonly Timer? _cleanupTimer;
    private readonly object _lockObject = new();
    
    // 统计数据
    private int _hitCount;
    private int _missCount;
    private int _totalRequests;
    private int _errorCount;
    private readonly ConcurrentQueue<TimeSpan> _responseTimes = new();
    private readonly ConcurrentDictionary<string, DateTime> _cacheItems = new();
    
    public ResourceManager(
        ICacheService cacheService,
        IMemoryMonitor memoryMonitor,
        IPerformanceMonitor performanceMonitor,
        IEventBus eventBus,
        ResourceSystemConfig config)
    {
        _cacheService = cacheService;
        _memoryMonitor = memoryMonitor;
        _performanceMonitor = performanceMonitor;
        _eventBus = eventBus;
        _config = config;
        
        // 订阅内存监控事件
        _memoryMonitor.MemoryPressureDetected += OnMemoryPressureDetected;
        
        // 启动定时清理
        if (_config.EnableAutoCleanup)
        {
            _cleanupTimer = new Timer(OnCleanupTimer, null, _config.CleanupInterval, _config.CleanupInterval);
        }
        
        // 启动内存监控
        _memoryMonitor.StartMonitoring();
    }
    
    #region IResourceCacheService Implementation

    public T? Get<T>(string key) where T : class
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = _cacheService.Get<T>(key);

            lock (_lockObject)
            {
                _totalRequests++;
                if (result != null)
                {
                    _hitCount++;
                }
                else
                {
                    _missCount++;
                }
            }

            _responseTimes.Enqueue(stopwatch.Elapsed);

            if (_responseTimes.Count > 1000)
            {
                _responseTimes.TryDequeue(out _);
            }

            if (_config.EnablePerformanceMonitoring)
            {
                _performanceMonitor.RecordTimer("resource_cache_get", stopwatch.Elapsed,
                    new Dictionary<string, string> { { "hit", (result != null).ToString() } });
            }

            _eventBus.Publish(new ResourceLoadEvent(
                key,
                typeof(T).Name,
                result != null ? ResourceLoadResult.CacheHit : ResourceLoadResult.CacheMiss,
                stopwatch.Elapsed,
                0,
                result != null));

            return result;
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _errorCount);

            _eventBus.Publish(new ResourceLoadEvent(
                key,
                typeof(T).Name,
                ResourceLoadResult.Failed,
                stopwatch.Elapsed,
                0,
                false,
                ex.Message));

            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    public void Set<T>(string key, T resource, ResourceCacheStrategy cacheStrategy = ResourceCacheStrategy.Default) where T : class
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var expiration = GetExpirationFromStrategy(cacheStrategy);
            _cacheService.Set(key, resource, expiration);

            var expiryTime = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : DateTime.MaxValue;
            _cacheItems.TryAdd(key, expiryTime);

            if (_config.EnablePerformanceMonitoring)
            {
                _performanceMonitor.RecordTimer("resource_cache_set", stopwatch.Elapsed);
                _performanceMonitor.RecordCounter("resource_cache_items_added");
            }
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    public void Remove(string key)
    {
        _cacheService.Remove(key);
        _cacheItems.TryRemove(key, out _);

        if (_config.EnablePerformanceMonitoring)
        {
            _performanceMonitor.RecordCounter("resource_cache_items_removed");
        }
    }

    public void Cleanup()
    {
        PerformCleanup(CacheCleanupReason.Manual);
    }

    public bool Exists(string key)
    {
        return _cacheService.Exists(key);
    }

    #endregion
    
    #region IResourceMonitorService Implementation

    public CacheStatistics GetCacheStatistics()
    {
        var now = DateTime.UtcNow;
        var expiredCount = _cacheItems.Values.Count(expiry => expiry < now);

        lock (_lockObject)
        {
            var stats = new CacheStatistics
            {
                HitCount = _hitCount,
                MissCount = _missCount,
                TotalItems = _cacheItems.Count,
                TotalSize = 0, // 需要从缓存服务获取实际大小
                ExpiredItems = expiredCount,
                LastUpdated = DateTime.UtcNow
            };
            return stats;
        }
    }

    public MemoryUsage GetMemoryUsage()
    {
        var currentUsage = _memoryMonitor.GetCurrentMemoryUsage();
        var maxUsage = _config.MaxMemorySize;

        var usage = new MemoryUsage
        {
            CurrentUsage = currentUsage,
            MaxUsage = maxUsage,
            UsagePercentage = maxUsage > 0 ? (double)currentUsage / maxUsage : 0,
            AvailableMemory = Math.Max(0, maxUsage - currentUsage),
            GCCollectionCount = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2),
            LastChecked = DateTime.UtcNow
        };
        return usage;
    }

    public PerformanceReport GetPerformanceReport(TimeSpan period)
    {
        var cacheStats = GetCacheStatistics();
        var memoryStats = GetMemoryUsage();

        var responseTimes = _responseTimes.ToArray();

        var avgResponseTime = responseTimes.Length > 0
            ? TimeSpan.FromTicks((long)responseTimes.Average(t => t.Ticks))
            : TimeSpan.Zero;

        var fastestTime = responseTimes.Length > 0
            ? responseTimes.Min()
            : TimeSpan.Zero;

        var slowestTime = responseTimes.Length > 0
            ? responseTimes.Max()
            : TimeSpan.Zero;

        int totalRequests;
        int errorCount;
        lock (_lockObject)
        {
            totalRequests = _totalRequests;
            errorCount = _errorCount;
        }

        return new PerformanceReport
        {
            Period = period,
            CacheStats = cacheStats,
            MemoryStats = memoryStats,
            TotalRequests = totalRequests,
            AverageResponseTime = avgResponseTime,
            FastestResponseTime = fastestTime,
            SlowestResponseTime = slowestTime,
            ErrorCount = errorCount,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public ResourceSystemConfig GetConfiguration()
    {
        return _config;
    }

    public void UpdateConfiguration(ResourceSystemConfig config)
    {
        // 更新配置逻辑
        _config.MaxMemorySize = config.MaxMemorySize;
        _config.DefaultExpiration = config.DefaultExpiration;
        _config.MemoryPressureThreshold = config.MemoryPressureThreshold;
        _config.CleanupInterval = config.CleanupInterval;
        _config.EnableAutoCleanup = config.EnableAutoCleanup;
        _config.EnablePerformanceMonitoring = config.EnablePerformanceMonitoring;
        _config.MaxCacheItems = config.MaxCacheItems;

        // 更新内存监控器配置
        _memoryMonitor.MemoryPressureThreshold = config.MemoryPressureThreshold;
    }

    #endregion
    
    #region Event Handlers
    
    private void OnMemoryPressureDetected(long currentUsage)
    {
        var usagePercentage = _config.MaxMemorySize > 0 ? (double)currentUsage / _config.MaxMemorySize : 0;
        var pressureLevel = GetPressureLevel(usagePercentage);

        // 发布内存压力事件
        _eventBus.Publish(new MemoryPressureEvent(currentUsage, usagePercentage, pressureLevel));

        // 根据压力级别执行清理
        if (pressureLevel >= MemoryPressureLevel.Warning)
        {
            PerformCleanup(CacheCleanupReason.MemoryPressure);
        }
    }
    
    private void OnCleanupTimer(object? state)
    {
        PerformCleanup(CacheCleanupReason.Scheduled);
    }
    
    #endregion
    
    #region Private Methods
    
    private void PerformCleanup(CacheCleanupReason reason)
    {
        try
        {
            var itemsBeforeCleanup = _cacheItems.Count;
            var now = DateTime.UtcNow;

            // 清理过期项
            var expiredKeys = _cacheItems
                .Where(kvp => kvp.Value < now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _cacheService.Remove(key);
                _cacheItems.TryRemove(key, out _);
            }

            var itemsAfterCleanup = _cacheItems.Count;
            var memoryFreed = (itemsBeforeCleanup - itemsAfterCleanup) * 1024; // 估算释放的内存

            // 发布清理事件
            _eventBus.Publish(new CacheCleanupEvent(reason, itemsBeforeCleanup, itemsAfterCleanup, memoryFreed));

            if (_config.EnablePerformanceMonitoring)
            {
                _performanceMonitor.RecordCounter("cache_cleanup_performed", 1,
                    new Dictionary<string, string> { { "reason", reason.ToString() } });
                _performanceMonitor.RecordCounter("cache_items_cleaned", expiredKeys.Count);
            }
        }
        catch (Exception)
        {
            // 记录错误
            if (_config.EnablePerformanceMonitoring)
            {
                _performanceMonitor.RecordCounter("cache_cleanup_errors");
            }

            // 可以考虑记录日志或发布错误事件
        }
    }
    
    private static MemoryPressureLevel GetPressureLevel(double usagePercentage)
    {
        return usagePercentage switch
        {
            >= 0.9 => MemoryPressureLevel.Critical,
            >= 0.8 => MemoryPressureLevel.Warning,
            _ => MemoryPressureLevel.Normal
        };
    }
    
    private TimeSpan? GetExpirationFromStrategy(ResourceCacheStrategy cacheStrategy)
    {
        return cacheStrategy switch
        {
            ResourceCacheStrategy.NoCache => null,
            ResourceCacheStrategy.Permanent => null, // 永久缓存
            ResourceCacheStrategy.Default => _config.DefaultExpiration,
            ResourceCacheStrategy.ForceCache => _config.DefaultExpiration,
            ResourceCacheStrategy.WeakReference => _config.DefaultExpiration,
            ResourceCacheStrategy.Temporary => TimeSpan.FromMinutes(5), // 临时缓存5分钟
            _ => _config.DefaultExpiration
        };
    }
    
    #endregion
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cleanupTimer?.Dispose();
            _memoryMonitor.MemoryPressureDetected -= OnMemoryPressureDetected;
            _memoryMonitor.StopMonitoring();
        }
        base.Dispose(disposing);
    }
}
