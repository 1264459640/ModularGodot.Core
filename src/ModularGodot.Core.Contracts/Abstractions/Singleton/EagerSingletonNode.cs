using Godot;

namespace ModularGodot.Core.Contracts.Abstractions
{
    /// <summary>
    /// Base class for an "eager" singleton node. This pattern is best suited for nodes
    /// configured as an AutoLoad in Godot's Project Settings.
    /// The instance is assigned in the constructor, which Godot calls at startup for AutoLoads.
    /// </summary>
    /// <typeparam name="T">The type of the singleton node.</typeparam>
    public abstract class EagerSingletonNode<T> : Node where T : Node
    {
        private static T _instance;

        /// <summary>
        /// The single, globally accessible instance of the node. It is available after Godot
        /// has instantiated the AutoLoad nodes at startup.
        /// </summary>
        public static T Instance => _instance;

        public EagerSingletonNode()
        {
            if (_instance != null)
            {
                GD.Print($"Note: A second instance of EagerSingletonNode {typeof(T).Name} was constructed. This is expected if you have an instance in your scene, which will be removed.");
            }
            _instance = this as T;
        }

        public override void _EnterTree()
        {
            if (_instance != this)
            {
                GD.PushWarning($"Duplicate instance of eager singleton {typeof(T).Name} detected in the scene. Only the AutoLoad instance is allowed. Destroying the scene instance.");
                QueueFree();
            }
        }

        public override void _ExitTree()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}