using WorldPainter.Runtime.Providers.MultiTile;
using WorldPainter.Runtime.Providers.Tile;
using WorldPainter.Runtime.Providers.Wall;

namespace WorldPainter.Runtime.Providers
{
    public interface IWorldFacade : ITileService, IWallService, IMultiTileService { }
}
