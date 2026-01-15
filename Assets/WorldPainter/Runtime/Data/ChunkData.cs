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
        
        public ChunkData(Vector2Int chunkCoords)
        {
            ChunkCoords = chunkCoords;
            Tiles = new TileData[SIZE, SIZE];
        }

        public void SetTile(Vector2Int localPos, TileData tile) => Tiles[localPos.x, localPos.y] = tile;
        public TileData GetTile(Vector2Int localPos) => Tiles[localPos.x, localPos.y];  
        
        public void SetWall(Vector2Int localPos, WallData wall) => Walls[localPos.x, localPos.y] = wall;
        public WallData GetWall(Vector2Int localPos) => Walls[localPos.x, localPos.y];
        
        public bool IsEmpty(Vector2Int localPos) => Tiles[localPos.x, localPos.y] is null;
    }
}
