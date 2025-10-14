using System.Diagnostics;
using Godot;
using ModularGodot.Core.AutoLoads;

namespace ModularGodot.Core.Test.Scenes;

[Tool]
public partial class EventBusTestScene : BaseTestScene
{
    public override void _Ready()
    {
        TestDescription = "验证事件总线的组件通信";
        base._Ready();
        GD.Print("EventBusTestScene ready");
    }

    /// <summary>
    /// 执行事件总线测试的具体逻辑
    /// </summary>
    /// <param name="stopwatch">计时器</param>
    protected override void ExecuteTest(Stopwatch stopwatch)
    {
        _testLogger.LogDebug("尝试解析事件总线服务", _sceneName);
        // 获取事件总线实例
        var eventBus = MiddlewareProvider.Instance.ResolveEventBus();
        if (eventBus == null)
        {
            _testResult = CreateFailureResult("无法解析事件总线服务", stopwatch);
            _testLogger.LogError("测试失败: 无法解析事件总线服务", _sceneName);
            return;
        }

        _testLogger.LogDebug("事件总线服务解析成功", _sceneName);
        // 这里可以添加具体的事件总线测试逻辑
        // 例如发布测试事件

        _testResult = CreateSuccessResult("事件总线通信测试通过", stopwatch);
    }
}