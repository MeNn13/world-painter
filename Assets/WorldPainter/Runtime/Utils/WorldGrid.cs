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

        private static int PositiveMod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }
    }
}
