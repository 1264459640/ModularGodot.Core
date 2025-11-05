using System.Reflection;
using Autofac;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ModularGodot.Core.Contracts.Attributes;
using ModularGodot.Core.Infrastructure.Logging;

namespace ModularGodot.Core.Contexts;

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

   
}