namespace ModularGodot.Core.Contracts.Attributes;

/// <summary>
/// 定义服务的生命周期作用域
/// </summary>
public enum Lifetime
{
    /// <summary>
    /// 瞬态：每次请求都创建新实例
    /// </summary>
    Transient,

    /// <summary>
    /// 作用域：每个作用域内使用单例
    /// </summary>
    Scoped,

    /// <summary>
    /// 单例：整个应用生命周期内使用单例
    /// </summary>
    Singleton
}