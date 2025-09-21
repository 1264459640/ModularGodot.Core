using ModularGodot.Contracts.Abstractions.Bases;
using ModularGodot.Contracts.Abstractions.Logging;
using ModularGodot.Contracts.Abstractions.Monitoring;

namespace MF.Infrastructure.Monitoring;

/// <summary>
/// 内存监控服务实现
/// </summary>
public class MemoryMonitor : BaseInfrastructure, IMemoryMonitor
{
    private readonly IGameLogger _logger;
    private readonly Timer _monitorTimer;
    private long _lastMemoryUsage;
    
    public event Action<long>? MemoryPressureDetected;
    public event Action? AutoReleaseTriggered;
    
    /// <summary>
    /// 自动释放阈值（字节�?
    /// </summary>
    public long AutoReleaseThreshold { get; set; } = 800 * 1024 * 1024; // 800MB
    
    /// <summary>
    /// 检查间�?
    /// </summary>
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(15);
    
    /// <summary>
    /// 内存压力阈�?
    /// </summary>
    public double MemoryPressureThreshold { get; set; } = 0.8; // 80%
    
    public MemoryMonitor(IGameLogger logger)
    {
        _logger = logger;
        _monitorTimer = new Timer(CheckMemoryUsage, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        
        _logger.LogInformation("MemoryMonitor initialized with threshold: {Threshold} bytes", AutoReleaseThreshold);
    }
    
    /// <summary>
    /// 开始监�?
    /// </summary>
    public void StartMonitoring()
    {
        if (IsDisposed) return;
        
        _monitorTimer.Change(CheckInterval, CheckInterval);
        _logger.LogInformation("Memory monitoring started with interval: {Interval}", CheckInterval);
    }
    
    /// <summary>
    /// 停止监控
    /// </summary>
    public void StopMonitoring()
    {
        if (IsDisposed) return;
        
        _monitorTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        _logger.LogInformation("Memory monitoring stopped");
    }
    
    /// <summary>
    /// 检查内存压�?
    /// </summary>
    /// <param name="currentUsage">当前内存使用�?/param>
    public void CheckMemoryPressure(long currentUsage)
    {
        if (currentUsage > AutoReleaseThreshold)
        {
            _logger.LogWarning("Memory pressure detected: {CurrentUsage} > {Threshold}", 
                FormatBytes(currentUsage), FormatBytes(AutoReleaseThreshold));
            
            MemoryPressureDetected?.Invoke(currentUsage);
            
            // 如果内存压力很高，触发自动释�?
            var pressureLevel = CalculatePressureLevel(currentUsage);
            if (pressureLevel == "High" || pressureLevel == "Critical")
            {
                AutoReleaseTriggered?.Invoke();
            }
        }
    }
    
    /// <summary>
    /// 获取当前内存使用�?
    /// </summary>
    /// <returns>内存使用量（字节�?/returns>
    public long GetCurrentMemoryUsage()
    {
        return GC.GetTotalMemory(false);
    }
    
    /// <summary>
    /// 强制垃圾回收
    /// </summary>
    public void ForceGarbageCollection()
    {
        _logger.LogInformation("Forcing garbage collection");
        
        var beforeGC = GetCurrentMemoryUsage();
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var afterGC = GetCurrentMemoryUsage();
        var freed = beforeGC - afterGC;
        
        _logger.LogInformation("Garbage collection completed. Freed: {FreedMemory}, Before: {BeforeMemory}, After: {AfterMemory}", 
            FormatBytes(freed), FormatBytes(beforeGC), FormatBytes(afterGC));
    }
    
    /// <summary>
    /// 获取当前内存压力级别
    /// </summary>
    /// <returns>内存压力级别</returns>
    public string GetCurrentPressureLevel()
    {
        var currentUsage = GetCurrentMemoryUsage();
        return CalculatePressureLevel(currentUsage);
    }
    
    private void CheckMemoryUsage(object? state)
    {
        try
        {
            var currentUsage = GetCurrentMemoryUsage();
            
            // 检查内存压�?
            CheckMemoryPressure(currentUsage);
            
            // 记录内存使用变化
            if (_lastMemoryUsage > 0)
            {
                var change = currentUsage - _lastMemoryUsage;
                var changePercent = (double)change / _lastMemoryUsage * 100;
                
                if (Math.Abs(changePercent) > 10) // 变化超过10%时记�?
                {
                    _logger.LogDebug("Memory usage changed: {Change} ({ChangePercent:F1}%), Current: {CurrentUsage}", 
                        FormatBytes(change), changePercent, FormatBytes(currentUsage));
                }
            }
            
            _lastMemoryUsage = currentUsage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during memory monitoring");
        }
    }
    
    private string CalculatePressureLevel(long currentUsage)
    {
        var pressureRatio = (double)currentUsage / AutoReleaseThreshold;
        
        return pressureRatio switch
        {
            < 0.5 => "Low",
            < 0.8 => "Medium",
            < 1.0 => "High",
            _ => "Critical"
        };
    }
    
    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        
        return $"{number:n1} {suffixes[counter]}";
    }
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _logger.LogInformation("Disposing MemoryMonitor");
            
            _monitorTimer?.Dispose();
            
            _logger.LogInformation("MemoryMonitor disposed");
        }
        
        base.Dispose(disposing);
    }
}
