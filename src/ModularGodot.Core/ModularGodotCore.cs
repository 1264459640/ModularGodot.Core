using MF.Contracts.Abstractions.Logging;
using MF.Contracts.Abstractions.Messaging;
using MF.Contracts.Abstractions.ResourceLoading;
using MF.Contracts.Abstractions.ResourceManagement;
using MF.Contracts.Abstractions.Caching;
using MF.Contracts.Abstractions.Monitoring;
using MF.Contracts.Enums;
using MF.Contracts.Abstractions.Bases;

namespace ModularGodot.Core;

/// <summary>
/// ModularGodot.Core 主入口类
/// 提供统一的API来访问所有模块功能
/// </summary>
public static class ModularGodotCore
{
    /// <summary>
    /// 获取版本信息
    /// </summary>
    public static string Version => "0.1.0";
    
    /// <summary>
    /// 获取包描述
    /// </summary>
    public static string Description => "Complete ModularGodot.Core package for modular Godot game development";
    
    /// <summary>
    /// 获取支持的功能列表
    /// </summary>
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