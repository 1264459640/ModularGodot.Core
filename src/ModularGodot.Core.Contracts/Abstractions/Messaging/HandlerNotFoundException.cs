namespace ModularGodot.Core.Contracts.Abstractions.Messaging;

/// <summary>
/// 当无法找到处理特定消息的处理器时抛出的异常
/// </summary>
public class HandlerNotFoundException : Exception
{
    /// <summary>
    /// 使用指定的错误消息初始化 HandlerNotFoundException 类的新实例
    /// </summary>
    /// <param name="message">描述错误的消息</param>
    public HandlerNotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    /// 使用指定的错误消息和对作为此异常原因的内部异常的引用来初始化 HandlerNotFoundException 类的新实例
    /// </summary>
    /// <param name="message">描述错误的消息</param>
    /// <param name="innerException">导致当前异常的异常；如果未指定内部异常，则是一个 null 引用</param>
    public HandlerNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}