using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;
using WorldPainter.Runtime.Utils;

namespace WorldPainter.Runtime.Providers.Wall
{
    public class WallService : IWallService
    {
        private readonly ChunkService _chunkService;
        private readonly IWorldFacade _worldFacade;

        public WallService(IWorldFacade worldFacade, ChunkService chunkService)
        {
            _worldFacade = worldFacade;
            _chunkService = chunkService;
        }
        
        public void SetWallAt(Vector2Int worldPos, WallData wall)
        {
            var (chunkCoord, localPos) = WorldGrid.GetChunkCoordsAndLocalPos(worldPos);
            _chunkService.SetWallInChunk(chunkCoord, localPos, wall, _worldFacade);
        }
        public WallData GetWallAt(Vector2Int worldPos)
        {
            var (chunkCoord, localPos) = WorldGrid.GetChunkCoordsAndLocalPos(worldPos);
            return _chunkService.GetWallDataFromChunk(chunkCoord, localPos);
        }
        
        public bool HasWallInArea(Vector2Int startPos, Vector2Int size)
        {
            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                {
                    Vector2Int checkPos = startPos + new Vector2Int(x, y);
                    if (GetWallAt(checkPos) is null)
                        return false;
                }
            return true;
        }
        public bool HasContinuousWall(Vector2Int startPos, Vector2Int direction, int length)
        {
            for (int i = 0; i < length; i++)
            {
                Vector2Int checkPos = startPos + direction * i;
                if (GetWallAt(checkPos) is null)
                    return false;
            }
            return true;
        }
        public bool HasWallOfType(Vector2Int position, WallData requiredWall = null)
        {
            WallData wall = GetWallAt(position);
            if (wall is null) return false;

            if (requiredWall is null) return true;

            return wall.TileId == requiredWall.TileId;
        }
    }
}
