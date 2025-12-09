namespace ModularGodot.Core.Contracts.Abstractions.Bases
{
    /// <summary>
    /// A marker interface for services.
    /// </summary>
    public interface IService<TConfig> where TConfig : class
    {
        /// <summary>
        /// Initialize the service with the given configuration.
        /// </summary>
        void Initialize(TConfig config);

        /// <summary>
        /// Dispose the service.
        /// </summary>
        void Dispose();
    }
}
