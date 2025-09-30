namespace ModularGodot.Contracts.Abstractions.ResourceManagement.DTOs;

/// <summary>
/// 系统信息
/// </summary>
public class SystemInfo
{
    /// <summary>
    /// 操作系统
    /// </summary>
    public string OperatingSystem { get; set; } = string.Empty;
    
    /// <summary>
    /// 处理器数�?    /// </summary>
    public int ProcessorCount { get; set; }
    
    /// <summary>
    /// 总内�?    /// </summary>
    public long TotalMemory { get; set; }
    
    /// <summary>
    /// 可用内存
    /// </summary>
    public long AvailableMemory { get; set; }
    
    /// <summary>
    /// 进程启动时间
    /// </summary>
    public DateTime ProcessStartTime { get; set; }
    
    /// <summary>
    /// 运行时间
    /// </summary>
    public TimeSpan Uptime { get; set; }
}
