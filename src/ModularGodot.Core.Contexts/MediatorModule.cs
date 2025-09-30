using Autofac;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using ModularGodot.Contracts.Abstractions.Messaging;
using ModularGodot.Infrastructure.Messaging;
using System.Reflection;

namespace ModularGodot.Contexts;

public class MediatorModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // 确保加载测试程序集
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

        // 主动加载测试程序集（如果尚未加载）
        try
        {
            var testAssemblyPath = "D:/GodotProjects/ModularGodot.Core/src/ModularGodot.Core.Tests/bin/Debug/net9.0/ModularGodot.Core.Tests.dll";
            if (System.IO.File.Exists(testAssemblyPath))
            {
                var testAssembly = Assembly.LoadFrom(testAssemblyPath);
                if (!loadedAssemblies.Contains(testAssembly))
                {
                    loadedAssemblies.Add(testAssembly);
                }
            }
        }
        catch
        {
            // 如果无法加载测试程序集，继续使用已加载的程序集
        }

        var assembliesToScan = loadedAssemblies.Distinct().ToList();

        builder.RegisterType<MediatRAdapter>()
            .As<IDispatcher>()
            .InstancePerLifetimeScope();

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