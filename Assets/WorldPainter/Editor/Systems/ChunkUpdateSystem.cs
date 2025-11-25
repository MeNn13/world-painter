using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using WorldPainter.Runtime.Chunking;
using WorldPainter.Runtime.Data;
using TileData = WorldPainter.Runtime.ScriptableObjects.TileData;

namespace WorldPainter.Editor.Systems
{
    public class ChunkUpdateSystem
    {
        private readonly List<TileData> _tileDatabase;

        public ChunkUpdateSystem(List<TileData> tileDatabase)
        {
            _tileDatabase = tileDatabase;
        }

        public void UpdateDirtyChunks(Transform chunksParent)
        {
            int updatedCount = 0;

            foreach (Transform chunkTransform in chunksParent)
            {
                WorldChunk chunk = chunkTransform.GetComponent<WorldChunk>();
                if (chunk is not null && chunk.IsDirty)
                {
                    UpdateChunkVisuals(chunk);
                    UpdateChunkCollision(chunk);
                    chunk.IsDirty = false;
                    updatedCount++;
                }
            }

            if (updatedCount > 0)
            {
                Debug.Log($"Updated {updatedCount} dirty chunks");
            }
        }

        private void UpdateChunkVisuals(WorldChunk chunk)
        {
            chunk.VisualTilemap.ClearAllTiles();

            for (int x = 0; x < WorldChunk.ChunkSize; x++)
                for (int y = 0; y < WorldChunk.ChunkSize; y++)
                {
                    if (x < 0 || y < 0)
                    {
                        Debug.LogWarning($"Trying to read cell at invalid position: ({x}, {y})");
                        continue;
                    }

                    TileCell cell = chunk.GetCell(x, y);

                    if (!cell.IsEmpty && cell.TileId > 0 && cell.TileId <= _tileDatabase.Count)
                    {
                        TileData tileData = _tileDatabase[cell.TileId - 1];
                        if (tileData is not null && tileData.RuleTile is not null)
                        {
                            Vector3Int position = new Vector3Int(x, y, 0);
                            chunk.VisualTilemap.SetTile(position, tileData.RuleTile);
                        }
                    }
                }
        }

        private void UpdateChunkCollision(WorldChunk chunk)
        {
            chunk.CollisionTilemap.ClearAllTiles();

            Tile collisionTile = ScriptableObject.CreateInstance<Tile>();
            collisionTile.sprite = null;

            for (int x = 0; x < WorldChunk.ChunkSize; x++)
                for (int y = 0; y < WorldChunk.ChunkSize; y++)
                {
                    TileCell cell = chunk.GetCell(x, y);

                    if (!cell.IsEmpty && cell.TileId > 0 && cell.TileId <= _tileDatabase.Count)
                    {
                        TileData tileData = _tileDatabase[cell.TileId - 1];
                        if (tileData is not null && tileData.IsSolid)
                        {
                            Vector3Int position = new Vector3Int(x, y, 0);
                            chunk.CollisionTilemap.SetTile(position, collisionTile);
                        }
                    }
                }

            chunk.CollisionTilemap.RefreshAllTiles();
            if (chunk.ChunkCollider is not null)
            {
                chunk.ChunkCollider.GenerateGeometry();
            }

            Object.DestroyImmediate(collisionTile);
        }
    }
}
