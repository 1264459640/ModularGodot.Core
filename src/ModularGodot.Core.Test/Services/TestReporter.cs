using Godot;
using ModularGodot.Core.Test.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ModularGodot.Core.Test.Services
{
    /// <summary>
    /// 测试结果报告生成器
    /// </summary>
    public class TestReporter
    {
        private readonly TestConfiguration _configuration;
        private readonly TestLogger _testLogger;

        public TestReporter(TestConfiguration configuration = null)
        {
            _configuration = configuration ?? TestConfiguration.Instance;
            _testLogger = new TestLogger(_configuration);
        }

        /// <summary>
        /// 生成测试报告
        /// </summary>
        /// <param name="testResults">测试结果列表</param>
        /// <returns>测试报告</returns>
        public TestReport GenerateReport(List<TestResult> testResults)
        {
            if (testResults == null)
            {
                throw new ArgumentNullException(nameof(testResults));
            }

            _testLogger.LogInfo($"开始生成测试报告，包含 {testResults.Count} 个测试结果");

            var report = new TestReport
            {
                GeneratedAt = DateTime.Now,
                TotalTests = testResults.Count,
                PassedTests = testResults.Count(r => r.Status == TestExecutionStatus.Passed),
                FailedTests = testResults.Count(r => r.Status == TestExecutionStatus.Failed),
                SkippedTests = testResults.Count(r => r.Status == TestExecutionStatus.Skipped),
                TotalExecutionTimeMs = testResults.Sum(r => r.ExecutionTimeMs),
                AverageExecutionTimeMs = testResults.Count > 0 ? testResults.Average(r => r.ExecutionTimeMs) : 0,
                TestResults = new List<TestResult>(testResults)
            };

            report.SuccessRate = report.TotalTests > 0 ? (double)report.PassedTests / report.TotalTests * 100 : 0;
            report.IsSuccessful = report.FailedTests == 0 && report.PassedTests > 0;

            _testLogger.LogInfo($"测试报告生成完成 - 通过: {report.PassedTests}, 失败: {report.FailedTests}, 跳过: {report.SkippedTests}");
            return report;
        }

        /// <summary>
        /// 生成详细的测试报告
        /// </summary>
        /// <param name="testResults">测试结果列表</param>
        /// <returns>详细的测试报告</returns>
        public DetailedTestReport GenerateDetailedReport(List<TestResult> testResults)
        {
            _testLogger.LogInfo("开始生成详细测试报告");
            var basicReport = GenerateReport(testResults);
            var detailedReport = new DetailedTestReport
            {
                GeneratedAt = basicReport.GeneratedAt,
                TotalTests = basicReport.TotalTests,
                PassedTests = basicReport.PassedTests,
                FailedTests = basicReport.FailedTests,
                SkippedTests = basicReport.SkippedTests,
                TotalExecutionTimeMs = basicReport.TotalExecutionTimeMs,
                AverageExecutionTimeMs = basicReport.AverageExecutionTimeMs,
                SuccessRate = basicReport.SuccessRate,
                IsSuccessful = basicReport.IsSuccessful,
                TestResults = basicReport.TestResults,
                FailedTestDetails = new List<TestResult>(testResults.Where(r => r.Status == TestExecutionStatus.Failed)),
                PerformanceIssues = IdentifyPerformanceIssues(testResults),
                EnvironmentInfo = GetEnvironmentInfo()
            };

            _testLogger.LogInfo("详细测试报告生成完成");
            return detailedReport;
        }

        /// <summary>
        /// 将测试报告保存到文件
        /// </summary>
        /// <param name="report">测试报告</param>
        /// <param name="filePath">文件路径</param>
        public void SaveReportToFile(TestReport report, string filePath)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            _testLogger.LogInfo($"开始保存测试报告到文件: {filePath}");
            var content = GenerateReportContent(report);

            // 确保目录存在
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, content, Encoding.UTF8);
            _testLogger.LogInfo($"测试报告已保存到: {filePath}");
        }

        /// <summary>
        /// 生成报告内容
        /// </summary>
        /// <param name="report">测试报告</param>
        /// <returns>报告内容字符串</returns>
        private string GenerateReportContent(TestReport report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# 测试报告");
            sb.AppendLine($"生成时间: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine("## 概述");
            sb.AppendLine($"总测试数: {report.TotalTests}");
            sb.AppendLine($"通过: {report.PassedTests}");
            sb.AppendLine($"失败: {report.FailedTests}");
            sb.AppendLine($"跳过: {report.SkippedTests}");
            sb.AppendLine($"成功率: {report.SuccessRate:F2}%");
            sb.AppendLine($"总执行时间: {report.TotalExecutionTimeMs}ms");
            sb.AppendLine($"平均执行时间: {report.AverageExecutionTimeMs:F2}ms");
            sb.AppendLine();

            if (report.TestResults.Count > 0)
            {
                sb.AppendLine("## 详细结果");
                sb.AppendLine("| 场景名称 | 状态 | 执行时间(ms) | 消息 |");
                sb.AppendLine("|---------|------|-------------|------|");
                foreach (var result in report.TestResults)
                {
                    sb.AppendLine($"| {result.SceneName} | {result.Status} | {result.ExecutionTimeMs} | {result.Message} |");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 识别性能问题
        /// </summary>
        /// <param name="testResults">测试结果列表</param>
        /// <returns>性能问题列表</returns>
        private List<PerformanceIssue> IdentifyPerformanceIssues(List<TestResult> testResults)
        {
            var issues = new List<PerformanceIssue>();
            _testLogger.LogDebug("开始识别性能问题");

            foreach (var result in testResults)
            {
                if (result.ExecutionTimeMs > 100)
                {
                    issues.Add(new PerformanceIssue
                    {
                        SceneName = result.SceneName,
                        ExecutionTimeMs = result.ExecutionTimeMs,
                        IssueType = "执行时间过长",
                        Description = $"测试执行时间({result.ExecutionTimeMs}ms)超过了100ms的预期上限"
                    });
                    _testLogger.LogWarning($"发现性能问题: {result.SceneName} - 执行时间 {result.ExecutionTimeMs}ms");
                }
            }

            _testLogger.LogDebug($"性能问题识别完成，发现 {issues.Count} 个问题");
            return issues;
        }

        /// <summary>
        /// 获取环境信息
        /// </summary>
        /// <returns>环境信息</returns>
        private EnvironmentInfo GetEnvironmentInfo()
        {
            _testLogger.LogDebug("获取环境信息");
            return new EnvironmentInfo
            {
                Environment = _configuration.GetValue<string>("Environment", "Unknown"),
                IsDetailedLoggingEnabled = _configuration.IsDetailedLoggingEnabled(),
                IsPerformanceMonitoringEnabled = _configuration.IsPerformanceMonitoringEnabled(),
                TestTimeoutMs = _configuration.GetTestTimeoutMs(),
                MaxParallelTests = _configuration.GetMaxParallelTests()
            };
        }
    }

    /// <summary>
    /// 测试报告
    /// </summary>
    public class TestReport
    {
        public DateTime GeneratedAt { get; set; }
        public int TotalTests { get; set; }
        public int PassedTests { get; set; }
        public int FailedTests { get; set; }
        public int SkippedTests { get; set; }
        public long TotalExecutionTimeMs { get; set; }
        public double AverageExecutionTimeMs { get; set; }
        public double SuccessRate { get; set; }
        public bool IsSuccessful { get; set; }
        public List<TestResult> TestResults { get; set; }
    }

    /// <summary>
    /// 详细的测试报告
    /// </summary>
    public class DetailedTestReport : TestReport
    {
        public List<TestResult> FailedTestDetails { get; set; }
        public List<PerformanceIssue> PerformanceIssues { get; set; }
        public EnvironmentInfo EnvironmentInfo { get; set; }
    }

    /// <summary>
    /// 性能问题
    /// </summary>
    public class PerformanceIssue
    {
        public string SceneName { get; set; }
        public long ExecutionTimeMs { get; set; }
        public string IssueType { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// 环境信息
    /// </summary>
    public class EnvironmentInfo
    {
        public string Environment { get; set; }
        public bool IsDetailedLoggingEnabled { get; set; }
        public bool IsPerformanceMonitoringEnabled { get; set; }
        public int TestTimeoutMs { get; set; }
        public int MaxParallelTests { get; set; }
    }
}