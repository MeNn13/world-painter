using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Providers.MultiTile
{
    public interface IMultiTileDataProvider
    {
        bool TrySetMultiTile(MultiTileData data, Vector2Int rootPosition);
        Core.MultiTile GetMultiTileAt(Vector2Int position);
        bool RemoveMultiTileAt(Vector2Int anyPosition);
    }

}
