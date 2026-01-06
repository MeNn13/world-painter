using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Editor
{
    public class WorldPaintOperation
    {
        public Vector2Int Position { get; }
        public TileData OldTile { get; }
        public TileData NewTile { get; }
        
        public WorldPaintOperation(Vector2Int position, TileData oldTile, TileData newTile)
        {
            Position = position;
            OldTile = oldTile;
            NewTile = newTile;
        }
    }
}
