using System.Text;
using Godot;
using Environment = System.Environment;

namespace ModularGodot.Core.Test.Services
{
    /// <summary>
    /// 测试日志记录器
    /// </summary>
    public class TestLogger
    {
        private readonly List<LogEntry> _logEntries;
        private readonly object _lockObject = new object();
        private readonly TestConfiguration _configuration;
        private readonly string _logFilePath;

        public TestLogger(TestConfiguration configuration = null)
        {
            _logEntries = new List<LogEntry>();
            _configuration = configuration ?? TestConfiguration.Instance;
            _logFilePath = "res://TestLogs/test_log.txt";
        }

        /// <summary>
        /// 记录信息级别日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="sceneName">场景名称（可选）</param>
        public void LogInfo(string message, string sceneName = null)
        {
            AddLogEntry(LogLevel.Info, message, sceneName);
        }

        /// <summary>
        /// 记录警告级别日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="sceneName">场景名称（可选）</param>
        public void LogWarning(string message, string sceneName = null)
        {
            AddLogEntry(LogLevel.Warning, message, sceneName);
        }

        /// <summary>
        /// 记录错误级别日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="sceneName">场景名称（可选）</param>
        public void LogError(string message, string sceneName = null)
        {
            AddLogEntry(LogLevel.Error, message, sceneName);
        }

        /// <summary>
        /// 记录调试级别日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="sceneName">场景名称（可选）</param>
        public void LogDebug(string message, string sceneName = null)
        {
            // 只有在启用详细日志记录时才记录调试信息
            if (_configuration.IsDetailedLoggingEnabled())
            {
                AddLogEntry(LogLevel.Debug, message, sceneName);
            }
        }

        /// <summary>
        /// 添加日志条目
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        /// <param name="sceneName">场景名称（可选）</param>
        private void AddLogEntry(LogLevel level, string message, string sceneName)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message,
                SceneName = sceneName
            };

            lock (_lockObject)
            {
                _logEntries.Add(logEntry);
            }

            // 输出到Godot控制台
            OutputToConsole(logEntry);

            // 如果启用文件日志记录，则写入文件
            if (ShouldLogToFile())
            {
                WriteToFile(logEntry);
            }
        }

        /// <summary>
        /// 输出日志到Godot控制台
        /// </summary>
        /// <param name="logEntry">日志条目</param>
        private void OutputToConsole(LogEntry logEntry)
        {
            var logMessage = $"[{logEntry.Timestamp:HH:mm:ss.fff}] [{logEntry.Level}] {logEntry.Message}";
            if (!string.IsNullOrEmpty(logEntry.SceneName))
            {
                logMessage = $"[{logEntry.Timestamp:HH:mm:ss.fff}] [{logEntry.Level}] [{logEntry.SceneName}] {logEntry.Message}";
            }

            switch (logEntry.Level)
            {
                case LogLevel.Error:
                    GD.PrintErr(logMessage);
                    break;
                case LogLevel.Warning:
                    GD.Print(logMessage);
                    break;
                default:
                    GD.Print(logMessage);
                    break;
            }
        }

        /// <summary>
        /// 将日志写入文件
        /// </summary>
        /// <param name="logEntry">日志条目</param>
        private void WriteToFile(LogEntry logEntry)
        {
            try
            {
                var logMessage = $"[{logEntry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{logEntry.Level}]";
                if (!string.IsNullOrEmpty(logEntry.SceneName))
                {
                    logMessage += $" [{logEntry.SceneName}]";
                }
                logMessage += $" {logEntry.Message}{Environment.NewLine}";

                // 确保目录存在
                var directory = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.AppendAllText(_logFilePath, logMessage, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                // 如果写入文件失败，只输出到控制台
                GD.PrintErr($"写入日志文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查是否应该记录到文件
        /// </summary>
        /// <returns>如果应该记录到文件返回true，否则返回false</returns>
        private bool ShouldLogToFile()
        {
            // 在实际实现中，这里可以检查配置或环境变量
            // 为了简化，我们假设总是返回false
            return false;
        }

        /// <summary>
        /// 获取所有日志条目
        /// </summary>
        /// <returns>日志条目列表</returns>
        public List<LogEntry> GetLogEntries()
        {
            lock (_lockObject)
            {
                return new List<LogEntry>(_logEntries);
            }
        }

        /// <summary>
        /// 清除所有日志条目
        /// </summary>
        public void ClearLogs()
        {
            lock (_lockObject)
            {
                _logEntries.Clear();
            }
        }

        /// <summary>
        /// 获取指定级别的日志条目
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <returns>指定级别的日志条目列表</returns>
        public List<LogEntry> GetLogEntriesByLevel(LogLevel level)
        {
            lock (_lockObject)
            {
                return _logEntries.FindAll(entry => entry.Level == level);
            }
        }

        /// <summary>
        /// 获取指定场景的日志条目
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <returns>指定场景的日志条目列表</returns>
        public List<LogEntry> GetLogEntriesByScene(string sceneName)
        {
            lock (_lockObject)
            {
                return _logEntries.FindAll(entry => entry.SceneName == sceneName);
            }
        }
    }

    /// <summary>
    /// 日志条目
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public string SceneName { get; set; }
    }

    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
}