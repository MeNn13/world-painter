using System;
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
        
        public bool HasTiles { get; private set; }
        public bool HasWalls { get; private set; }
        public bool IsEmpty => !HasTiles && !HasWalls;

        public ChunkData(Vector2Int chunkCoords)
        {
            ChunkCoords = chunkCoords;
            Tiles = new TileData[SIZE, SIZE];
            Walls = new WallData[SIZE, SIZE];
        }
        
        public TileData GetTile(Vector2Int localPos) => Tiles[localPos.x, localPos.y];
        public void SetTile(Vector2Int localPos, TileData tile)
        {
            Tiles[localPos.x, localPos.y] = tile;
            UpdateTileCache();
        }
        
        public WallData GetWall(Vector2Int localPos) => Walls[localPos.x, localPos.y];
        public void SetWall(Vector2Int localPos, WallData wall)
        {
            Walls[localPos.x, localPos.y] = wall;
            UpdateWallCache();
        }
        
        private void UpdateTileCache()
        {
            HasTiles = false;
            for (int x = 0; x < SIZE; x++)
                for (int y = 0; y < SIZE; y++)
                    if (Tiles[x, y] is not null)
                    {
                        HasTiles = true;
                        return;
                    }
        }
        private void UpdateWallCache()
        {
            HasWalls = false;
            for (int x = 0; x < SIZE; x++)
                for (int y = 0; y < SIZE; y++)
                    if (Walls[x, y] is not null)
                    {
                        HasWalls = true;
                        return;
                    }
        }
    }
}
