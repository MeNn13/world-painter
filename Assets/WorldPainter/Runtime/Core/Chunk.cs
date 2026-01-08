using System;
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
        private WorldManager _worldManager;
        private Vector2Int _chunkCoord;

        [Obsolete("Obsolete")]
        public void Initialize(Vector2Int chunkCoord, ChunkData data, WorldManager worldManager = null)
        {
            if (tilePool is null)
            {
                Debug.LogError("TilePool не назначен в Chunk!", this);
                return;
            }

            _chunkCoord = chunkCoord;
            _worldManager = worldManager ?? FindObjectOfType<WorldManager>();

            transform.position = new Vector3(
                chunkCoord.x * ChunkData.SIZE,
                chunkCoord.y * ChunkData.SIZE,
                0);

            RenderChunk(data);
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
                    Tile tile = tilePool?.GetTile(tileData, LocalToWorldPosition(localPos));
                    tile?.Initialize(tileData, LocalToWorldPosition(localPos), _worldManager);
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
            if (localPos.x is >= 0 and < ChunkData.SIZE
                && localPos.y is >= 0 and < ChunkData.SIZE)
            {
                return _tiles[localPos.x, localPos.y];
            }
            return null;
        }
        public void RemoveTile(Vector2Int localPos)
        {
            if (_tiles[localPos.x, localPos.y] != null)
            {
                tilePool?.ReturnTile(_tiles[localPos.x, localPos.y]);
                _tiles[localPos.x, localPos.y] = null;
            }
        }

        private void RenderChunk(ChunkData data)
        {
            for (int x = 0; x < ChunkData.SIZE; x++)
                for (int y = 0; y < ChunkData.SIZE; y++)
                {
                    Vector2Int localPos = new Vector2Int(x, y);
                    TileData tileData = data.GetTile(localPos);
                    
                    if (_worldManager is not null && tileData is MultiTileData)
                    {
                        Vector2Int worldPos = LocalToWorldPosition(localPos);
                        var multiTile = _worldManager.GetMultiTileAt(worldPos);
                        if (multiTile is null)
                            continue;
                    }
                    
                    if (tileData is not null && tileData is not MultiTileData)
                    {
                        Tile tile = tilePool.GetTile(tileData, LocalToWorldPosition(localPos));
                        tile.Initialize(tileData, LocalToWorldPosition(localPos), _worldManager);
                        tile.transform.SetParent(transform);
                        _tiles[x, y] = tile;
                    }
                }
        }
        private Vector2Int LocalToWorldPosition(Vector2Int localPos)
        {
            return new Vector2Int(
                _chunkCoord.x * ChunkData.SIZE + localPos.x,
                _chunkCoord.y * ChunkData.SIZE + localPos.y);
        }
    }
}
