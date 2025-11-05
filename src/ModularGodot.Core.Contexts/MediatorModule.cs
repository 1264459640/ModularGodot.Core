using System.Reflection;
using Autofac;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using ModularGodot.Core.Contracts.Abstractions.Messaging;
using ModularGodot.Core.Infrastructure.Logging;
using ModularGodot.Core.Infrastructure.Messaging;

namespace ModularGodot.Core.Contexts;

public class MediatorModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // 确保加载测试程序集
        var assembliesToScan = AppDomain.CurrentDomain.GetAssemblies().ToList();

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