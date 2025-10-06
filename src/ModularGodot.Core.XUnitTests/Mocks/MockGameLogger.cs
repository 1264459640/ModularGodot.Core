using ModularGodot.Core.Contracts.Abstractions.Logging;
using System;
using System.Text.RegularExpressions;

namespace ModularGodot.Core.XUnitTests.Mocks
{
    public class MockGameLogger : IGameLogger
    {
        public void LogDebug(string message, params object[] args)
        {
            Console.WriteLine($"[DEBUG] {FormatMessage(message, args)}");
        }

        public void LogInformation(string message, params object[] args)
        {
            Console.WriteLine($"[INFO] {FormatMessage(message, args)}");
        }

        public void LogWarning(string message, params object[] args)
        {
            Console.WriteLine($"[WARNING] {FormatMessage(message, args)}");
        }

        public void LogError(string message, params object[] args)
        {
            Console.WriteLine($"[ERROR] {FormatMessage(message, args)}");
        }

        public void LogError(Exception exception, string message, params object[] args)
        {
            var formattedCore = FormatMessage(message, args);
            var fullMessage = "[ERROR] " + formattedCore + $"\nException: {exception?.Message}";
            Console.WriteLine(fullMessage);
        }

        public void LogCritical(string message, params object[] args)
        {
            Console.WriteLine($"[CRITICAL] {FormatMessage(message, args)}");
        }

        public void LogCritical(Exception exception, string message, params object[] args)
        {
            var formattedCore = FormatMessage(message, args);
            var fullMessage = "[CRITICAL] " + formattedCore + $"\nException: {exception?.Message}";
            Console.WriteLine(fullMessage);
        }

        /// <summary>
        /// 格式化消息，支持结构化日志格式 {PropertyName} 和传统格式
        /// </summary>
        /// <param name="message">消息模板</param>
        /// <param name="args">参数数组</param>
        /// <returns>格式化后的消息</returns>
        private static string FormatMessage(string message, object[] args)
        {
            if (args == null || args.Length == 0)
                return message;

            try
            {
                // 首先尝试标准 string.Format
                return string.Format(message, args);
            }
            catch (FormatException)
            {
                // 如果失败，可能是结构化日志格式，尝试解析 {PropertyName} 格式
                return FormatStructuredMessage(message, args);
            }
        }

        /// <summary>
        /// 格式化结构化日志消息（如："Subscribed to event type {EventType} with ID {SubscriptionId}"）
        /// </summary>
        private static string FormatStructuredMessage(string message, object[] args)
        {
            var result = message;

            // 匹配 {PropertyName} 格式的占位符
            var placeholderPattern = @"\{([^}]+)\}";
            var matches = Regex.Matches(message, placeholderPattern);

            var argIndex = 0;
            foreach (Match match in matches)
            {
                if (argIndex < args.Length)
                {
                    var placeholder = match.Value;
                    var value = args[argIndex]?.ToString() ?? "null";
                    result = result.Replace(placeholder, value);
                    argIndex++;
                }
            }

            return result;
        }
    }
}