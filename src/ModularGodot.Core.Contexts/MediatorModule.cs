using System.Reflection;
using Autofac;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using ModularGodot.Core.Contracts.Abstractions.Messaging;
using ModularGodot.Core.Infrastructure.Messaging;

namespace ModularGodot.Core.Contexts;

internal class MediatorModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // 确保加载测试程序集
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

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
                    }
                }
            }
            catch
            {
                // 如果无法加载程序集，继续使用已加载的程序集
            }
        }

        var assembliesToScan = loadedAssemblies.Distinct().ToList();

        // 注册所有 ICommandHandler<T, TRes> 和 IQueryHandler<T, TRes>
        foreach (var assembly in assembliesToScan)
        {
            try
            {
                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.IsClosedTypeOf(typeof(ICommandHandler<,>)) ||
                                t.IsClosedTypeOf(typeof(IQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();

                var configuration = MediatRConfigurationBuilder.Create("", assembly)
                    .WithAllOpenGenericHandlerTypesRegistered()
                    .Build();
                builder.RegisterMediatR(configuration);
            }
            catch
            {
                // 忽略无法处理的程序集
            }
        }

        // 注册 MediatR 内部处理器
        builder.RegisterAssemblyTypes(typeof(IMediator).Assembly)
            .AsImplementedInterfaces();

        // 注册包装处理器（开放泛型）
        builder.RegisterGeneric(typeof(CommandHandlerWrapper<,>))
            .As(typeof(IRequestHandler<,>))
            .InstancePerLifetimeScope();
        builder.RegisterGeneric(typeof(QueryHandlerWrapper<,>))
            .As(typeof(IRequestHandler<,>))
            .InstancePerLifetimeScope();
    }
}