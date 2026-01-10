namespace WorldPainter.Runtime.Providers.Dependencies
{
    public interface IRequiresDependencies
    {
        void InjectDependencies(IDependencyContainer container);
    }
}
