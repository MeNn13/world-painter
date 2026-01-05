using UnityEngine;

namespace WorldPainter.Runtime.Interfaces
{
    public interface ITileVisualProvider
    {
        GameObject GetTileVisual(string tileId, Vector3 position);
    }
}
