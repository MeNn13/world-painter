namespace WorldPainter.Runtime.Providers
{
    public interface IWorldDataProviderEditor : IWorldDataProvider
    {
        void InitializeForEditor();
        bool IsInitialized { get; }
    }
}
