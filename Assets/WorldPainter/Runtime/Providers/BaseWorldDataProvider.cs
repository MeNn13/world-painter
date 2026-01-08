using System.Collections.Generic;
using UnityEngine;
using WorldPainter.Runtime.Data;
using WorldPainter.Runtime.Utils;

namespace WorldPainter.Runtime.Providers
{
    public class BaseWorldDataProvider : MonoBehaviour
    {
        protected readonly Dictionary<Vector2Int, ChunkData> Chunks = new();
        
        protected ChunkData GetOrCreateChunk(Vector2Int chunkCoord)
        {
            if (!Chunks.TryGetValue(chunkCoord, out ChunkData chunk))
            {
                chunk = new ChunkData(chunkCoord);
                Chunks[chunkCoord] = chunk;
            }
            return chunk;
        }
        
        protected ChunkData GetChunk(Vector2Int chunkCoord)
        {
            Chunks.TryGetValue(chunkCoord, out ChunkData chunk);
            return chunk;
        }
        
        protected Vector2Int WorldToChunkCoord(Vector2Int worldPos) => 
            WorldGrid.WorldToChunkCoord(worldPos);
        
        protected Vector2Int WorldToLocalInChunk(Vector2Int worldPos) => 
            WorldGrid.WorldToLocalInChunk(worldPos);
    }
}
