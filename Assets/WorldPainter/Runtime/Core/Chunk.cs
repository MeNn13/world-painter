using UnityEngine;
using WorldPainter.Runtime.Data;
using WorldPainter.Runtime.Providers;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Core
{
    public class Chunk : MonoBehaviour
    {
        public TilePool tilePool;

        private readonly Tile[,] _tiles = new Tile[ChunkData.SIZE, ChunkData.SIZE];
        private SimpleWorldData _cachedWorldProvider;

        public Vector2Int ChunkCoord { get; private set; }

        public void Initialize(Vector2Int chunkCoord, ChunkData data)
        {
            if (tilePool is null)
            {
                Debug.LogError("TilePool не назначен в Chunk!", this);
                return;
            }

            ChunkCoord = chunkCoord;
            _cachedWorldProvider = FindObjectOfType<SimpleWorldData>();

            transform.position = new Vector3(
                chunkCoord.x * ChunkData.SIZE,
                chunkCoord.y * ChunkData.SIZE,
                0);

            RenderChunk(data);
        }

        private void RenderChunk(ChunkData data)
        {
            SimpleWorldData worldProvider = FindObjectOfType<SimpleWorldData>();

            for (int x = 0; x < ChunkData.SIZE; x++)
                for (int y = 0; y < ChunkData.SIZE; y++)
                {
                    Vector2Int localPos = new Vector2Int(x, y);
                    TileData tileData = data.GetTile(localPos);

                    if (tileData is MultiTileData multiTileData)
                    {
                        // Для мультитайлов:
                        // 1. Проверяем, не был ли мультитайл уже создан
                        Vector2Int worldPos = LocalToWorldPosition(localPos);
                        
                        // Если это НЕ корневая позиция мультитайла, пропускаем
                        // (мультитайл создаётся только для своей корневой позиции)
                        if (_cachedWorldProvider != null)
                        {
                            var multiTile = _cachedWorldProvider.GetMultiTileAt(worldPos);
                            if (multiTile != null)
                            {
                                // Эта позиция уже занята мультитайлом - пропускаем создание обычного Tile
                                continue;
                            }
                        }
                        else
                        {
                            // Если нет провайдера, всё равно пропускаем MultiTileData
                            continue;
                        }
                    }

                    // Обычные тайлы - создаём как раньше
                    if (tileData is not null && !(tileData is MultiTileData))
                    {
                        Tile tile = tilePool.GetTile(tileData, LocalToWorldPosition(localPos));
                        tile.Initialize(tileData, LocalToWorldPosition(localPos), _cachedWorldProvider);
                        tile.transform.SetParent(transform);
                        _tiles[x, y] = tile;
                    }
                }
        }

        public void SetTile(Vector2Int localPos, TileData tileData)
        {
            if (tileData is MultiTileData)
            {
                // MultiTileData обрабатывается отдельно через SimpleWorldData.PlaceMultiTile()
                // Не создаём для него обычный Tile
                return;
            }
            
            DeleteHasOldTile();

            CreateNewTile();

            return;

            void DeleteHasOldTile()
            {
                if (_tiles[localPos.x, localPos.y] is not null)
                {
                    tilePool?.ReturnTile(_tiles[localPos.x, localPos.y]);
                    _tiles[localPos.x, localPos.y] = null;
                }
            }

            void CreateNewTile()
            {
                if (tileData is not null)
                {
                    SimpleWorldData worldProvider = FindObjectOfType<SimpleWorldData>();
                    Tile tile = tilePool?.GetTile(tileData, LocalToWorldPosition(localPos));
                    // ПЕРЕДАЕМ worldProvider
                    tile?.Initialize(tileData, LocalToWorldPosition(localPos), worldProvider);
                    tile?.transform.SetParent(transform);
                    _tiles[localPos.x, localPos.y] = tile;
                }
            }
        }

        public bool IsEmpty()
        {
            for (int x = 0; x < ChunkData.SIZE; x++)
                for (int y = 0; y < ChunkData.SIZE; y++)
                    if (_tiles[x, y] is not null)
                        return false;

            return true;
        }

        public Tile GetTileAtLocalPos(Vector2Int localPos)
        {
            if (localPos.x >= 0 && localPos.x < ChunkData.SIZE && localPos.y >= 0 && localPos.y < ChunkData.SIZE)
            {
                return _tiles[localPos.x, localPos.y];
            }
            return null;
        }

        public void Clear()
        {
            for (int x = 0; x < ChunkData.SIZE; x++)
                for (int y = 0; y < ChunkData.SIZE; y++)
                    if (_tiles[x, y] is not null)
                    {
                        tilePool?.ReturnTile(_tiles[x, y]);
                        _tiles[x, y] = null;
                    }
        }
        
        /// <summary>
        /// Обновляет все тайлы в чанке (например, при изменении соседей)
        /// </summary>
        public void UpdateAllTiles()
        {
            for (int x = 0; x < ChunkData.SIZE; x++)
                for (int y = 0; y < ChunkData.SIZE; y++)
                {
                    var tile = _tiles[x, y];
                    if (tile != null && _cachedWorldProvider != null)
                    {
                        tile.UpdateSprite(_cachedWorldProvider);
                    }
                }
        }

        
        /// <summary>
        /// Удаляет тайл из чанка по локальной позиции
        /// </summary>
        public void RemoveTile(Vector2Int localPos)
        {
            if (_tiles[localPos.x, localPos.y] != null)
            {
                tilePool?.ReturnTile(_tiles[localPos.x, localPos.y]);
                _tiles[localPos.x, localPos.y] = null;
            }
        }

        private Vector2Int LocalToWorldPosition(Vector2Int localPos)
        {
            return new Vector2Int(
                ChunkCoord.x * ChunkData.SIZE + localPos.x,
                ChunkCoord.y * ChunkData.SIZE + localPos.y);
        }
    }
}
