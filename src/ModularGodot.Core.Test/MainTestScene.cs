using Godot;
using ModularGodot.Core.Test.Scenes;
using ModularGodot.Core.Test.Services;
using System;
using System.Collections.Generic;

[Tool]
public partial class MainTestScene : Node
{
    private TestRunner _testRunner;
    private TestConfiguration _testConfiguration;
    private TestReporter _testReporter;
    private TestLogger _testLogger;

    public override void _Ready()
    {
        _testLogger = new TestLogger(_testConfiguration);
        _testLogger.LogInfo("主测试场景初始化");
        InitializeTestInfrastructure();
    }

    /// <summary>
    /// 初始化测试基础设施
    /// </summary>
    private void InitializeTestInfrastructure()
    {
        try
        {
            _testConfiguration = TestConfiguration.Instance;
            _testRunner = new TestRunner();
            _testReporter = new TestReporter(_testConfiguration);

            _testLogger.LogInfo("测试基础设施初始化完成");
        }
        catch (Exception ex)
        {
            _testLogger.LogError($"测试基础设施初始化失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 运行所有测试
    /// </summary>
    public void RunAllTests()
    {
        _testLogger.LogInfo("开始运行所有测试");

        // 检查是否应该运行测试
        if (!_testConfiguration.ShouldRunTests())
        {
            _testLogger.LogWarning("测试未在开发环境中运行，跳过测试执行");
            return;
        }

        // 获取所有测试场景
        var testScenes = new List<Node>();

        // 尝试获取已加载的测试场景
        var mediatorTestScene = GetNodeOrNull<Node>("MediatorTestScene");
        if (mediatorTestScene != null)
        {
            testScenes.Add(mediatorTestScene);
        }

        var eventBusTestScene = GetNodeOrNull<Node>("EventBusTestScene");
        if (eventBusTestScene != null)
        {
            testScenes.Add(eventBusTestScene);
        }

        var packageTestScene = GetNodeOrNull<Node>("PackageTestScene");
        if (packageTestScene != null)
        {
            testScenes.Add(packageTestScene);
        }

        var testIsolationScene = GetNodeOrNull<Node>("TestIsolationScene");
        if (testIsolationScene != null)
        {
            testScenes.Add(testIsolationScene);
        }

        if (testScenes.Count == 0)
        {
            _testLogger.LogWarning("未找到测试场景，跳过测试执行");
            return;
        }

        _testLogger.LogInfo($"找到 {testScenes.Count} 个测试场景，开始运行测试");
        // 运行测试
        RunTestsAsync(testScenes);
    }

    /// <summary>
    /// 异步运行测试
    /// </summary>
    /// <param name="testScenes">测试场景列表</param>
    private async void RunTestsAsync(List<Node> testScenes)
    {
        try
        {
            _testLogger.LogInfo("开始异步运行测试");
            var results = await _testRunner.RunTestsAsync(testScenes);

            // 生成报告
            var report = _testReporter.GenerateDetailedReport(results);
            _testLogger.LogInfo($"测试完成 - 通过: {report.PassedTests}, 失败: {report.FailedTests}, 跳过: {report.SkippedTests}");

            // 保存报告
            var reportPath = "res://TestResults/report.md";
            _testReporter.SaveReportToFile(report, reportPath);
            _testLogger.LogInfo($"测试报告已保存到: {reportPath}");

            // 显示详细结果
            foreach (var result in results)
            {
                _testLogger.LogInfo($"测试 {result.SceneName}: {result.Status} - {result.Message}");
            }
        }
        catch (Exception ex)
        {
            _testLogger.LogError($"运行测试时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 重置所有测试
    /// </summary>
    public void ResetAllTests()
    {
        _testLogger.LogInfo("重置所有测试");

        // 重置各个测试场景
        ResetTestScene("MediatorTestScene");
        ResetTestScene("EventBusTestScene");
        ResetTestScene("PackageTestScene");
        ResetTestScene("TestIsolationScene");

        // 清除测试结果
        _testRunner?.ClearTestResults();

        _testLogger.LogInfo("所有测试已重置");
    }

    /// <summary>
    /// 重置指定的测试场景
    /// </summary>
    /// <param name="sceneName">场景名称</param>
    private void ResetTestScene(string sceneName)
    {
        var testScene = GetNodeOrNull<Node>(sceneName);
        if (testScene != null)
        {
            var resetMethod = testScene.GetType().GetMethod("ResetTest");
            if (resetMethod != null)
            {
                resetMethod.Invoke(testScene, null);
                _testLogger.LogDebug($"重置测试场景: {sceneName}");
            }
        }
    }
}