using Godot;
using ModularGodot.Core.AutoLoads;
using ModularGodot.Core.Contracts.Abstractions;


namespace ModularGodot.Core.Abstractions
{
    /// <summary>
    /// ServiceHostNode: 负责将 Godot 物理层的资源“注入”到纯 C# Service 中
    /// TService: 目标服务的接口
    /// TConfig: 服务所需的配置数据类型 
    /// </summary>
    public abstract partial class ServiceHostNode<T, TService, TConfig> : EagerSingletonNode<T>
        where T : ServiceHostNode<T, TService, TConfig>
        where TService : class
    {
        protected TService Service { get; private set; }

        public override void _Ready()
        {
            // 1. 自动解析服务
            if (!MiddlewareProvider.Instance.TryResolveService<TService>(out var service))
            {
                GD.PrintErr($"ServiceHostNode: Failed to resolve service {typeof(TService).Name}");
                return;
            }
            Service = service;

            // 2. 构建配置对象 (由子类实现具体构建逻辑)
            var config = BuildConfiguration();

            // 3. 将配置应用到服务 (使用反射或约定，或者强制服务实现 IConfigurable<TConfig>)
            // 这里为了通用性，我们通过抽象方法让子类调用具体的 Configure 方法
            ConfigureService(Service, config);
            
            GD.Print($"ServiceHostNode: Configured {typeof(TService).Name}");
        }

        public override void _ExitTree()
        {
            // 4. 自动处理清理 (可选)
            OnServiceDetaching(Service);
            base._ExitTree();
        }

        /// <summary>
        /// 子类需实现：收集场景树中的节点，构建配置对象
        /// </summary>
        protected abstract TConfig BuildConfiguration();

        /// <summary>
        /// 子类需实现：调用 Service 的具体配置方法
        /// </summary>
        protected abstract void ConfigureService(TService service, TConfig config);

        /// <summary>
        /// 可选：清理逻辑
        /// </summary>
        protected virtual void OnServiceDetaching(TService service) { }
    }
}