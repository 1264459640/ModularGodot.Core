using Godot;

namespace ModularGodot.Core.Contracts.Abstractions
{
    /// <summary>
    /// A lazy-loaded singleton pattern for Godot nodes.
    /// The instance is created and added to the root of the scene tree when it's first accessed.
    /// Note: This implementation creates a new node from the script. If your singleton is a scene (.tscn),
    /// you should use Godot's AutoLoad feature with EagerSingletonNode instead.
    /// </summary>
    /// <typeparam name="T">The type of the singleton node, which must have a parameterless constructor.</typeparam>
    public abstract class LazySingletonNode<T> : Node where T : Node, new()
    {
        private static T _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// The single, globally accessible instance of the node.
        /// When accessed for the first time, it will create a new instance of the node
        /// and add it as a child of the scene tree's root.
        /// </summary>
        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        if (Engine.GetMainLoop() is SceneTree tree && tree.Root != null)
                        {
                            _instance = tree.Root.GetNode<T>(typeof(T).Name);
                        }

                        if (_instance == null)
                        {
                            GD.Print($"Creating lazy singleton instance of type: {typeof(T).Name}");
                            _instance = new T
                            {
                                Name = typeof(T).Name
                            };

                            if (Engine.GetMainLoop() is SceneTree tree2 && tree2.Root != null)
                            {
                                tree2.Root.AddChild(_instance);
                            }
                            else
                            {
                                GD.PushError($"Cannot add lazy singleton {typeof(T).Name} to the scene tree because the main loop is not a SceneTree or the root is null.");
                            }
                        }
                    }
                    return _instance;
                }
            }
        }

        public override void _EnterTree()
        {
            lock (_lock)
            {
                if (_instance != null && _instance != this)
                {
                    GD.PushWarning($"Duplicate instance of lazy singleton {typeof(T).Name} detected. The existing instance will be used, and this new one will be freed.");
                    QueueFree();
                    return;
                }

                if (_instance == null)
                {
                    _instance = this as T;
                }
            }
            base._EnterTree();
        }

        public override void _ExitTree()
        {
            lock(_lock)
            {
                if (_instance == this)
                {
                    _instance = null;
                }
            }
            base._ExitTree();
        }
    }
}