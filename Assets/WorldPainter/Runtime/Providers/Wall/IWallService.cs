using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Providers.Wall
{
    public interface IWallService
    {
        void SetWallAt(Vector2Int worldPos, WallData wall);
        WallData GetWallAt(Vector2Int worldPos);

        bool HasWallInArea(Vector2Int startPos, Vector2Int size);
        bool HasContinuousWall(Vector2Int startPos, Vector2Int direction, int length);
        bool HasWallOfType(Vector2Int position, WallData requiredWall = null);
    }

}
