using Autofac;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using ModularGodot.Contracts.Abstractions.Messaging;
using ModularGodot.Infrastructure.Messaging;

namespace ModularGodot.Contexts;

public class MediatorModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var assembliesToScan = AppDomain.CurrentDomain.GetAssemblies()
            .ToList();
            
        builder.RegisterType<MediatRAdapter>()
            .As<IDispatcher>()
            .InstancePerLifetimeScope();
        // 2. 注册所有 ICommandHandler<T, TRes> 和 IQueryHandler<T, TRes>
        foreach (var assembly in assembliesToScan)
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

        // 4. 注册包装处理器（开放泛型）
        builder.RegisterGeneric(typeof(CommandHandlerWrapper<,>))
            .As(typeof(IRequestHandler<,>))
            .InstancePerLifetimeScope();
        builder.RegisterGeneric(typeof(QueryHandlerWrapper<,>))
            .As(typeof(IRequestHandler<,>))
            .InstancePerLifetimeScope();
            
        // 注册 MediatR 内部处理器
        builder.RegisterAssemblyTypes(typeof(IMediator).Assembly)
            .AsImplementedInterfaces();

    }
}