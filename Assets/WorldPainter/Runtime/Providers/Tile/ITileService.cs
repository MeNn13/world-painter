using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Providers.Tile
{
    public interface ITileService
    {
        void SetTileAt(Vector2Int worldPos, TileData tile);
        TileData GetTileAt(Vector2Int worldPos);
    }
}
