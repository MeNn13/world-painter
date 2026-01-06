using System;
using System.Collections.Generic;
using UnityEditor;
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
            if (tilePool is not null)
                SetTilePool(tilePool);
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
            Undo.RegisterCompleteObjectUndo(this, "Paint Tile");

            Vector2Int chunkCoord = WorldGrid.WorldToChunkCoord(worldPos);
            Vector2Int localPos = WorldGrid.WorldToLocalInChunk(worldPos);

            if (!_chunks.TryGetValue(chunkCoord, out ChunkData chunkData))
            {
                chunkData = new ChunkData(chunkCoord);
                _chunks[chunkCoord] = chunkData;
            }

            chunkData.SetTile(localPos, tile);

            // ВАЖНОЕ ИСПРАВЛЕНИЕ: Проверяем, не уничтожен ли чанк
            bool needNewChunk = true;

            if (_activeChunks.TryGetValue(chunkCoord, out Chunk chunk))
            {
                // ПРОВЕРЯЕМ, ЖИВ ЛИ ЧАНК
                if (chunk != null)
                {
                    try
                    {
                        // Если можем получить доступ - чанк жив
                        var transform = chunk.transform;
                        if (transform != null)
                        {
                            // Чанк жив - обновляем тайл
                            chunk.SetTile(localPos, tile);
                            needNewChunk = false;
                        }
                    }
                    catch
                    {
                        // Чанк уничтожен - удаляем из словаря
                        _activeChunks.Remove(chunkCoord);
                    }
                }
                else
                {
                    // Чанк null - удаляем из словаря
                    _activeChunks.Remove(chunkCoord);
                }
            }

            // ЕСЛИ НУЖЕН НОВЫЙ ЧАНК
            if (needNewChunk)
            {
                GameObject chunkGo = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
                chunk = chunkGo.AddComponent<Chunk>();

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
                chunk.Initialize(chunkCoord, chunkData);
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

        public TileData SetTileAtWithUndo(Vector2Int worldPos, TileData tile)
        {
            Vector2Int chunkCoord = WorldGrid.WorldToChunkCoord(worldPos);
            Vector2Int localPos = WorldGrid.WorldToLocalInChunk(worldPos);

            if (!_chunks.TryGetValue(chunkCoord, out ChunkData chunkData))
            {
                chunkData = new ChunkData(chunkCoord);
                _chunks[chunkCoord] = chunkData;
            }

            // Сохраняем старый тайл для Undo
            TileData oldTile = chunkData.GetTile(localPos);

            // Устанавливаем новый тайл
            chunkData.SetTile(localPos, tile);

            // Обновляем визуал
            if (!_activeChunks.TryGetValue(chunkCoord, out Chunk chunk))
            {
                GameObject chunkGO = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
                chunk = chunkGO.AddComponent<Chunk>();

                TilePool tilePool = FindObjectOfType<TilePool>();
                if (tilePool != null)
                {
                    chunk.tilePool = tilePool;
                }
                else
                {
                    Debug.LogError("TilePool not found in scene!");
                    return oldTile;
                }

                _activeChunks[chunkCoord] = chunk;
                chunk.Initialize(chunkCoord, chunkData);
            }
            else
            {
                chunk.SetTile(localPos, tile);
            }

            return oldTile; // Возвращаем старый тайл для Undo
        }
    }
}
