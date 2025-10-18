using Godot;
using ModularGodot.Core.Contracts.Abstractions.Messaging;


namespace ModularGodot.Core.AutoLoads;

/// <summary>
/// Godot全局服务节点，提供从依赖注入容器解析核心服务的方法
/// </summary>
public partial class MiddlewareProvider : Node
{
    private static MiddlewareProvider _instance;
    private Contexts.Contexts _contexts;
    private bool _initialized = false;

    /// <summary>
    /// 获取全局服务实例
    /// </summary>
    public static MiddlewareProvider Instance => _instance;

    /// <summary>
    /// 当场景加载时调用
    /// </summary>
    public override void _Ready()
    {
        // Immediately initialize the DI container upon readiness.
        // This triggers the potentially blocking assembly loading at a safe time.
        Initialize();

        if (_instance == null)
        {
            _instance = this;
            // 不要释放这个节点，让它在整个应用程序生命周期中存在
            ProcessMode = ProcessModeEnum.Always;
        }
        else if (_instance != this)
        {
            // 如果已经有实例，移除这个重复的节点
            QueueFree();
            return;
        }
    }

    /// <summary>
    /// 初始化依赖注入容器
    /// </summary>
    private void Initialize()
    {
        if (_initialized) return;

        try
        {
            // 初始化应用程序上下文
            _contexts = Contexts.Contexts.Instance;
            _initialized = true;
            GD.Print("MiddlewareProvider initialized successfully");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to initialize MiddlewareProvider: {ex.Message}");
        }
    }

    /// <summary>
    /// 从依赖注入容器解析中介者接口
    /// </summary>
    /// <returns>IDispatcher实例</returns>
    public IDispatcher ResolveDispatcher()
    {
        if (!_initialized)
        {
            GD.PrintErr("MiddlewareProvider not initialized. Call Initialize() first.");
            return null;
        }

        try
        {
            return _contexts.ResolveService<IDispatcher>();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to resolve IDispatcher: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 从依赖注入容器解析事件总线接口
    /// </summary>
    /// <returns>IEventBus实例</returns>
    public IEventBus ResolveEventBus()
    {
        if (!_initialized)
        {
            GD.PrintErr("MiddlewareProvider not initialized. Call Initialize() first.");
            return null;
        }

        try
        {
            return _contexts.ResolveService<IEventBus>();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to resolve IEventBus: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 尝试解析中介者接口
    /// </summary>
    /// <param name="dispatcher">输出参数，返回解析的IDispatcher实例（如果成功）</param>
    /// <returns>如果成功解析则返回true，否则返回false</returns>
    public bool TryResolveDispatcher(out IDispatcher dispatcher)
    {
        dispatcher = null;
        if (!_initialized) return false;

        try
        {
            return _contexts.TryResolveService(out dispatcher);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 尝试解析事件总线接口
    /// </summary>
    /// <param name="eventBus">输出参数，返回解析的IEventBus实例（如果成功）</param>
    /// <returns>如果成功解析则返回true，否则返回false</returns>
    public bool TryResolveEventBus(out IEventBus eventBus)
    {
        eventBus = null;
        if (!_initialized) return false;

        try
        {
            return _contexts.TryResolveService(out eventBus);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 检查中介者服务是否已注册
    /// </summary>
    /// <returns>如果服务已注册则返回true，否则返回false</returns>
    public bool IsDispatcherRegistered()
    {
        if (!_initialized) return false;
        return _contexts.IsServiceRegistered<IDispatcher>();
    }

    /// <summary>
    /// 检查事件总线服务是否已注册
    /// </summary>
    /// <returns>如果服务已注册则返回true，否则返回false</returns>
    public bool IsEventBusRegistered()
    {
        if (!_initialized) return false;
        return _contexts.IsServiceRegistered<IEventBus>();
    }

    /// <summary>
    /// 当节点即将被释放时调用
    /// </summary>
    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            // 如果这是实例节点，清除实例引用
            if (_instance == this)
            {
                _instance = null;
            }

            // 清理资源
            _contexts?.Dispose();
            _contexts = null;
            _initialized = false;
        }
    }
}