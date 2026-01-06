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

        public ChunkData(Vector2Int chunkCoords)
        {
            ChunkCoords = chunkCoords;
            Tiles = new TileData[SIZE, SIZE];
        }
        
        public TileData GetTile(Vector2Int localPos) => Tiles[localPos.x, localPos.y];
        public void SetTile(Vector2Int localPos, TileData tile) =>  Tiles[localPos.x, localPos.y] = tile;
    }
}
