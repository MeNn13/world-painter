using UnityEngine;
using WorldPainter.Runtime.Data;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Core
{
    public class Chunk : MonoBehaviour
    {
        public TilePool tilePool;

        private readonly Tile[,] _tiles = new Tile[ChunkData.SIZE, ChunkData.SIZE];

        public Vector2Int ChunkCoord { get; private set; }

        public void Initialize(Vector2Int chunkCoord, ChunkData data)
        {
            if (tilePool is null)
            {
                Debug.LogError("TilePool не назначен в Chunk!", this);
                return;
            }

            ChunkCoord = chunkCoord;

            transform.position = new Vector3(
                chunkCoord.x * ChunkData.SIZE,
                chunkCoord.y * ChunkData.SIZE,
                0);

            RenderChunk(data);
        }

        private void RenderChunk(ChunkData data)
        {
            for (int x = 0; x < ChunkData.SIZE; x++)
                for (int y = 0; y < ChunkData.SIZE; y++)
                {
                    Vector2Int localPos = new Vector2Int(x, y);
                    TileData tileData = data.GetTile(localPos);

                    if (tileData is not null)
                    {
                        Tile tile = tilePool.GetTile(tileData, LocalToWorldPosition(localPos));
                        tile.transform.SetParent(transform);
                        _tiles[x, y] = tile;
                    }
                }
        }

        public void SetTile(Vector2Int localPos, TileData tileData)
        {
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
                    tile?.transform.SetParent(transform);
                    _tiles[localPos.x, localPos.y] = tile;
                }
            }
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

        private Vector2Int LocalToWorldPosition(Vector2Int localPos)
        {
            return new Vector2Int(
                ChunkCoord.x * ChunkData.SIZE + localPos.x,
                ChunkCoord.y * ChunkData.SIZE + localPos.y);
        }
    }
}
