using UnityEngine;
using WorldPainter.Runtime.Data;
using WorldPainter.Runtime.Providers;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Core
{
    public class ChunkView : MonoBehaviour
    {
        private readonly TileView[,] _tiles = new TileView[ChunkData.SIZE, ChunkData.SIZE];
        private readonly TileView[,] _walls = new TileView[ChunkData.SIZE, ChunkData.SIZE];

        private IWorldFacade _worldProvider;
        private TilePool _tilePool;
        private Vector2Int _chunkCoord;
        
        public void Initialize(ChunkData data, TilePool tilePool , IWorldFacade worldManager)
        {
            _chunkCoord = data.ChunkCoords;
            _tilePool =  tilePool;
            _worldProvider = worldManager;

            transform.position = new Vector3(
                data.ChunkCoords.x * ChunkData.SIZE,
                data.ChunkCoords.y * ChunkData.SIZE,
                0);

            RenderChunk(data);
        }
        
        public TileView GetTileViewAtLocalPos(Vector2Int localPos)
        {
            if (localPos.x is >= 0 and < ChunkData.SIZE && localPos.y is >= 0 and < ChunkData.SIZE)
                return _tiles[localPos.x, localPos.y];
            return null;
        }
        public void UpdateChunk(TileData tileData, Vector2Int localPos)
        {
            if (tileData is MultiTileData) return;
            
            bool isWall = tileData is WallData;
            var targetArray = isWall ? _walls : _tiles;
            
            RemoveFromArray(targetArray, localPos);
            CreateNewTile();

            return;

            void CreateNewTile()
            {
                if (tileData is not null)
                {
                    TileView tileView = _tilePool?.GetTile(tileData, LocalToWorldPosition(localPos));
                    tileView?.Initialize(tileData, LocalToWorldPosition(localPos), _worldProvider);
                    tileView?.transform.SetParent(transform);
                    
                    if (isWall)
                        _walls[localPos.x, localPos.y] = tileView;
                    else
                        _tiles[localPos.x, localPos.y] = tileView;
                }
            }
        }
        private void RemoveFromArray(TileView[,] array, Vector2Int localPos)
        {
            if (array[localPos.x, localPos.y] is not null)
            {
                _tilePool?.ReturnTile(array[localPos.x, localPos.y]);
                array[localPos.x, localPos.y] = null;
            }
        }

        private void RenderChunk(ChunkData data)
        {
            for (int x = 0; x < ChunkData.SIZE; x++)
                for (int y = 0; y < ChunkData.SIZE; y++)
                {
                    Vector2Int localPos = new Vector2Int(x, y);
                    TileData tileData = data.GetTile(localPos);
                    
                    if (tileData is not null && tileData is not MultiTileData)
                        UpdateChunk(tileData, localPos);
                }
        }
        private Vector2Int LocalToWorldPosition(Vector2Int localPos)
        {
            return new Vector2Int(
                _chunkCoord.x * ChunkData.SIZE + localPos.x,
                _chunkCoord.y * ChunkData.SIZE + localPos.y);
        }
    }
}
