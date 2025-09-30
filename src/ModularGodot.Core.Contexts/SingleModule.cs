using Autofac;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ModularGodot.Contracts.Attributes;
// Add this using
using System.Reflection;

namespace ModularGodot.Contexts
{
    public class SingleModule : Autofac.Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            // 注册 IOptions<MemoryCacheOptions>
            builder.RegisterInstance(Options.Create(new MemoryCacheOptions()))
                   .As<IOptions<MemoryCacheOptions>>()
                   .SingleInstance();

            // 注册 MemoryCache
            builder.RegisterType<MemoryCache>()
                   .As<IMemoryCache>()
                   .SingleInstance();

            // 先主动加载所有引用的程序集
            LoadAllReferencedAssemblies();

            // 然后获取所有已加载的程序集进行扫描
            var assembliesToScan = AppDomain.CurrentDomain.GetAssemblies().ToList();

            builder.RegisterAssemblyTypes(assembliesToScan.Distinct().ToArray())
                .Where(t => t.GetCustomAttribute<InjectableAttribute>() != null)
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();
        }

        private void LoadAllReferencedAssemblies()
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var assembliesToCheck = new List<Assembly>(loadedAssemblies);

            // 主动加载测试程序集（如果存在）
            try
            {
                var testAssemblyPath = "D:/GodotProjects/ModularGodot.Core/src/ModularGodot.Core.Tests/bin/Debug/net9.0/ModularGodot.Core.Tests.dll";
                if (System.IO.File.Exists(testAssemblyPath))
                {
                    var testAssembly = Assembly.LoadFrom(testAssemblyPath);
                    if (!loadedAssemblies.Contains(testAssembly))
                    {
                        loadedAssemblies.Add(testAssembly);
                        assembliesToCheck.Add(testAssembly);
                    }
                }
            }
            catch { /* 忽略加载失败的程序集 */ }

            foreach (Assembly assembly in assembliesToCheck)
            {
                foreach (AssemblyName refName in assembly.GetReferencedAssemblies())
                {
                    try
                    {
                        var referencedAssembly = Assembly.Load(refName);
                        if (!loadedAssemblies.Contains(referencedAssembly))
                        {
                            loadedAssemblies.Add(referencedAssembly);
                        }
                    }
                    catch { /* 忽略加载失败的程序集 */ }
                }
            }
        }
    }
}