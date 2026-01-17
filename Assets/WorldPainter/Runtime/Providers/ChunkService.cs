using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WorldPainter.Runtime.Core;
using WorldPainter.Runtime.Data;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Providers
{
    public class ChunkService : MonoBehaviour
    {
        [SerializeField] private TilePool tilePool;
        
        private readonly Dictionary<Vector2Int, ChunkData> _chunksData = new();
        private readonly Dictionary<Vector2Int, ChunkView> _chunksView = new();
        
        #region Tile
        
        public void SetTileInChunk(Vector2Int chunkCoord, Vector2Int localPos, TileData tile, IWorldFacade worldProvider)
        {
            ChunkData data = GetOrCreateChunkData(chunkCoord);
            data.SetTile(localPos, tile);
            
            ChunkView view = GetOrCreateChunkView(chunkCoord, data, worldProvider);
            view.UpdateChunk(tile, localPos);
            
            if (tile is null)
                TryRemoveEmptyChunk(chunkCoord);
        }
        public TileData GetTileDataFromChunk(Vector2Int chunkCoord, Vector2Int localPos) => 
                    _chunksData.TryGetValue(chunkCoord, out ChunkData data) ? data.GetTile(localPos) : null;
        public bool TryGetTileView(Vector2Int chunkCoord, Vector2Int localPos, out TileView tileView)
                {
                    tileView = null;
            
                    if (_chunksView.TryGetValue(chunkCoord, out ChunkView chunkView))
                    {
                        tileView = chunkView.GetTileViewAtLocalPos(localPos);
                        return tileView is not null;
                    }
            
                    return false;
                }
        
        public void SetWallInChunk(Vector2Int chunkCoord, Vector2Int localPos, WallData wall, IWorldFacade worldProvider)
        {
            ChunkData data = GetOrCreateChunkData(chunkCoord);
            data.SetWall(localPos, wall);
            
            ChunkView view = GetOrCreateChunkView(chunkCoord, data, worldProvider);
            view.UpdateChunk(wall, localPos);
            
            if (wall is null)
                TryRemoveEmptyChunk(chunkCoord);
        }
        public WallData GetWallDataFromChunk(Vector2Int chunkCoord, Vector2Int localPos) => 
            _chunksData.TryGetValue(chunkCoord, out ChunkData data) ? data.GetWall(localPos) : null;
        
        
        #endregion

        #region MultiTile

        public ChunkData GetChunkData(Vector2Int chunkCoord)
        {
            return _chunksData.GetValueOrDefault(chunkCoord);
        }
        public bool HasChunkAt(Vector2Int chunkCoord) => _chunksData.ContainsKey(chunkCoord);
        
        #endregion
        
        #region Внутренние методы
        
        private ChunkData GetOrCreateChunkData(Vector2Int coord)
        {
            if (!_chunksData.TryGetValue(coord, out var data))
            {
                data = new ChunkData(coord);
                _chunksData[coord] = data;
            }
            return data;
        }
        
        private ChunkView GetOrCreateChunkView(Vector2Int coord, ChunkData data, IWorldFacade worldFacade)
        {
            if (!_chunksView.TryGetValue(coord, out var view))
            {
                GameObject chunkObj = new GameObject($"Chunk_{coord.x}_{coord.y}");
                chunkObj.transform.SetParent(transform);
                
                view = chunkObj.AddComponent<ChunkView>();
                view.Initialize(data, tilePool, worldFacade);
                
                _chunksView[coord] = view;
            }
            return view;
        }
        
        private void TryRemoveEmptyChunk(Vector2Int coord)
        {
            if (!_chunksData.TryGetValue(coord, out var data) || 
                (!data.IsEmpty(coord) && !data.HasMultiTileReferences))
                return;
            
            if (_chunksView.TryGetValue(coord, out var view))
            {
                Destroy(view.gameObject);
                _chunksView.Remove(coord);
            }
                
            _chunksData.Remove(coord);
        }
        
        #endregion
        
        #region Вспомогательные методы
        
        public bool HasChunkData(Vector2Int coord) => _chunksData.ContainsKey(coord);
        
        public bool HasChunkView(Vector2Int coord) => _chunksView.ContainsKey(coord);
        
        public int GetChunkDataCount() => _chunksData.Count;
        
        public int GetChunkViewCount() => _chunksView.Count;
        
        #endregion
        
        private void OnDestroy()
        {
            foreach (ChunkView view in _chunksView.Values.Where(view => view is not null))
                Destroy(view.gameObject);
            
            _chunksView.Clear();
            _chunksData.Clear();
        }
    }
}