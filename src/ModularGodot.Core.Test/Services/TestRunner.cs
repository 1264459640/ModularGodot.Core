using Godot;
using ModularGodot.Core.Test.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ModularGodot.Core.Test.Services
{
    /// <summary>
    /// 测试运行器，负责执行测试场景并验证结果
    /// </summary>
    public class TestRunner
    {
        private readonly List<TestResult> _testResults;
        private readonly object _lockObject = new object();
        private readonly TestLogger _testLogger;
        private readonly TestConfiguration _testConfiguration;

        public TestRunner(TestConfiguration configuration = null)
        {
            _testResults = new List<TestResult>();
            _testConfiguration = configuration ?? TestConfiguration.Instance;
            _testLogger = new TestLogger(_testConfiguration);
        }

        /// <summary>
        /// 运行单个测试场景
        /// </summary>
        /// <param name="testScene">要运行的测试场景节点</param>
        /// <returns>测试结果</returns>
        public async Task<TestResult> RunTestAsync(Node testScene)
        {
            if (testScene == null)
            {
                throw new ArgumentNullException(nameof(testScene));
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                _testLogger.LogInfo($"开始执行测试: {testScene.Name}");

                // 检查场景是否包含RunTest方法
                var runTestMethod = testScene.GetType().GetMethod("RunTest");
                if (runTestMethod == null)
                {
                    var result = new TestResult
                    {
                        SceneName = testScene.Name,
                        Status = TestExecutionStatus.Failed,
                        Message = "测试场景缺少RunTest方法",
                        ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                        CompletedAt = DateTime.Now
                    };
                    AddTestResult(result);
                    _testLogger.LogError($"测试 {testScene.Name} 失败: 测试场景缺少RunTest方法");
                    return result;
                }

                // 检查是否是开发环境
                if (!_testConfiguration.ShouldRunTests())
                {
                    var result = new TestResult
                    {
                        SceneName = testScene.Name,
                        Status = TestExecutionStatus.Skipped,
                        Message = "测试仅在开发环境中运行",
                        ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                        CompletedAt = DateTime.Now
                    };
                    AddTestResult(result);
                    _testLogger.LogWarning($"测试 {testScene.Name} 跳过: 测试仅在开发环境中运行");
                    return result;
                }

                // 执行测试
                runTestMethod.Invoke(testScene, null);

                // 等待测试完成
                await Task.Delay(100); // 给测试一些时间完成

                // 获取测试结果
                var getResultMethod = testScene.GetType().GetMethod("GetTestResult");
                if (getResultMethod != null)
                {
                    var result = (TestResult)getResultMethod.Invoke(testScene, null);
                    if (result != null)
                    {
                        AddTestResult(result);
                        _testLogger.LogInfo($"测试 {testScene.Name} 完成: {result.Status} - {result.Message}");
                        return result;
                    }
                }

                stopwatch.Stop();
                var defaultResult = new TestResult
                {
                    SceneName = testScene.Name,
                    Status = TestExecutionStatus.Passed,
                    Message = "测试执行完成",
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                    CompletedAt = DateTime.Now
                };
                AddTestResult(defaultResult);
                _testLogger.LogInfo($"测试 {testScene.Name} 完成: {defaultResult.Status} - {defaultResult.Message}");
                return defaultResult;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var result = new TestResult
                {
                    SceneName = testScene.Name,
                    Status = TestExecutionStatus.Failed,
                    Message = $"测试执行异常: {ex.Message}",
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                    CompletedAt = DateTime.Now
                };
                AddTestResult(result);
                _testLogger.LogError($"测试 {testScene.Name} 执行失败: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// 运行多个测试场景
        /// </summary>
        /// <param name="testScenes">要运行的测试场景节点列表</param>
        /// <returns>测试结果列表</returns>
        public async Task<List<TestResult>> RunTestsAsync(List<Node> testScenes)
        {
            _testLogger.LogInfo($"开始运行 {testScenes.Count} 个测试场景");
            var results = new List<TestResult>();

            foreach (var testScene in testScenes)
            {
                var result = await RunTestAsync(testScene);
                results.Add(result);
            }

            _testLogger.LogInfo($"完成运行 {testScenes.Count} 个测试场景");
            return results;
        }

        /// <summary>
        /// 验证测试结果
        /// </summary>
        /// <param name="testResult">要验证的测试结果</param>
        /// <returns>验证结果</returns>
        public TestValidationResult ValidateTestResult(TestResult testResult)
        {
            if (testResult == null)
            {
                _testLogger.LogError("测试结果验证失败: 测试结果为空");
                return new TestValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "测试结果为空"
                };
            }

            // 验证执行时间
            if (testResult.ExecutionTimeMs > 100)
            {
                _testLogger.LogWarning($"测试执行时间过长: {testResult.SceneName} - {testResult.ExecutionTimeMs}ms > 100ms");
                return new TestValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"测试执行时间过长: {testResult.ExecutionTimeMs}ms > 100ms"
                };
            }

            // 验证状态
            if (testResult.Status == TestExecutionStatus.Failed)
            {
                _testLogger.LogError($"测试失败: {testResult.SceneName} - {testResult.Message}");
                return new TestValidationResult
                {
                    IsValid = false,
                    ErrorMessage = testResult.Message
                };
            }

            _testLogger.LogDebug($"测试验证通过: {testResult.SceneName}");
            return new TestValidationResult
            {
                IsValid = true,
                ErrorMessage = null
            };
        }

        /// <summary>
        /// 获取所有测试结果
        /// </summary>
        /// <returns>测试结果列表</returns>
        public List<TestResult> GetTestResults()
        {
            lock (_lockObject)
            {
                return new List<TestResult>(_testResults);
            }
        }

        /// <summary>
        /// 清除测试结果
        /// </summary>
        public void ClearTestResults()
        {
            lock (_lockObject)
            {
                _testResults.Clear();
            }
            _testLogger.LogInfo("测试结果已清除");
        }

        /// <summary>
        /// 添加测试结果到结果列表
        /// </summary>
        /// <param name="result">测试结果</param>
        private void AddTestResult(TestResult result)
        {
            lock (_lockObject)
            {
                _testResults.Add(result);
            }
        }
    }

    /// <summary>
    /// 测试验证结果
    /// </summary>
    public class TestValidationResult
    {
        /// <summary>
        /// 验证是否通过
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 错误信息（如果验证失败）
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}