namespace ModularGodot.Core.Contracts.Attributes;

/// <summary>
/// 标记类可以被依赖注入容器注册的特性
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class InjectableAttribute : Attribute
{
    /// <summary>
    /// 服务的生命周期
    /// </summary>
    public Lifetime Lifetime { get; set; } = Lifetime.Singleton;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="lifetime">服务的生命周期，默认为单例</param>
    public InjectableAttribute(Lifetime lifetime = Lifetime.Singleton)
    {
        Lifetime = lifetime;
    }
}
