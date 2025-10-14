using Godot;
using ModularGodot.Core.Test.Models;
using ModularGodot.Core.Test.Services;
using System.Diagnostics;

namespace ModularGodot.Core.Test.Scenes
{
    /// <summary>
    /// 测试场景基类，提供通用的测试执行框架
    /// </summary>
    public abstract partial class BaseTestScene : Node
    {
        protected TestResult _testResult;
        protected bool _testExecuted = false;
        protected TestConfiguration _testConfiguration;
        protected TestLogger _testLogger;
        protected string _sceneName;

        public override void _Ready()
        {
            _sceneName = GetType().Name;
            _testConfiguration = TestConfiguration.Instance;
            _testLogger = new TestLogger(_testConfiguration);
            _testLogger.LogInfo($"{_sceneName} 初始化完成", _sceneName);

            // Add a button to run the test
            var runButton = new Button();
            runButton.Text = "Run Test";
            runButton.Pressed += RunTest;
            AddChild(runButton);
        }

        [Godot.Export]
        public string TestDescription { get; set; } = "测试场景";

        /// <summary>
        /// 运行测试的核心逻辑
        /// </summary>
        public void RunTest()
        {
            _testLogger.LogInfo($"开始执行测试: {TestDescription}", _sceneName);

            // 检查是否应该运行测试
            if (!_testConfiguration.ShouldRunTests())
            {
                _testResult = new TestResult
                {
                    SceneName = _sceneName,
                    Status = TestExecutionStatus.Skipped,
                    Message = "测试仅在开发环境中运行",
                    ExecutionTimeMs = 0,
                    CompletedAt = DateTime.Now
                };
                _testLogger.LogWarning("测试未在开发环境中运行，跳过执行", _sceneName);
                return;
            }

            if (_testExecuted)
            {
                _testLogger.LogInfo("测试已执行，跳过重复执行", _sceneName);
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // 执行具体的测试逻辑
                ExecuteTest(stopwatch);

                _testExecuted = true;
                _testLogger.LogInfo($"测试通过: {TestDescription} - 执行时间: {stopwatch.ElapsedMilliseconds}ms", _sceneName);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _testResult = new TestResult
                {
                    SceneName = _sceneName,
                    Status = TestExecutionStatus.Failed,
                    Message = $"测试执行异常: {ex.Message}",
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                    CompletedAt = DateTime.Now
                };
                _testLogger.LogError($"测试失败: {ex.Message}", _sceneName);
            }
        }

        /// <summary>
        /// 执行具体的测试逻辑，由子类实现
        /// </summary>
        /// <param name="stopwatch">计时器</param>
        protected abstract void ExecuteTest(Stopwatch stopwatch);

        /// <summary>
        /// 获取测试结果
        /// </summary>
        /// <returns>测试结果</returns>
        public TestResult GetTestResult()
        {
            return _testResult;
        }

        /// <summary>
        /// 重置测试状态
        /// </summary>
        public void ResetTest()
        {
            _testResult = null;
            _testExecuted = false;
            _testLogger.LogInfo("测试状态已重置", _sceneName);
        }

        /// <summary>
        /// 创建成功的测试结果
        /// </summary>
        /// <param name="message">成功消息</param>
        /// <param name="stopwatch">计时器</param>
        /// <returns>测试结果</returns>
        protected TestResult CreateSuccessResult(string message, Stopwatch stopwatch)
        {
            stopwatch.Stop();
            return new TestResult
            {
                SceneName = _sceneName,
                Status = TestExecutionStatus.Passed,
                Message = message,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                CompletedAt = DateTime.Now
            };
        }

        /// <summary>
        /// 创建失败的测试结果
        /// </summary>
        /// <param name="message">失败消息</param>
        /// <param name="stopwatch">计时器</param>
        /// <returns>测试结果</returns>
        protected TestResult CreateFailureResult(string message, Stopwatch stopwatch)
        {
            stopwatch.Stop();
            return new TestResult
            {
                SceneName = _sceneName,
                Status = TestExecutionStatus.Failed,
                Message = message,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                CompletedAt = DateTime.Now
            };
        }
    }
}