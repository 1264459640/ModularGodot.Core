using System;
using System.IO;
using Autofac;
using MF.Context;

namespace ModularGodot.Core.Examples
{
    /// <summary>
    /// 动态DLL加载使用示例
    /// </summary>
    public class DynamicLoadingExample
    {
        /// <summary>
        /// 示例1：使用默认搜索路径和模式
        /// </summary>
        public static void ExampleWithDefaults()
        {
            Console.WriteLine("=== 使用默认配置的动态加载示例 ===");
            
            var builder = new ContainerBuilder();
            
            // 使用默认构造函数，会自动搜索以下路径：
            // - 当前目录
            // - bin文件夹
            // - lib文件夹  
            // - modules文件夹
            // - plugins文件夹
            builder.RegisterModule<SingleModule>();
            
            var container = builder.Build();
            
            Console.WriteLine("容器构建完成，已自动发现并加载DLL文件");
        }

        /// <summary>
        /// 示例2：自定义搜索路径和文件模式
        /// </summary>
        public static void ExampleWithCustomPaths()
        {
            Console.WriteLine("=== 使用自定义路径的动态加载示例 ===");
            
            var builder = new ContainerBuilder();
            
            // 自定义搜索路径
            var customSearchPaths = new[]
            {
                AppDomain.CurrentDomain.BaseDirectory,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "custom_modules"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extensions"),
                @"C:\MyModules" // 绝对路径示例
            };
            
            // 自定义文件名模式
            var customPatterns = new[]
            {
                "*.Services.dll",
                "*.Repositories.dll",
                "*.Infrastructure.dll",
                "MyApp.*.dll",
                "Plugin*.dll"
            };
            
            // 使用自定义配置创建模块
            var customModule = new SingleModule(customSearchPaths, customPatterns);
            builder.RegisterModule(customModule);
            
            var container = builder.Build();
            
            Console.WriteLine("容器构建完成，已从自定义路径加载DLL文件");
        }

        /// <summary>
        /// 示例3：在NuGet包中的使用方式
        /// </summary>
        public static void ExampleInNuGetPackage()
        {
            Console.WriteLine("=== NuGet包中的动态加载示例 ===");
            
            var builder = new ContainerBuilder();
            
            // 在NuGet包环境中，通常DLL文件会被复制到输出目录
            // 默认配置已经包含了常见的搜索路径，包括当前目录和bin文件夹
            
            // 如果需要特定的搜索路径，可以这样配置：
            var nugetSearchPaths = new[]
            {
                AppDomain.CurrentDomain.BaseDirectory, // 主程序目录
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runtimes"), // NuGet运行时目录
                Environment.CurrentDirectory // 当前工作目录
            };
            
            var nugetPatterns = new[]
            {
                "MF.*.dll", // 匹配MF开头的所有DLL
                "*Services*.dll",
                "*Repositories*.dll", 
                "*Infrastructure*.dll"
            };
            
            var nugetModule = new SingleModule(nugetSearchPaths, nugetPatterns);
            builder.RegisterModule(nugetModule);
            
            var container = builder.Build();
            
            Console.WriteLine("NuGet包环境下的容器构建完成");
        }

        /// <summary>
        /// 示例4：Godot项目中的使用方式
        /// </summary>
        public static void ExampleInGodotProject()
        {
            Console.WriteLine("=== Godot项目中的动态加载示例 ===");
            
            var builder = new ContainerBuilder();
            
            // 在Godot项目中，DLL通常位于项目的.godot/mono/temp/bin目录
            // 或者项目根目录的bin文件夹中
            var godotSearchPaths = new[]
            {
                AppDomain.CurrentDomain.BaseDirectory,
                Path.Combine(Environment.CurrentDirectory, "bin"),
                Path.Combine(Environment.CurrentDirectory, ".godot", "mono", "temp", "bin", "Debug"),
                Path.Combine(Environment.CurrentDirectory, ".godot", "mono", "temp", "bin", "Release")
            };
            
            // Godot项目的DLL模式
            var godotPatterns = new[]
            {
                "*.Services.dll",
                "*.Repositories.dll",
                "*.Infrastructure.dll",
                "MF.*.dll"
            };
            
            var godotModule = new SingleModule(godotSearchPaths, godotPatterns);
            builder.RegisterModule(godotModule);
            
            var container = builder.Build();
            
            Console.WriteLine("Godot项目环境下的容器构建完成");
        }
    }
}