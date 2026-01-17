using WorldPainter.Runtime.Core;
using WorldPainter.Runtime.Providers.MultiTile;
using WorldPainter.Runtime.Providers.Tile;
using WorldPainter.Runtime.Providers.Wall;

namespace WorldPainter.Runtime.Providers.Dependencies
{
    public class DependencyContainer : IDependencyContainer
    {
        public DependencyContainer(ITileService tileService, 
            IWallService wallService,
            IMultiTileService multiTileService, 
            IWorldFacade worldFacade,
            ChunkService chunkService,
            TilePool tilePool)
        {
            TileService = tileService;
            WallService = wallService;
            MultiTileService = multiTileService;
            WorldFacade = worldFacade;
            ChunkService = chunkService;
            TilePool = tilePool;
        }
        
        public ITileService TileService { get; }
        public IWallService WallService { get; }
        public IMultiTileService MultiTileService { get; }
        public IWorldFacade WorldFacade { get; } 
        public ChunkService ChunkService { get; }
        public TilePool TilePool { get; }
    }
}
