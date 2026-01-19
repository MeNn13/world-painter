using UnityEngine;

namespace WorldPainter.Runtime.Utils
{
    public static class WorldGrid
    {
        public const int CHUNK_SIZE = 16;

        public static Vector2Int WorldToChunkCoord(Vector2Int worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x / (float)CHUNK_SIZE),
                Mathf.FloorToInt(worldPos.y / (float)CHUNK_SIZE));
        }

        public static Vector2Int WorldToLocalInChunk(Vector2Int worldPos)
        {
            return new Vector2Int(
                PositiveMod(worldPos.x, CHUNK_SIZE),
                PositiveMod(worldPos.y, CHUNK_SIZE));
        }

        public static Vector2Int ChunkToWorldCoord(Vector2Int chunkCoord, Vector2Int localPos)
        {
            return new Vector2Int(
                chunkCoord.x * CHUNK_SIZE + localPos.x,
                chunkCoord.y * CHUNK_SIZE + localPos.y);
        }
        
        public static Vector2Int WorldToGridPosition(Vector3 worldPosition, bool snapToCenter = true)
        {
            if (snapToCenter)
            {
                // Для обычных тайлов: центр клетки
                return new Vector2Int(
                    Mathf.FloorToInt(worldPosition.x + 0.5f),
                    Mathf.FloorToInt(worldPosition.y + 0.5f)
                    );
            }
            
            // Для мультитайлов: левый нижний угол
            return new Vector2Int(
                Mathf.FloorToInt(worldPosition.x),
                Mathf.FloorToInt(worldPosition.y)
                );
        }
        
        public static Vector3 GridToWorldPosition(Vector2Int gridPosition, bool isCenter = true)
        {
            if (isCenter)
            {
                // Центр клетки
                return new Vector3(
                    gridPosition.x + 0.5f,
                    gridPosition.y + 0.5f,
                    0);
            }
            
            return new Vector3(
                gridPosition.x,
                gridPosition.y,
                0);
        }
        
        public static (Vector2Int, Vector2Int) GetChunkCoordsAndLocalPos(Vector2Int worldPos)
        {
            Vector2Int chunkCoord = WorldGrid.WorldToChunkCoord(worldPos);
            Vector2Int localPos = WorldGrid.WorldToLocalInChunk(worldPos);

            return (chunkCoord, localPos);
        }

        private static int PositiveMod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }
    }
}
