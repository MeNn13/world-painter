using System;
using System.Collections.Generic;
using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Data
{
    [Serializable]
    public class ChunkData
    {
        public const int SIZE = 16;
        public Vector2Int ChunkCoords { get; private set; }

        public TileData[,] Tiles { get; private set; }
        public WallData[,] Walls { get; private set; }
        public List<Vector2Int> MultiTileRoots { get; private set; } = new();
        
        public ChunkData(Vector2Int chunkCoords)
        {
            ChunkCoords = chunkCoords;
            Tiles = new TileData[SIZE, SIZE];
            Walls = new WallData[SIZE, SIZE];
        }

        public void SetTile(Vector2Int localPos, TileData tile) => Tiles[localPos.x, localPos.y] = tile;
        public TileData GetTile(Vector2Int localPos) => Tiles[localPos.x, localPos.y];  
        
        public void SetWall(Vector2Int localPos, WallData wall) => Walls[localPos.x, localPos.y] = wall;
        public WallData GetWall(Vector2Int localPos) => Walls[localPos.x, localPos.y];
        
        public void AddMultiTileReference(Vector2Int rootPosition)
        {
            if (!MultiTileRoots.Contains(rootPosition))
                MultiTileRoots.Add(rootPosition);
        }
        public void RemoveMultiTileReference(Vector2Int rootPosition)
        {
            MultiTileRoots.Remove(rootPosition);
        }
        public bool HasMultiTileReferences => MultiTileRoots.Count > 0;
        
        public bool IsEmpty(Vector2Int localPos) => Tiles[localPos.x, localPos.y] is null;
        public bool IsChunkEmpty()
        {
            for (int x = 0; x < SIZE; x++)
                for (int y = 0; y < SIZE; y++)
                    if (Tiles[x, y] is not null || Walls[x, y] is not null)
                        return false;
            
            return true;
        }
    }
}
