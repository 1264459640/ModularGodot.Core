namespace ModularGodot.Core.Contracts.Abstractions.ResourceManagement.DTOs;

/// <summary>
/// 内存压力级别
/// </summary>
public enum MemoryPressureLevel
{
    /// <summary>
    /// 低压级
    /// </summary>
    Low,
    
    /// <summary>
    /// 中等压力
    /// </summary>
    Medium,
    
    /// <summary>
    /// 高压级
    /// </summary>
    High,
    
    /// <summary>
    /// 严重压力
    /// </summary>
    Critical
}
