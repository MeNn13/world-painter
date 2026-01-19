namespace WorldPainter.Runtime.Providers
{
    public interface IWorldFacadeEditor : IWorldFacade
    {
        void InitializeForEditor();
        bool IsInitialized { get; }
    }
}
