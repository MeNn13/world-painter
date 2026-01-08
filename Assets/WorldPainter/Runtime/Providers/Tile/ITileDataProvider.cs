using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Providers.Tile
{
    public interface ITileDataProvider
    {
        TileData GetTileAt(Vector2Int worldPos);
        void SetTileAt(Vector2Int worldPos, TileData tile);
        TileData SetTileAtWithUndo(Vector2Int worldPos, TileData tile);
    }

}
