using UnityEngine;
using WorldPainter.Runtime.Data;
using WorldPainter.Runtime.Providers.MultiTile;
using WorldPainter.Runtime.Providers.Tile;
using WorldPainter.Runtime.Providers.Wall;

namespace WorldPainter.Runtime.Providers
{
    public interface IWorldDataProvider : ITileDataProvider, IWallDataProvider, IMultiTileDataProvider
    {
        ChunkData GetChunkData(Vector2Int chunkCoord);
        void SetChunkData(Vector2Int chunkCoord, ChunkData chunk);
    }

}
