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

        // НОВЫЕ ПОЛЯ для мультитайлов
        private Dictionary<Vector2Int, MultiTile> _multiTiles = new(); // Позиция → MultiTile
        private Dictionary<Vector2Int, Vector2Int> _positionToMultiTileRoot = new(); // Быстрый поиск корня

        [Obsolete("Obsolete")]
        private void Start()
        {
            TilePool tilePool = FindObjectOfType<TilePool>();
            if (tilePool is not null)
                SetTilePool(tilePool);
        }

        public TileData GetTileAt(Vector2Int worldPos)
        {
            // Сначала проверяем мультитайлы
            if (_multiTiles.TryGetValue(worldPos, out MultiTile multiTile))
            {
                return multiTile.Data;
            }

            // Потом обычные тайлы
            Vector2Int chunkCoord = WorldGrid.WorldToChunkCoord(worldPos);
            Vector2Int localPos = WorldGrid.WorldToLocalInChunk(worldPos);

            return _chunks.TryGetValue(chunkCoord, out ChunkData chunk)
                ? chunk.GetTile(localPos)
                : null;
        }

        [Obsolete("Obsolete")]
        public void SetTileAt(Vector2Int worldPos, TileData tile)
        {
            if (_multiTiles.ContainsKey(worldPos))
            {
                RemoveMultiTileAt(worldPos);
            }

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

            if (tile != null || GetTileAt(worldPos) != null)
            {
                UpdateNeighborTiles(worldPos);
            }
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

        private void UpdateNeighborTiles(Vector2Int worldPos)
        {
            // 8 соседних позиций
            Vector2Int[] neighborOffsets =
            {
                new(0, 1), new(1, 1), new(1, 0), new(1, -1),
                new(0, -1), new(-1, -1), new(-1, 0), new(-1, 1)
            };

            foreach (var offset in neighborOffsets)
            {
                Vector2Int neighborPos = worldPos + offset;
                UpdateTileVisual(neighborPos);
            }

            // И сам тайл
            UpdateTileVisual(worldPos);
        }

        private void UpdateTileVisual(Vector2Int worldPos)
        {
            Vector2Int chunkCoord = WorldGrid.WorldToChunkCoord(worldPos);
            Vector2Int localPos = WorldGrid.WorldToLocalInChunk(worldPos);

            if (_activeChunks.TryGetValue(chunkCoord, out Chunk chunk) && chunk != null)
            {
                // Получаем тайл из чанка
                Tile tile = chunk.GetTileAtLocalPos(localPos);
                if (tile != null)
                {
                    // Обновляем спрайт тайла
                    tile.UpdateSprite(this); // this = IWorldDataProvider
                }
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

        /// <summary>
        /// Проверяет можно ли разместить мультитайл
        /// </summary>
        public bool CanPlaceMultiTile(MultiTileData data, Vector2Int rootPosition)
        {
            // 1. Проверяем что все клетки свободны
            var occupiedPositions = data.GetAllOccupiedPositions(rootPosition);
            foreach (var pos in occupiedPositions)
            {
                // Проверяем нет ли здесь обычного тайла
                if (GetTileAt(pos) != null)
                    return false;

                // Проверяем нет ли здесь другого мультитайла
                if (_multiTiles.ContainsKey(pos))
                    return false;
            }

            // 2. Проверяем правила крепления
            return CheckAttachmentRules(data, rootPosition);
        }

        /// <summary>
        /// Размещает мультитайл
        /// </summary>
        public bool PlaceMultiTile(MultiTileData data, Vector2Int rootPosition)
        {
            if (!CanPlaceMultiTile(data, rootPosition))
                return false;

            Undo.RegisterCompleteObjectUndo(this, $"Place {data.DisplayName}");

            // Получаем пул
            TilePool tilePool = FindObjectOfType<TilePool>();
            if (tilePool == null)
            {
                Debug.LogError("TilePool not found in scene!");
                return false;
            }

            // Создаём мультитайл
            MultiTile multiTile = tilePool.GetMultiTile(data, rootPosition);
            if (multiTile == null)
                return false;

            // Запоминаем во всех клетках
            var occupiedPositions = data.GetAllOccupiedPositions(rootPosition);
            foreach (var pos in occupiedPositions)
            {
                _multiTiles[pos] = multiTile;
                _positionToMultiTileRoot[pos] = rootPosition;

                // В данных чанка отмечаем что здесь мультитайл
                Vector2Int chunkCoord = WorldGrid.WorldToChunkCoord(pos);
                Vector2Int localPos = WorldGrid.WorldToLocalInChunk(pos);

                if (!_chunks.TryGetValue(chunkCoord, out ChunkData chunkData))
                {
                    chunkData = new ChunkData(chunkCoord);
                    _chunks[chunkCoord] = chunkData;
                }

                // Сохраняем ссылку на MultiTileData вместо null
                chunkData.SetTile(localPos, data);
            }

            // Родитель - этот объект или можно в отдельный контейнер
            multiTile.transform.SetParent(transform);

            Debug.Log($"Placed MultiTile {data.DisplayName} at {rootPosition}");
            return true;
        }

        /// <summary>
        /// Удаляет мультитайл по любой позиции внутри него
        /// </summary>
        public bool RemoveMultiTileAt(Vector2Int anyPosition)
        {
            if (!_positionToMultiTileRoot.TryGetValue(anyPosition, out Vector2Int rootPosition))
                return false;

            if (!_multiTiles.TryGetValue(anyPosition, out MultiTile multiTile))
                return false;

            Undo.RegisterCompleteObjectUndo(this, $"Remove {multiTile.Data.DisplayName}");

            // Удаляем из всех словарей
            var occupiedPositions = multiTile.GetAllOccupiedPositions();
            foreach (var pos in occupiedPositions)
            {
                _multiTiles.Remove(pos);
                _positionToMultiTileRoot.Remove(pos);

                // Очищаем данные чанка
                Vector2Int chunkCoord = WorldGrid.WorldToChunkCoord(pos);
                Vector2Int localPos = WorldGrid.WorldToLocalInChunk(pos);

                if (_chunks.TryGetValue(chunkCoord, out ChunkData chunkData))
                {
                    chunkData.SetTile(localPos, null);
                }
            }

            // Возвращаем в пул
            TilePool tilePool = FindObjectOfType<TilePool>();
            if (tilePool != null)
            {
                tilePool.ReturnMultiTile(multiTile);
            }
            else
            {
                DestroyImmediate(multiTile.gameObject);
            }

            Debug.Log($"Removed MultiTile {multiTile.Data.DisplayName}");
            return true;
        }

        /// <summary>
        /// Получает мультитайл по позиции
        /// </summary>
        public MultiTile GetMultiTileAt(Vector2Int position)
        {
            _multiTiles.TryGetValue(position, out MultiTile multiTile);
            return multiTile;
        }

        /// <summary>
        /// Проверяет правила крепления для мультитайла
        /// </summary>
        private bool CheckAttachmentRules(MultiTileData data, Vector2Int rootPosition)
        {
            switch (data.attachmentType)
            {
                case MultiTileData.AttachmentType.None:
                    return true; // Можно размещать где угодно

                case MultiTileData.AttachmentType.Ground:
                    return CheckGroundAttachment(data, rootPosition);

                case MultiTileData.AttachmentType.Ceiling:
                    return CheckCeilingAttachment(data, rootPosition);

                case MultiTileData.AttachmentType.Wall:
                    // Пока заглушка - всегда true
                    Debug.LogWarning("Wall attachment not implemented yet");
                    return true;

                case MultiTileData.AttachmentType.GroundAndCeiling:
                    return CheckGroundAttachment(data, rootPosition) && CheckCeilingAttachment(data, rootPosition);

                default:
                    return true;
            }
        }

        /// <summary>
        /// Проверяет крепление к земле
        /// </summary>
        private bool CheckGroundAttachment(MultiTileData data, Vector2Int rootPosition)
        {
            // Для каждого тайла в нижнем ряду проверяем, есть ли под ним блок
            for (int x = 0; x < data.size.x; x++)
            {
                Vector2Int groundPos = rootPosition + new Vector2Int(x, -1);
                if (GetTileAt(groundPos) == null)
                    return false; // Нет блока под этим тайлом
            }
            return true;
        }

        /// <summary>
        /// Проверяет крепление к потолку
        /// </summary>
        private bool CheckCeilingAttachment(MultiTileData data, Vector2Int rootPosition)
        {
            // Для каждого тайла в верхнем ряду проверяем, есть ли над ним блок
            for (int x = 0; x < data.size.x; x++)
            {
                Vector2Int ceilingPos = rootPosition + new Vector2Int(x, data.size.y);
                if (GetTileAt(ceilingPos) == null)
                    return false; // Нет блока над этим тайлом
            }
            return true;
        }

    }
}
