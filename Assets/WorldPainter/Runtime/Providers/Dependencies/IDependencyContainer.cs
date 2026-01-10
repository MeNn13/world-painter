using WorldPainter.Runtime.Core;
using WorldPainter.Runtime.Providers.Tile;
using WorldPainter.Runtime.Providers.Wall;

namespace WorldPainter.Runtime.Providers.Dependencies
{
    public interface IDependencyContainer
    {
        ITileDataProvider TileProvider { get; }
        IWallDataProvider WallProvider { get; }
        TilePool TilePool { get; }
    }
}
