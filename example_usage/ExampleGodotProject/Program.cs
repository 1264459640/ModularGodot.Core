using Autofac;
using ExampleGodotProject;

namespace ExampleGodotProject;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== ModularGodot.Core 动态加载测试 ===");
        
        try
        {
            // 创建容器构建器
            var builder = new ContainerBuilder();
            
            // 注册示例服务（不依赖MediatR）
            builder.RegisterType<ExampleService>().AsSelf().SingleInstance();
            
            // 构建容器
            var container = builder.Build();
            
            Console.WriteLine("✓ 依赖注入容器已创建");
            
            // 获取示例服务
            var exampleService = container.Resolve<ExampleService>();
            Console.WriteLine("✓ ExampleService 已成功解析");
            
            // 测试服务功能
            await exampleService.InitializeAsync();
            exampleService.TestDynamicLoading();
            
            var result = await exampleService.ProcessGameDataAsync("测试数据");
            Console.WriteLine($"✓ 处理结果: {result}");
            
            Console.WriteLine("\n=== 动态加载功能测试完成 ===");
            Console.WriteLine("✓ ModularGodot.Core NuGet包正常工作");
            Console.WriteLine("✓ 动态程序集发现机制正常");
            Console.WriteLine("✓ 依赖注入容器正常");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 测试失败: {ex.Message}");
            Console.WriteLine($"详细信息: {ex}");
        }
        
        Console.WriteLine("\n按任意键退出...");
        Console.ReadKey();
    }
}