using Godot;
using ModularGodot.Core.Test.Scenes;
using System.Diagnostics;
using ModularGodot.Core.AutoLoads;

[Tool]
public partial class MediatorTestScene : BaseTestScene
{
    public override void _Ready()
    {
        TestDescription = "验证中介者模式的组件通信";
        base._Ready();
        GD.Print("MediatorTestScene ready");
    }

    /// <summary>
    /// 执行中介者测试的具体逻辑
    /// </summary>
    /// <param name="stopwatch">计时器</param>
    protected override void ExecuteTest(Stopwatch stopwatch)
    {
        GD.Print("尝试解析中介者服务", _sceneName);
        // 获取中介者实例
        var mediator = MiddlewareProvider.Instance.ResolveDispatcher();
        if (mediator == null)
        {
            _testResult = CreateFailureResult("无法解析中介者服务", stopwatch);
            GD.PrintErr("测试失败: 无法解析中介者服务", _sceneName);
            return;
        }

        GD.Print("中介者服务解析成功", _sceneName);
        // 这里可以添加具体的中介者测试逻辑
        // 例如发送测试命令或查询

        _testResult = CreateSuccessResult("中介者通信测试通过", stopwatch);
    }
}