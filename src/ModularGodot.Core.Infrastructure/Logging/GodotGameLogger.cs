using System.Text.RegularExpressions;
using Godot;
using ModularGodot.Core.Contracts.Abstractions.Bases;
using ModularGodot.Core.Contracts.Abstractions.Logging;
using ModularGodot.Core.Contracts.Attributes;

namespace ModularGodot.Core.Infrastructure.Logging;

/// <summary>
/// Godot游戏日志实现类
/// 提供基于Godot引擎的日志记录功能，支持不同级别的日志输出和颜色显示
/// </summary>
[Injectable(Lifetime.Singleton)]
public class GodotGameLogger : BaseInfrastructure, IGameLogger
{
    private static readonly Dictionary<string, Color> DefaultLogColors = new()
    {
        { "Trace", Colors.Gray },
        { "Debug", Colors.Cyan },
        { "Information", Colors.White },
        { "Warning", Colors.Yellow },
        { "Error", Colors.Red },
        { "Critical", Colors.DarkRed }
    };
    
    private Dictionary<string, Color> _logColors = new(DefaultLogColors);
    private readonly object _lock = new();
    
    // 支持 Microsoft.Extensions.Logging 风格的命名占位符：{Name} 和 {Value:F1}
    private static readonly Regex MessageTemplateRegex = new("\\{(?<name>[A-Za-z_][A-Za-z0-9_]*)(:(?<format>[^}]+))?\\}", RegexOptions.Compiled);

    /// <summary>
    /// 记录调试级别日志
    /// </summary>
    /// <param name="message">日志消息模板</param>
    /// <param name="args">日志消息参数</param>
    public void LogDebug(string message, params object[] args)
    {
        Log("Debug", message, args);
    }
    
    /// <summary>
    /// 记录信息级别日志
    /// </summary>
    /// <param name="message">日志消息模板</param>
    /// <param name="args">日志消息参数</param>
    public void LogInformation(string message, params object[] args)
    {
        Log("Information", message, args);
    }
    
    /// <summary>
    /// 记录警告级别日志
    /// </summary>
    /// <param name="message">日志消息模板</param>
    /// <param name="args">日志消息参数</param>
    public void LogWarning(string message, params object[] args)
    {
        Log("Warning", message, args);
    }
    
    /// <summary>
    /// 记录错误级别日志
    /// </summary>
    /// <param name="message">日志消息模板</param>
    /// <param name="args">日志消息参数</param>
    public void LogError(string message, params object[] args)
    {
        Log("Error", message, args);
    }
    
    /// <summary>
    /// 记录带异常信息的错误级别日志
    /// </summary>
    /// <param name="exception">异常对象</param>
    /// <param name="message">日志消息模板</param>
    /// <param name="args">日志消息参数</param>
    public void LogError(Exception exception, string message, params object[] args)
    {
        var formattedCore = args.Length > 0 ? SafeFormat(message, args) : message;
        var fullMessage = formattedCore + $"\nException: {exception}";
        Log("Error", fullMessage);
    }
    
    /// <summary>
    /// 记录关键级别日志
    /// </summary>
    /// <param name="message">日志消息模板</param>
    /// <param name="args">日志消息参数</param>
    public void LogCritical(string message, params object[] args)
    {
        Log("Critical", message, args);
    }
    
    /// <summary>
    /// 记录带异常信息的关键级别日志
    /// </summary>
    /// <param name="exception">异常对象</param>
    /// <param name="message">日志消息模板</param>
    /// <param name="args">日志消息参数</param>
    public void LogCritical(Exception exception, string message, params object[] args)
    {
        var formattedCore = args.Length > 0 ? SafeFormat(message, args) : message;
        var fullMessage = formattedCore + $"\nException: {exception}";
        Log("Critical", fullMessage);
    }
    

    
    /// <summary>
    /// 内部日志记录方法
    /// </summary>
    /// <param name="level">日志级别</param>
    /// <param name="message">日志消息</param>
    /// <param name="args">日志参数</param>
    private void Log(string level, string message, params object[] args)
    {
        if (IsDisposed) return;

        try
        {
            var formattedMessage = args.Length > 0 ? SafeFormat(message, args) : message;
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logMessage = $"[{timestamp}] [{level}] {formattedMessage}";

            // 控制台输出
            if (_logColors.TryGetValue(level, out var color))
            {
                GD.PrintRich($"[color={color.ToHtml()}]{logMessage}[/color]");
            }
            else
            {
                GD.Print(logMessage);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Logging error: {ex.Message}");
        }
    }

    /// <summary>
    /// 将命名占位符转换为顺序占位符以兼容 string.Format
    /// 例如：{Name} -> {0}，{Percent:F1} -> {0:F1}
    /// </summary>
    /// <param name="template">消息模板</param>
    /// <returns>转换后的消息模板</returns>
    private static string ConvertMessageTemplate(string template)
    {
        int index = 0;
        return MessageTemplateRegex.Replace(template, m =>
        {
            var fmt = m.Groups["format"].Success ? ":" + m.Groups["format"].Value : string.Empty;
            var replaced = "{" + (index++) + fmt + "}";
            return replaced;
        });
    }

    /// <summary>
    /// 安全格式化日志消息，支持命名参数格式
    /// </summary>
    /// <param name="template">消息模板</param>
    /// <param name="args">格式化参数</param>
    /// <returns>格式化后的消息</returns>
    private static string SafeFormat(string template, object[] args)
    {
        try
        {
            // 先尝试按命名模板转换
            var converted = ConvertMessageTemplate(template);
            return string.Format(converted, args);
        }
        catch
        {
            // 回退：直接返回原模板与参数的拼接，避免抛异常中断业务
            return args.Length > 0 ? ($"{template} | args: " + string.Join(", ", args.Select(a => a?.ToString()))) : template;
        }
    }
    
    /// <summary>
    /// 释放日志记录器资源
    /// </summary>
    /// <param name="disposing">是否释放托管资源</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // 没有需要特殊处理的资源
        }

        base.Dispose(disposing);
    }
}

