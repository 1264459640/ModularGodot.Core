using Godot;
using ModularGodot.Core.Test.Scenes;
using System.Diagnostics;
using ModularGodot.Core.AutoLoads;
using ModularGodot.Core.Contracts.Abstractions.Messaging;
using System;
using System.Threading.Tasks;

// Define a simple event for testing purposes
public class TestEvent : EventBase
{
    public string Message { get; }

    public TestEvent(string message) : base(nameof(TestEvent))
    {
        Message = message;
    }
}

public partial class EventBusTestScene : BaseTestScene
{
    private const string TestMessage = "EventBusTest";

    public override void _Ready()
    {
        TestDescription = "验证事件总线的同步发布和订阅";
        base._Ready();
        GD.Print("EventBusTestScene ready");
    }

    protected override void ExecuteTest(Stopwatch stopwatch)
    {
        GD.Print("--- Starting Synchronous EventBus Test ---", _sceneName);

        var eventBus = MiddlewareProvider.Instance.ResolveEventBus();
        if (eventBus == null)
        {
            _testResult = CreateFailureResult("Failed to resolve EventBus service", stopwatch);
            GD.PrintErr("Test failed: Could not resolve EventBus service", _sceneName);
            return;
        }

        GD.Print("EventBus service resolved successfully", _sceneName);
        IDisposable subscription = null;
        try
        {
            subscription = eventBus.Subscribe<TestEvent>(HandleTestEvent);
            GD.Print("Subscribed to TestEvent.", _sceneName);

            var testEvent = new TestEvent(TestMessage);
            GD.Print("Publishing TestEvent synchronously...", _sceneName);
            eventBus.Publish(testEvent);
            GD.Print("Synchronous Publish call finished.", _sceneName);

            
        }
        catch (Exception ex)
        {
             _testResult = CreateFailureResult($"An exception occurred: {ex.Message}", stopwatch);
             GD.PrintErr($"Test failed with exception: {ex}", _sceneName);
        }
        finally
        {
            subscription?.Dispose();
            GD.Print("Unsubscribed from TestEvent.", _sceneName);
            GD.Print("--- Finished Synchronous EventBus Test ---", _sceneName);
        }
    }

    private void HandleTestEvent(TestEvent @event)
    {
        GD.Print($"Received TestEvent with message: '{@event.Message}'", _sceneName);

    }
}
