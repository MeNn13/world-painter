using UnityEngine;

namespace WorldPainter.Runtime.Interfaces
{
    public interface IWorldDataProvider
    {
        string GetTileIdAt(Vector2Int position);
        void SetTileIdAt(Vector2Int position, string tileId);
    }
}
