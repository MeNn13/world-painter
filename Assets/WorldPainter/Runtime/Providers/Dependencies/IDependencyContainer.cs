using WorldPainter.Runtime.Core;
using WorldPainter.Runtime.Providers.Tile;
using WorldPainter.Runtime.Providers.Wall;

namespace WorldPainter.Runtime.Providers.Dependencies
{
    public interface IDependencyContainer
    {
        ITileService TileService { get; }
        IWallService WallService { get; }
        ChunkService ChunkService { get; }
        TilePool TilePool { get; }
    }
}
