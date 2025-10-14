using Godot;
using System;
using System.Collections.Generic;

namespace ModularGodot.Core.Test.Services
{
    /// <summary>
    /// 测试配置管理
    /// </summary>
    public class TestConfiguration
    {
        private readonly Dictionary<string, object> _settings;
        private static TestConfiguration _instance;
        private static readonly object _lockObject = new object();
        private readonly TestLogger _testLogger;

        private TestConfiguration()
        {
            _settings = new Dictionary<string, object>();
            _testLogger = new TestLogger(this);
            InitializeDefaultSettings();
            _testLogger.LogInfo("测试配置管理器初始化完成");
        }

        /// <summary>
        /// 获取配置管理器实例
        /// </summary>
        public static TestConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new TestConfiguration();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 初始化默认设置
        /// </summary>
        private void InitializeDefaultSettings()
        {
            _settings["TestTimeoutMs"] = 5000;
            _settings["EnableDetailedLogging"] = true;
            _settings["Environment"] = "Development";
            _settings["MaxParallelTests"] = 1; // Godot场景测试不能并行执行
            _settings["TestResultSavePath"] = "res://TestResults/";
            _settings["EnablePerformanceMonitoring"] = true;
            _settings["MinSuccessfulTestRuns"] = 1;
            _testLogger.LogInfo("默认测试配置初始化完成");
        }

        /// <summary>
        /// 获取配置值
        /// </summary>
        /// <typeparam name="T">配置值类型</typeparam>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        public T GetValue<T>(string key, T defaultValue = default(T))
        {
            if (_settings.ContainsKey(key))
            {
                var value = _settings[key];
                if (value is T directValue)
                {
                    _testLogger.LogDebug($"获取配置值: {key} = {value}");
                    return directValue;
                }

                // 尝试转换类型
                try
                {
                    var convertedValue = (T)Convert.ChangeType(value, typeof(T));
                    _testLogger.LogDebug($"获取配置值(转换): {key} = {convertedValue}");
                    return convertedValue;
                }
                catch (Exception ex)
                {
                    _testLogger.LogWarning($"配置值转换失败: {key} - {ex.Message}, 返回默认值: {defaultValue}");
                    return defaultValue;
                }
            }

            _testLogger.LogDebug($"配置键不存在，返回默认值: {key} = {defaultValue}");
            return defaultValue;
        }

        /// <summary>
        /// 设置配置值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="value">配置值</param>
        public void SetValue(string key, object value)
        {
            _settings[key] = value;
            _testLogger.LogInfo($"设置配置值: {key} = {value}");
        }

        /// <summary>
        /// 检查是否是开发环境
        /// </summary>
        /// <returns>如果是开发环境返回true，否则返回false</returns>
        public bool IsDevelopmentEnvironment()
        {
            var environment = GetValue<string>("Environment", "Production");
            var isDev = environment.Equals("Development", StringComparison.OrdinalIgnoreCase);
            _testLogger.LogDebug($"环境检查: {environment} - 是否为开发环境: {isDev}");
            return isDev;
        }

        /// <summary>
        /// 检查测试是否应该运行
        /// </summary>
        /// <returns>如果测试应该运行返回true，否则返回false</returns>
        public bool ShouldRunTests()
        {
            var shouldRun = IsDevelopmentEnvironment();
            _testLogger.LogDebug($"测试运行检查: 是否应该运行测试: {shouldRun}");
            return shouldRun;
        }

        /// <summary>
        /// 获取测试超时时间（毫秒）
        /// </summary>
        /// <returns>测试超时时间</returns>
        public int GetTestTimeoutMs()
        {
            var timeout = GetValue<int>("TestTimeoutMs", 5000);
            _testLogger.LogDebug($"获取测试超时时间: {timeout}ms");
            return timeout;
        }

        /// <summary>
        /// 检查是否启用详细日志记录
        /// </summary>
        /// <returns>如果启用详细日志记录返回true，否则返回false</returns>
        public bool IsDetailedLoggingEnabled()
        {
            var enabled = GetValue<bool>("EnableDetailedLogging", false);
            _testLogger.LogDebug($"详细日志记录检查: {enabled}");
            return enabled;
        }

        /// <summary>
        /// 获取最大并行测试数
        /// </summary>
        /// <returns>最大并行测试数</returns>
        public int GetMaxParallelTests()
        {
            var maxParallel = GetValue<int>("MaxParallelTests", 1);
            _testLogger.LogDebug($"获取最大并行测试数: {maxParallel}");
            return maxParallel;
        }

        /// <summary>
        /// 检查是否启用性能监控
        /// </summary>
        /// <returns>如果启用性能监控返回true，否则返回false</returns>
        public bool IsPerformanceMonitoringEnabled()
        {
            var enabled = GetValue<bool>("EnablePerformanceMonitoring", false);
            _testLogger.LogDebug($"性能监控检查: {enabled}");
            return enabled;
        }

        /// <summary>
        /// 获取测试结果保存路径
        /// </summary>
        /// <returns>测试结果保存路径</returns>
        public string GetTestResultSavePath()
        {
            var path = GetValue<string>("TestResultSavePath", "res://TestResults/");
            _testLogger.LogDebug($"获取测试结果保存路径: {path}");
            return path;
        }

        /// <summary>
        /// 获取最小成功测试运行次数
        /// </summary>
        /// <returns>最小成功测试运行次数</returns>
        public int GetMinSuccessfulTestRuns()
        {
            var minRuns = GetValue<int>("MinSuccessfulTestRuns", 1);
            _testLogger.LogDebug($"获取最小成功测试运行次数: {minRuns}");
            return minRuns;
        }

        /// <summary>
        /// 重置配置到默认值
        /// </summary>
        public void ResetToDefaults()
        {
            _settings.Clear();
            InitializeDefaultSettings();
            _testLogger.LogInfo("配置已重置为默认值");
        }
    }
}