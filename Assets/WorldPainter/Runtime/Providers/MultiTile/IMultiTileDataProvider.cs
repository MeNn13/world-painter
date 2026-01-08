using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Providers.MultiTile
{
    public interface IMultiTileDataProvider
    {
        bool CanPlaceMultiTile(MultiTileData data, Vector2Int rootPosition);
        bool PlaceMultiTile(MultiTileData data, Vector2Int rootPosition);
        bool RemoveMultiTileAt(Vector2Int anyPosition);
        Core.MultiTile GetMultiTileAt(Vector2Int position);
    }

}
