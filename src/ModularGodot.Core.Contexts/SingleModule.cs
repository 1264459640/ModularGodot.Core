using Autofac;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ModularGodot.Contracts.Attributes;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

            // 根据 InjectableAttribute 特性注册类型，并根据 Lifetime 设置生命周期
            foreach (var assembly in assembliesToScan.Distinct())
            {
                foreach (var type in assembly.GetTypes())
                {
                    var injectableAttribute = type.GetCustomAttribute<InjectableAttribute>();
                    if (injectableAttribute != null)
                    {
                        var registration = builder.RegisterType(type)
                            .AsSelf()
                            .AsImplementedInterfaces();

                        // 根据特性中的生命周期域进行注册
                        switch (injectableAttribute.Lifetime)
                        {
                            case Lifetime.Transient:
                                registration.InstancePerDependency();
                                break;
                            case Lifetime.Scoped:
                                registration.InstancePerLifetimeScope();
                                break;
                            case Lifetime.Singleton:
                                registration.SingleInstance();
                                break;
                        }
                    }
                }
            }
        }

        private void LoadAllReferencedAssemblies()
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var assembliesToCheck = new List<Assembly>(loadedAssemblies);

            // 从配置获取程序集路径并加载
            var assemblyPaths = AssemblyLoadingConfiguration.GetAssemblyPathsFromConfiguration();
            foreach (var assemblyPath in assemblyPaths)
            {
                try
                {
                    if (File.Exists(assemblyPath))
                    {
                        var assembly = Assembly.LoadFrom(assemblyPath);
                        if (!loadedAssemblies.Contains(assembly))
                        {
                            loadedAssemblies.Add(assembly);
                            assembliesToCheck.Add(assembly);
                        }
                    }
                }
                catch
                {
                    // 忽略加载失败的程序集，记录日志（如果需要）
                }
            }

            // 加载引用的程序集
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