using System;
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

        [Obsolete("Obsolete")]
        private void Start()
        {
            TilePool tilePool = FindObjectOfType<TilePool>();
            if (tilePool != null)
            {
                SetTilePool(tilePool);
            }
        }

        public TileData GetTileAt(Vector2Int worldPos)
        {
            Vector2Int chunkCoord = WorldGrid.WorldToChunkCoord(worldPos);
            Vector2Int localPos = WorldGrid.WorldToLocalInChunk(worldPos);

            return _chunks.TryGetValue(chunkCoord, out ChunkData chunk)
                ? chunk.GetTile(localPos)
                : null;
        }

        [Obsolete("Obsolete")]
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

            // ВАЖНОЕ ИСПРАВЛЕНИЕ: Если чанка нет - создаем его
            if (!_activeChunks.TryGetValue(chunkCoord, out Chunk chunk))
            {
                // Создаем новый GameObject для чанка
                GameObject chunkGo = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
                chunk = chunkGo.AddComponent<Chunk>();

                // Нужно назначить TilePool - найдем его в сцене
                TilePool tilePool = FindObjectOfType<TilePool>();
                if (tilePool != null)
                {
                    chunk.tilePool = tilePool;
                }
                else
                {
                    Debug.LogError("TilePool not found in scene!");
                    return;
                }

                _activeChunks[chunkCoord] = chunk;

                // Инициализируем чанк с данными
                chunk.Initialize(chunkCoord, chunkData);
            }
            else
            {
                // Если чанк уже есть - просто обновляем тайл
                chunk.SetTile(localPos, tile);
            }
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
        
        public void SetTilePool(TilePool pool)
        {
            // Обновляем TilePool для всех активных чанков
            foreach (var activeChunk in _activeChunks.Values)
                activeChunk.tilePool = pool;
        }

        public void RegisterChunk(Vector2Int chunkCoord, Chunk chunk)
        {
            _activeChunks[chunkCoord] = chunk;
        }
    }
}
