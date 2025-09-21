using System.Reflection;
using Autofac;
using MF.Contracts.Attributes;
using Microsoft.Extensions.Caching.Memory;
using Module = Autofac.Module;

namespace MF.Context;

public class SingleModule : Module
{
    private readonly string[] _assemblySearchPaths;
    private readonly string[] _assemblyPatterns;

    /// <summary>
    /// 默认构造函数，使用默认的程序集搜索路径和模式
    /// </summary>
    public SingleModule() : this(GetDefaultSearchPaths(), GetDefaultAssemblyPatterns())
    {
    }

    /// <summary>
    /// 自定义构造函数，允许指定程序集搜索路径和模式
    /// </summary>
    /// <param name="assemblySearchPaths">程序集搜索路径数组</param>
    /// <param name="assemblyPatterns">程序集文件名模式数组</param>
    public SingleModule(string[] assemblySearchPaths, string[] assemblyPatterns)
    {
        _assemblySearchPaths = assemblySearchPaths ?? GetDefaultSearchPaths();
        _assemblyPatterns = assemblyPatterns ?? GetDefaultAssemblyPatterns();
    }

    protected override void Load(ContainerBuilder builder)
    {
        try
        {
            // 显式注册 IMemoryCache（MemoryCacheService 依赖）
            builder.RegisterInstance(new MemoryCache(new MemoryCacheOptions()))
                .As<IMemoryCache>()
                .SingleInstance();

            // 动态发现并加载程序集
            var discoveredAssemblies = DiscoverAssemblies();
            
            foreach (var assembly in discoveredAssemblies)
            {
                try
                {
                    RegisterAssemblyTypes(assembly, builder);
                    System.Diagnostics.Debug.WriteLine($"Successfully loaded assembly: {assembly.GetName().Name}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to register types from assembly {assembly.GetName().Name}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Assembly loading failed: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取默认的程序集搜索路径
    /// </summary>
    private static string[] GetDefaultSearchPaths()
    {
        var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        return new[]
        {
            currentDirectory,
            Path.Combine(currentDirectory, "bin"),
            Path.Combine(currentDirectory, "lib"),
            Path.Combine(currentDirectory, "modules"),
            Path.Combine(currentDirectory, "plugins")
        };
    }

    /// <summary>
    /// 获取默认的程序集文件名模式
    /// </summary>
    private static string[] GetDefaultAssemblyPatterns()
    {
        return new[]
        {
            "MF.Services*.dll",
            "MF.Repositories*.dll", 
            "MF.Infrastructure*.dll",
            "*Services.dll",
            "*Repositories.dll",
            "*Infrastructure.dll"
        };
    }

    /// <summary>
    /// 动态发现程序集
    /// </summary>
    private List<Assembly> DiscoverAssemblies()
    {
        var assemblies = new List<Assembly>();
        var loadedAssemblyNames = new HashSet<string>();

        foreach (var searchPath in _assemblySearchPaths)
        {
            if (!Directory.Exists(searchPath))
            {
                System.Diagnostics.Debug.WriteLine($"Search path does not exist: {searchPath}");
                continue;
            }

            foreach (var pattern in _assemblyPatterns)
            {
                try
                {
                    var files = Directory.GetFiles(searchPath, pattern, SearchOption.TopDirectoryOnly);
                    
                    foreach (var file in files)
                    {
                        try
                        {
                            var assemblyName = Path.GetFileNameWithoutExtension(file);
                            
                            // 避免重复加载同名程序集
                            if (loadedAssemblyNames.Contains(assemblyName))
                            {
                                continue;
                            }

                            // 尝试从文件加载程序集
                            var assembly = Assembly.LoadFrom(file);
                            
                            // 验证程序集是否包含我们需要的类型
                            if (IsValidAssembly(assembly))
                            {
                                assemblies.Add(assembly);
                                loadedAssemblyNames.Add(assemblyName);
                                System.Diagnostics.Debug.WriteLine($"Discovered assembly: {assemblyName} from {file}");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to load assembly from {file}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to search for pattern {pattern} in {searchPath}: {ex.Message}");
                }
            }
        }

        // 如果没有发现任何程序集，尝试使用传统的Assembly.Load方式作为后备
        if (assemblies.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine("No assemblies discovered via file system, falling back to Assembly.Load");
            assemblies.AddRange(LoadAssembliesUsingTraditionalMethod());
        }

        return assemblies;
    }

    /// <summary>
    /// 验证程序集是否包含我们需要的类型
    /// </summary>
    private bool IsValidAssembly(Assembly assembly)
    {
        try
        {
            // 检查程序集是否包含可注册的类型
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => !t.IsDefined(typeof(SkipRegistrationAttribute), false))
                .Where(t => !t.IsGenericTypeDefinition && !t.ContainsGenericParameters)
                .Where(t => !IsCompilerGeneratedType(t));

            return types.Any();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to validate assembly {assembly.GetName().Name}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 传统的程序集加载方法作为后备
    /// </summary>
    private List<Assembly> LoadAssembliesUsingTraditionalMethod()
    {
        var assemblies = new List<Assembly>();
        var assemblyNames = new[]
        {
            "MF.Services",
            "MF.Repositories", 
            "MF.Infrastructure"
        };

        foreach (var assemblyName in assemblyNames)
        {
            try
            {
                var assembly = Assembly.Load(assemblyName);
                assemblies.Add(assembly);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load assembly {assemblyName}: {ex.Message}");
            }
        }

        return assemblies;
    }
    
    /// <summary>
    /// 注册程序集中的所有类型
    /// </summary>
    private void RegisterAssemblyTypes(Assembly assembly, ContainerBuilder builder)
    {
        try
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => !t.IsDefined(typeof(SkipRegistrationAttribute), false))
                // 过滤开放泛型，仍包含未闭合类型参数的类型
                .Where(t => !t.IsGenericTypeDefinition && !t.ContainsGenericParameters)
                // 过滤编译器生成的类型（闭包类、状态机、匿名类型等）
                .Where(t => !IsCompilerGeneratedType(t));
            
            foreach (var type in types)
            {
                try
                {
                    builder.RegisterType(type)
                        .AsImplementedInterfaces()
                        .AsSelf()
                        .SingleInstance();
                    
                    System.Diagnostics.Debug.WriteLine($"Registered: {type.Name} with all implemented interfaces");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to register {type.Name}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to process assembly {assembly.FullName}: {ex.Message}");
        }
    }

    /// <summary>
    /// 检查类型是否为编译器生成的类型
    /// </summary>
    private bool IsCompilerGeneratedType(Type type)
    {
        var isCompilerGenerated = type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false);
        var name = type.Name;
        var looksCompilerGenerated = name.StartsWith("<") || name.Contains("DisplayClass") || name.Contains("d__") || name.Contains("AnonymousType");
        return isCompilerGenerated || looksCompilerGenerated;
    }
}
