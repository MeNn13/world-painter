using System.Collections.Generic;
using UnityEngine;
using WorldPainter.Runtime.Core;
using WorldPainter.Runtime.Data;
using WorldPainter.Runtime.ScriptableObjects;
using WorldPainter.Runtime.Utils;

namespace WorldPainter.Runtime.Providers
{
    public class SimpleWorldData : MonoBehaviour, IWorldDataProvider
    {
        private readonly Dictionary<Vector2Int, ChunkData> _chunks = new();
        private readonly Dictionary<Vector2Int, Chunk> _activeChunks = new();
        
        public TileData GetTileAt(Vector2Int worldPos)
        {
            Vector2Int chunkCoord = WorldGrid.WorldToChunkCoord(worldPos);
            Vector2Int localPos = WorldGrid.WorldToLocalInChunk(worldPos);
            
            return _chunks.TryGetValue(chunkCoord, out ChunkData chunk) 
                ? chunk.GetTile(localPos)
                : null;
        }
        
        public void SetTileAt(Vector2Int worldPos, TileData tile)
        {
            Vector2Int chunkCoord = WorldGrid.WorldToChunkCoord(worldPos);
            Vector2Int localPos = WorldGrid.WorldToLocalInChunk(worldPos);
            
            if (!_chunks.TryGetValue(chunkCoord, out ChunkData chunkData))
            {
                chunkData = new ChunkData(chunkCoord);
                _chunks[chunkCoord] = chunkData;
            }
            
            chunkData.SetTile(localPos, tile);
            
            if (_activeChunks.TryGetValue(chunkCoord, out Chunk chunk))
                chunk.SetTile(localPos, tile);
        }
        
        public ChunkData GetChunkData(Vector2Int chunkCoord)
        {
            if (_chunks.TryGetValue(chunkCoord, out ChunkData chunk))
                return chunk;
            
            var newChunk = new ChunkData(chunkCoord);
            _chunks[chunkCoord] = newChunk;
            return newChunk;
        }
        
        public void SetChunkData(Vector2Int chunkCoord, ChunkData chunk) => 
            _chunks[chunkCoord] = chunk;
        
        public void RequestChunkUpdate(Vector2Int chunkCoord)
        {
            throw new System.NotImplementedException();
        }

        public void RegisterChunk(Vector2Int chunkCoord, Chunk chunk)
        {
            _activeChunks[chunkCoord] = chunk;
        }
    }
}
