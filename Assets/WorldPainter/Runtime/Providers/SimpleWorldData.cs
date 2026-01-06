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
                // Если рисуем новый тайл в новом месте - создаем чанк
                if (tile != null)
                {
                    chunkData = new ChunkData(chunkCoord);
                    _chunks[chunkCoord] = chunkData;
                }
                else
                {
                    // Если стираем там, где ничего нет - просто выходим
                    return;
                }
            }

            // Устанавливаем тайл в данные
            chunkData.SetTile(localPos, tile);

            // ОБНОВЛЯЕМ ТОЛЬКО ОДИН ТАЙЛ (оптимально)
            UpdateSingleTileVisual(chunkCoord, localPos, tile);
        }

        private void UpdateSingleTileVisual(Vector2Int chunkCoord, Vector2Int localPos, TileData tile)
        {
            // Если чанк существует
            if (_activeChunks.TryGetValue(chunkCoord, out Chunk chunk) && chunk != null)
            {
                // Обновляем тайл
                chunk.SetTile(localPos, tile);
        
                // ПРОВЕРЯЕМ, СТАЛ ЛИ ЧАНК ПУСТЫМ ПОСЛЕ СТИРАНИЯ
                if (tile == null && chunk.IsEmpty())
                {
                    // Удаляем пустой чанк
                    DestroyImmediate(chunk.gameObject);
                    _activeChunks.Remove(chunkCoord);
                }
            }
            else if (tile != null) // Если чанка нет, но мы рисуем тайл
            {
                // Создаем новый чанк
                CreateNewChunk(chunkCoord);
            }
        }

        private void CreateNewChunk(Vector2Int chunkCoord)
        {
            if (!_chunks.TryGetValue(chunkCoord, out ChunkData chunkData))
                return;

            GameObject chunkGo = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
            Chunk newChunk = chunkGo.AddComponent<Chunk>();

            TilePool tilePool = FindObjectOfType<TilePool>();
            if (tilePool != null)
            {
                newChunk.tilePool = tilePool;
            }
            else
            {
                Debug.LogError("TilePool not found in scene!");
                return;
            }

            _activeChunks[chunkCoord] = newChunk;
            newChunk.Initialize(chunkCoord, chunkData);
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
