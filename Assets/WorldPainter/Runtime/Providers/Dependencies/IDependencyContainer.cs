using WorldPainter.Runtime.Core;
using WorldPainter.Runtime.Providers.Tile;
using WorldPainter.Runtime.Providers.Wall;

namespace WorldPainter.Runtime.Providers.Dependencies
{
    public interface IDependencyContainer
    {
        ITileService TileProvider { get; }
        IWallService WallProvider { get; }
        TilePool TilePool { get; }
    }
}
