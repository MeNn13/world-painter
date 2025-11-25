using UnityEngine;
using WorldPainter.Runtime.Chunking;

namespace WorldPainter.Editor.Coordinators
{
    public class ChunkCoordinator
    {
        public (int chunkX, int chunkY) WorldToChunkCoords(int worldX, int worldY, BoundsInt worldBounds)
        {
            int chunkX = (worldX - worldBounds.x) / WorldChunk.ChunkSize;
            int chunkY = (worldY - worldBounds.y) / WorldChunk.ChunkSize;
            return (chunkX, chunkY);
        }

        public (int localX, int localY) WorldToLocalCoords(int worldX, int worldY, BoundsInt worldBounds)
        {
            int localX = (worldX - worldBounds.x) % WorldChunk.ChunkSize;
            int localY = (worldY - worldBounds.y) % WorldChunk.ChunkSize;

            if (localX < 0) localX += WorldChunk.ChunkSize;
            if (localY < 0) localY += WorldChunk.ChunkSize;

            return (localX, localY);
        }

        public (int chunksX, int chunksY) CalculateChunkCount(BoundsInt bounds)
        {
            int chunksX = Mathf.CeilToInt((float)bounds.size.x / WorldChunk.ChunkSize);
            int chunksY = Mathf.CeilToInt((float)bounds.size.y / WorldChunk.ChunkSize);
            return (chunksX, chunksY);
        }

        public Vector3 CalculateChunkPosition(int chunkX, int chunkY, BoundsInt worldBounds)
        {
            return new Vector3(
                worldBounds.x + chunkX * WorldChunk.ChunkSize,
                worldBounds.y + chunkY * WorldChunk.ChunkSize,
                0);
        }
        
        public bool IsPositionInChunkBounds(int localX, int localY)
        {
            return localX is >= 0 and < WorldChunk.ChunkSize && 
                   localY is >= 0 and < WorldChunk.ChunkSize;
        }
    }
}
