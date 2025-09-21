using ModularGodot.Contracts.Abstractions.Logging;
using ModularGodot.Contracts.Abstractions.Messaging;
using ModularGodot.Contracts.Abstractions.ResourceLoading;
using ModularGodot.Contracts.Abstractions.ResourceManagement;
using ModularGodot.Contracts.Abstractions.Caching;
using ModularGodot.Contracts.Abstractions.Monitoring;
using ModularGodot.Contracts.Enums;
using ModularGodot.Contracts.Abstractions.Bases;

namespace ModularGodot.Core;

/// <summary>
/// ModularGodot.Core 主入口类
/// 提供统一的API来访问所有模块功�?/// </summary>
public static class ModularGodotCore
{
    /// <summary>
    /// 获取版本信息
    /// </summary>
    public static string Version => "0.1.0";
    
    /// <summary>
    /// 获取包描�?    /// </summary>
    public static string Description => "Complete ModularGodot.Core package for modular Godot game development";
    
    /// <summary>
    /// 获取支持的功能列�?    /// </summary>
    public static string[] SupportedFeatures => new[]
    {
        "Dependency Injection (Autofac)",
        "Event-Driven Architecture (MediatR)",
        "Caching System",
        "Logging Framework", 
        "Performance Monitoring",
        "Resource Management",
        "Resource Loading",
        "Reactive Extensions (R3)"
    };
    
    /// <summary>
    /// 获取所有可用的抽象接口类型
    /// </summary>
    public static Type[] AvailableInterfaces => new[]
    {
        typeof(ICacheService),
        typeof(IGameLogger),
        typeof(IEventBus),
        typeof(IEventSubscriber),
        typeof(IPerformanceMonitor),
        typeof(IMemoryMonitor),
        typeof(IResourceLoader),
        typeof(IResourceCacheService),
        typeof(IResourceMonitorService)
    };
    
    /// <summary>
    /// 获取所有基础类型
    /// </summary>
    public static Type[] BaseTypes => new[]
    {
        typeof(BaseInfrastructure)
    };
}