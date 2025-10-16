using Godot;
using ModularGodot.Core.Test.Models;
using ModularGodot.Core.Test.Scenes;
using System;
using System.Diagnostics;

[Tool]
public partial class TestIsolationScene : BaseTestScene
{
    private static int _testInstanceCount = 0;
    private int _instanceId;

    public override void _Ready()
    {
        _instanceId = Interlocked.Increment(ref _testInstanceCount);
        TestDescription = "验证测试隔离和副作用预防";
        base._Ready();
        GD.Print($"TestIsolationScene 实例 {_instanceId} ready", _sceneName);
    }

    /// <summary>
    /// 执行测试隔离测试的具体逻辑
    /// </summary>
    /// <param name="stopwatch">计时器</param>
    protected override void ExecuteTest(Stopwatch stopwatch)
    {
        GD.Print($"实例 {_instanceId} 开始执行测试隔离测试", _sceneName);   

        // 验证测试隔离
        // 检查是否有其他实例正在运行
        if (_testInstanceCount > 1)
        {
            GD.PrintErr($"检测到多个测试实例同时运行: {_testInstanceCount}", _sceneName);
        }

        GD.Print($"模拟测试操作", _sceneName);
        // 模拟一些测试操作
        // 这里可以添加具体的测试隔离验证逻辑
        Thread.Sleep(10); // 模拟一些工作

        _testResult = CreateSuccessResult($"测试隔离验证通过 (实例 {_instanceId})", stopwatch);
        _testExecuted = true;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            // 清理资源
            GD.Print($"TestIsolationScene 实例 {_instanceId} 被释放", _sceneName);
        }
    }
}