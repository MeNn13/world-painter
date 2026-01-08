using UnityEditor;
using UnityEngine;
using WorldPainter.Runtime.Data;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Providers.Wall
{
    public class WallDataProvider : BaseWorldDataProvider, IWallDataProvider
    {
        public WallData GetWallAt(Vector2Int worldPos)
        {
            Vector2Int chunkCoord = WorldToChunkCoord(worldPos);
            Vector2Int localPos = WorldToLocalInChunk(worldPos);

            return Chunks.TryGetValue(chunkCoord, out ChunkData chunk)
                ? chunk.GetWall(localPos)
                : null;
        }
        
        public void SetWallAt(Vector2Int worldPos, WallData wall)
        {
            Undo.RegisterCompleteObjectUndo(this, "Paint Wall");
            
            Vector2Int chunkCoord = WorldToChunkCoord(worldPos);
            Vector2Int localPos = WorldToLocalInChunk(worldPos);
            
            ChunkData chunkData = GetOrCreateChunk(chunkCoord);
            chunkData.SetWall(localPos, wall);
            
            // TODO: Обновить визуал стены
        }

        public WallData SetWallAtWithUndo(Vector2Int worldPos, WallData wall)
        {
            Vector2Int chunkCoord = WorldToChunkCoord(worldPos);
            Vector2Int localPos = WorldToLocalInChunk(worldPos);
            
            ChunkData chunkData = GetOrCreateChunk(chunkCoord);
            WallData oldWall = chunkData.GetWall(localPos);
            chunkData.SetWall(localPos, wall);
            
            // TODO: Обновить визуал стены
            
            return oldWall;
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
