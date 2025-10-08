using Godot;
using ModularGodot.Core.Contracts.Abstractions.Messaging;
using System.Threading.Tasks;
using ModularGodot.Core.AutoLoads;

namespace ModularGodot.Core.Examples;

/// <summary>
/// 使用全局服务的示例节点
/// </summary>
public partial class ServiceUsageExample : Node
{
    private IDispatcher _dispatcher;
    private IEventBus _eventBus;

    public override void _Ready()
    {
        // 获取全局服务实例
        var globalService = MiddlewareProvider.Instance;

        if (globalService != null)
        {
            // 解析核心服务
            _dispatcher = globalService.ResolveDispatcher();
            _eventBus = globalService.ResolveEventBus();

            if (_dispatcher != null)
            {
                GD.Print("Successfully resolved IDispatcher");
            }

            if (_eventBus != null)
            {
                GD.Print("Successfully resolved IEventBus");
            }
        }
        else
        {
            GD.PrintErr("MiddlewareProvider instance not found");
        }
    }

    /// <summary>
    /// 示例：发送命令
    /// </summary>
    private async Task SendExampleCommand()
    {
        if (_dispatcher == null) return;

        try
        {
            // 示例命令发送（需要根据实际命令类型调整）
            // var command = new ExampleCommand();
            // var result = await _dispatcher.Send(command);
            // GD.Print($"Command result: {result}");
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"Error sending command: {ex.Message}");
        }
    }

    /// <summary>
    /// 示例：发布事件
    /// </summary>
    private void PublishExampleEvent()
    {
        if (_eventBus == null) return;

        try
        {
            // 示例事件发布（需要根据实际事件类型调整）
            // var event = new ExampleEvent();
            // _eventBus.Publish(event);
            // GD.Print("Event published successfully");
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"Error publishing event: {ex.Message}");
        }
    }
}