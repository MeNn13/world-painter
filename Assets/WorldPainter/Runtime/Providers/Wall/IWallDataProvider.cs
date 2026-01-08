using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Providers.Wall
{
    public interface IWallDataProvider
    {
        WallData GetWallAt(Vector2Int worldPos);
        void SetWallAt(Vector2Int worldPos, WallData wall);
        WallData SetWallAtWithUndo(Vector2Int worldPos, WallData wall);
        
        bool HasWallInArea(Vector2Int startPos, Vector2Int size);
        bool HasContinuousWall(Vector2Int startPos, Vector2Int direction, int length);
        bool HasWallOfType(Vector2Int position, WallData requiredWall = null);
    }

}
