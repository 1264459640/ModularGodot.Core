using Godot;
using ModularGodot.Core.Test.Models;
using ModularGodot.Core.Test.Scenes;
using System;
using System.Diagnostics;
using System.Reflection;

[Tool]
public partial class PackageTestScene : BaseTestScene
{
    public override void _Ready()
    {
        TestDescription = "验证所有NuGet包的完整性";
        base._Ready();
        GD.Print("PackageTestScene ready");
    }

    /// <summary>
    /// 执行包完整性测试的具体逻辑
    /// </summary>
    /// <param name="stopwatch">计时器</param>
    protected override void ExecuteTest(Stopwatch stopwatch)
    {
        GD.Print("开始检查必需的程序集", _sceneName);
        // 检查必需的程序集是否已加载
        var requiredAssemblies = new[]
        {
            "ModularGodot.Core.Contracts",
            "ModularGodot.Core.Contexts",
            "ModularGodot.Core.Infrastructure",
            "ModularGodot.Core.Repositories",
            "ModularGodot.Core"
        };

        var missingAssemblies = new System.Collections.Generic.List<string>();

        foreach (var assemblyName in requiredAssemblies)
        {
            try
            {
                var assembly = Assembly.Load(assemblyName);
                if (assembly == null)
                {
                    missingAssemblies.Add(assemblyName);
                    GD.PrintErr($"程序集未找到: {assemblyName}", _sceneName);
                }
                else
                {
                    GD.Print($"程序集加载成功: {assemblyName} v{assembly.GetName().Version}", _sceneName);
                }
            }
            catch (Exception ex)
            {
                missingAssemblies.Add(assemblyName);
                GD.PrintErr($"程序集加载失败: {assemblyName} - {ex.Message}", _sceneName);
            }
        }

        if (missingAssemblies.Count > 0)
        {
            _testResult = CreateFailureResult($"缺少以下程序集: {string.Join(", ", missingAssemblies)}", stopwatch);
            GD.PrintErr($"测试失败: 缺少程序集 - {string.Join(", ", missingAssemblies)}", _sceneName);
        }
        else
        {
            _testResult = CreateSuccessResult("所有必需的NuGet包都已正确加载", stopwatch);
            _testExecuted = true;
        }
    }
}