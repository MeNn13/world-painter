using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WorldPainter.Runtime.Core;
using WorldPainter.Runtime.Data;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Providers.Tile
{
    public class TileDataProvider : BaseWorldDataProvider, ITileDataProvider
    {
        private readonly Dictionary<Vector2Int, Chunk> _activeChunks = new();
        
        public TileData GetTileAt(Vector2Int worldPos)
        {
            Vector2Int chunkCoord = WorldToChunkCoord(worldPos);
            Vector2Int localPos = WorldToLocalInChunk(worldPos);

            return Chunks.TryGetValue(chunkCoord, out ChunkData chunk)
                ? chunk.GetTile(localPos)
                : null;
        }
        
        public void SetTileAt(Vector2Int worldPos, TileData tile)
        {
            Undo.RegisterCompleteObjectUndo(this, "Paint Tile");

            Vector2Int chunkCoord = WorldToChunkCoord(worldPos);
            Vector2Int localPos = WorldToLocalInChunk(worldPos);
            
            ChunkData chunkData = GetOrCreateChunk(chunkCoord);
            chunkData.SetTile(localPos, tile);
            
            UpdateTileVisual(chunkCoord, localPos, tile);
            UpdateNeighborTiles(worldPos);
        }

        public TileData SetTileAtWithUndo(Vector2Int worldPos, TileData tile)
        {
            Vector2Int chunkCoord = WorldToChunkCoord(worldPos);
            Vector2Int localPos = WorldToLocalInChunk(worldPos);
            
            ChunkData chunkData = GetOrCreateChunk(chunkCoord);
            TileData oldTile = chunkData.GetTile(localPos);
            chunkData.SetTile(localPos, tile);
            
            UpdateTileVisual(chunkCoord, localPos, tile);
            UpdateNeighborTiles(worldPos);
            
            return oldTile;
        }

        private void UpdateTileVisual(Vector2Int chunkCoord, Vector2Int localPos, TileData tile)
        {
            // TODO: Интеграция с визуальной системой (чтобы не дублировать код из SimpleWorldData)
        }

        private void UpdateNeighborTiles(Vector2Int worldPos)
        {
            // TODO: Обновление соседних тайлов
        }
        
        public ChunkData GetChunkData(Vector2Int chunkCoord) => GetChunk(chunkCoord);
        public void SetChunkData(Vector2Int chunkCoord, ChunkData chunk) => 
            Chunks[chunkCoord] = chunk;
    }
}
