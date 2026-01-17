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

        public void SetTileInChunk(Vector2Int chunkCoord, Vector2Int localPos, TileData tileData, IWorldFacade worldProvider)
        {
            ChunkData data = GetOrCreateChunkData(chunkCoord);
            data.SetTile(localPos, tileData);

            if (_chunksView.TryGetValue(chunkCoord, out var view) && view != null)
                view.UpdateChunk(tileData, localPos);

            if (tileData is null)
            {
                TryRemoveEmptyChunk(chunkCoord);
            }
            else if (view == null)
            {
                view = GetOrCreateChunkView(chunkCoord, data, worldProvider);
                if (view != null)
                    view.UpdateChunk(tileData, localPos);
            }
        }
        public TileData GetTileDataFromChunk(Vector2Int chunkCoord, Vector2Int localPos) =>
            _chunksData.TryGetValue(chunkCoord, out ChunkData data) ? data.GetTile(localPos) : null;
        public bool TryGetTileView(Vector2Int chunkCoord, Vector2Int localPos, out TileView tileView)
        {
            tileView = null;

            if (_chunksView.TryGetValue(chunkCoord, out ChunkView chunkView) && chunkView != null)
            {
                tileView = chunkView.GetTileViewAtLocalPos(localPos);
                return tileView != null;
            }

            return false;
        }

        public void SetWallInChunk(Vector2Int chunkCoord, Vector2Int localPos, WallData wallData, IWorldFacade worldProvider)
        {
            ChunkData data = GetOrCreateChunkData(chunkCoord);
            data.SetWall(localPos, wallData);

            if (_chunksView.TryGetValue(chunkCoord, out var view) && view != null)
                view.UpdateChunk(wallData, localPos);

            if (wallData is null)
            {
                TryRemoveEmptyChunk(chunkCoord);
            }
            else if (view == null)
            {
                view = GetOrCreateChunkView(chunkCoord, data, worldProvider);
                if (view != null)
                    view.UpdateChunk(wallData, localPos);
            }
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

        private ChunkView GetOrCreateChunkView(Vector2Int chunkCoord, ChunkData data, IWorldFacade worldFacade)
        {
            if (data.IsChunkEmpty())
                return null;

            if (_chunksView.TryGetValue(chunkCoord, out var view) && view != null)
                return view;

            _chunksView.Remove(chunkCoord);

            view = CreateNewChunkView();
            _chunksView[chunkCoord] = view;
            return view;

            ChunkView CreateNewChunkView()
            {
                GameObject chunkObj = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
                chunkObj.transform.SetParent(transform);
                var chunkView = chunkObj.AddComponent<ChunkView>();
                chunkView.Initialize(data, tilePool, worldFacade);
                return chunkView;
            }
        }

        private void TryRemoveEmptyChunk(Vector2Int chunkCoord)
        {
            if (!_chunksData.TryGetValue(chunkCoord, out var data))
                return;

            if (!data.IsChunkEmpty())
                return;

            if (_chunksView.TryGetValue(chunkCoord, out var view) && view != null)
                if (!Application.isPlaying)
                    DestroyImmediate(view.gameObject);
                else
                    Destroy(view.gameObject);

            _chunksView.Remove(chunkCoord);
            _chunksData.Remove(chunkCoord);
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
