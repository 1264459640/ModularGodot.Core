using Godot;
using ModularGodot.Core.Test.Scenes;
using System.Diagnostics;
using ModularGodot.Core.AutoLoads;
using ModularGodot.Core.Contracts.Abstractions.Messaging;
using System.Threading.Tasks;
using System.Threading;

// Define a simple command for testing purposes
public class TestCommand : ICommand<bool>
{
    public bool Handled { get; set; } = false;
}

// Define a handler for the test command
public class TestCommandHandler : ICommandHandler<TestCommand, bool>
{
    public Task<bool> Handle(TestCommand command, CancellationToken cancellationToken)
    {
        command.Handled = true;
        GD.Print("TestCommandHandler executed successfully.");
        return Task.FromResult(true);
    }
}

[Tool]
public partial class MediatorCommandTestScene : BaseTestScene
{
    public override void _Ready()
    {
        TestDescription = "验证中介者命令发送和处理";
        base._Ready();
        GD.Print("MediatorCommandTestScene ready");
    }

    protected override async void ExecuteTest(Stopwatch stopwatch)
    {
        GD.Print("Attempting to resolve Mediator service", _sceneName);
        var dispatcher = MiddlewareProvider.Instance.ResolveDispatcher();
        if (dispatcher == null)
        {
            _testResult = CreateFailureResult("Failed to resolve Mediator service", stopwatch);
            GD.PrintErr("Test failed: Could not resolve Mediator service", _sceneName);
            return;
        }

        GD.Print("Mediator service resolved successfully", _sceneName);

        // Create a new command instance
        var command = new TestCommand();

        // Send the command
        GD.Print("Sending TestCommand...", _sceneName);
        var result = await dispatcher.Send(command);
        GD.Print($"TestCommand sent. Handled status: {command.Handled}, Result: {result}", _sceneName);

        if (command.Handled && result)
        {
            _testResult = CreateSuccessResult("Mediator command test passed", stopwatch);
        }
        else
        {
            _testResult = CreateFailureResult("Mediator command was not handled correctly", stopwatch);
            GD.PrintErr("Test failed: Command was not handled by its handler or returned an incorrect result.", _sceneName);
        }
    }
}
