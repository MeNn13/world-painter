using UnityEngine;
using WorldPainter.Runtime.Data;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Providers
{
    public interface IWorldDataProvider
    {
        TileData GetTileAt(Vector2Int worldPos);
        void SetTileAt(Vector2Int worldPos, TileData tile);
        
        ChunkData GetChunkData(Vector2Int chunkCoord);
        void SetChunkData(Vector2Int chunkCoord, ChunkData chunk);
        
        TileData SetTileAtWithUndo(Vector2Int worldPos, TileData tile);
        bool CanPlaceMultiTile(MultiTileData data, Vector2Int rootPosition);
        bool PlaceMultiTile(MultiTileData data, Vector2Int rootPosition);
        bool RemoveMultiTileAt(Vector2Int anyPosition);
    }
}
