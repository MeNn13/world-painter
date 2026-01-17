using UnityEngine;
using WorldPainter.Runtime.Core;
using WorldPainter.Runtime.Providers.Dependencies;
using WorldPainter.Runtime.ScriptableObjects;
using WorldPainter.Runtime.Utils;

namespace WorldPainter.Runtime.Providers.Tile
{
    internal class TileService : ITileService, IInitializable
    {
        private ChunkService _chunkService;
        private IWorldFacade _worldFacade;
        
        public void Initialize(IDependencyContainer container)
        {
            _chunkService = container.ChunkService;
            _worldFacade = container.WorldFacade;
        }

        public void SetTileAt(Vector2Int worldPos, TileData tile)
        {
            var (chunkCoord, localPos) = WorldGrid.GetChunkCoordsAndLocalPos(worldPos);
            _chunkService.SetTileInChunk(chunkCoord, localPos, tile, _worldFacade);
            
            UpdateNeighborTiles(worldPos);
        }
        public TileData GetTileAt(Vector2Int worldPos)
        {
            var (chunkCoord, localPos) = WorldGrid.GetChunkCoordsAndLocalPos(worldPos);
            return _chunkService.GetTileDataFromChunk(chunkCoord, localPos);
        }
        
        private void UpdateNeighborTiles(Vector2Int worldPos)
        {
            Vector2Int[] offsets =
            {
                new(0, 1), new(1, 1), new(1, 0), new(1, -1),
                new(0, -1), new(-1, -1), new(-1, 0), new(-1, 1)
            };

            foreach (var offset in offsets)
            {
                Vector2Int neighborPos = worldPos + offset;
                UpdateTileSprite(neighborPos);
            }

            UpdateTileSprite(worldPos);
        }
        private void UpdateTileSprite(Vector2Int worldPos)
        {
            var (chunkCoord, localPos) = WorldGrid.GetChunkCoordsAndLocalPos(worldPos);

            if (_chunkService.TryGetTileView(chunkCoord, localPos, out TileView tileView))
                tileView?.UpdateSprite();
        }
    }
}
