namespace MF.Contracts.Attributes;

/// <summary>
/// 标记类跳过自动容器注册的属�?
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SkipRegistrationAttribute : Attribute
{
    /// <summary>
    /// 跳过注册的原因（可选）
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// 初始化SkipRegistrationAttribute
    /// </summary>
    public SkipRegistrationAttribute()
    {
    }
    
    /// <summary>
    /// 初始化SkipRegistrationAttribute并指定跳过原�?
    /// </summary>
    /// <param name="reason">跳过注册的原�?/param>
    public SkipRegistrationAttribute(string reason)
    {
        Reason = reason;
    }
}
