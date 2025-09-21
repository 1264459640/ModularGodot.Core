using Autofac;
using ModularGodot.Contracts;

namespace MF.Context;

public class Contexts : LazySingleton<Contexts>
{
    private IContainer Container { get; init; }

    public Contexts()
    {
        var builder = new ContainerBuilder();

        // 注册自动扫描模块

        builder.RegisterModule<SingleModule>();
        builder.RegisterModule<MediatorModule>();


        Container = builder.Build();

    }

    /// <summary>
    /// 从容器中解析服务
    /// </summary>
    /// <typeparam name="T">服务类型</typeparam>
    /// <returns>服务实例</returns>
    public T ResolveService<T>() where T : class
    {
        return Container.Resolve<T>();
    }
    
    /// <summary>
    /// 尝试从容器中解析服务
    /// </summary>
    /// <typeparam name="T">服务类型</typeparam>
    /// <param name="service">输出的服务实�?/param>
    /// <returns>是否解析成功</returns>
    public bool TryResolveService<T>(out T? service) where T : class
    {
        return Container.TryResolve(out service);
    }
    
    /// <summary>
    /// 检查服务是否已注册
    /// </summary>
    /// <typeparam name="T">服务类型</typeparam>
    /// <returns>是否已注�?/returns>
    public bool IsServiceRegistered<T>() where T : class
    {
        return Container.IsRegistered<T>();
    }

}
