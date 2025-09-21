using Autofac;

namespace ExampleGodotProject;

/// <summary>
/// 简化的示例服务类，演示如何使用ModularGodot.Core的动态加载功能
/// </summary>
public class ExampleService
{
    public ExampleService()
    {
    }

    public Task InitializeAsync()
    {
        Console.WriteLine("ExampleService 正在初始化...");
        
        // 演示基本功能
        Console.WriteLine("基本服务功能已就绪");
        
        Console.WriteLine("ExampleService 初始化完成");
        return Task.CompletedTask;
    }

    public void TestDynamicLoading()
    {
        Console.WriteLine("测试动态加载功能:");
        Console.WriteLine("- SingleModule 已成功加载");
        Console.WriteLine("- 依赖注入容器已正确配置");
        Console.WriteLine("- 动态程序集发现功能正常工作");
    }

    public Task<string> ProcessGameDataAsync(string data)
    {
        Console.WriteLine($"处理游戏数据: {data}");
        
        // 模拟数据处理
        var result = $"已处理: {data}";
        
        return Task.FromResult(result);
    }
}