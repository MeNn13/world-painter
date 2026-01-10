using System;
using UnityEngine;
using WorldPainter.Runtime.Core;
using WorldPainter.Runtime.Data;
using WorldPainter.Runtime.Providers.Dependencies;
using WorldPainter.Runtime.Providers.MultiTile;
using WorldPainter.Runtime.Providers.Tile;
using WorldPainter.Runtime.Providers.Wall;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Providers
{
    public class WorldManager : MonoBehaviour, IWorldDataProvider, IWorldDataProviderEditor, IDependencyContainer
    {
        [Header("Data Providers")] 
        [SerializeField] private TileDataProvider tileDataProvider;
        [SerializeField] private WallDataProvider wallDataProvider;
        [SerializeField] private MultiTileDataProvider multiTileDataProvider;

        [Header("Dependencies")]
        [SerializeField] private TilePool tilePool;

        private bool _initialized = false;
        public bool IsInitialized => _initialized;
        public ITileDataProvider TileProvider => tileDataProvider;
        public IWallDataProvider WallProvider => wallDataProvider;
        public IMultiTileDataProvider MultiTileProvider => multiTileDataProvider;
        public TilePool TilePool => tilePool;

        private void Awake()
        {
            if (!Application.isPlaying) return;
            InitializeForEditor();
        }
        public void InitializeForEditor()
        {
            if (_initialized) return;
        
            ValidateAndSetup();
            InjectDependencies();
            _initialized = true;
        }

        private void ValidateAndSetup()
        {
            tileDataProvider ??= GetComponent<TileDataProvider>();
            wallDataProvider ??= GetComponent<WallDataProvider>();
            multiTileDataProvider ??= GetComponent<MultiTileDataProvider>();
            tilePool ??= GetComponentInChildren<TilePool>(true);
            
            tileDataProvider ??= gameObject.AddComponent<TileDataProvider>();
            wallDataProvider ??= gameObject.AddComponent<WallDataProvider>();
            multiTileDataProvider ??= gameObject.AddComponent<MultiTileDataProvider>();
            
            Debug.Assert(tileDataProvider is not null, "TileDataProvider is required!");
            Debug.Assert(wallDataProvider is not null, "WallDataProvider is required!");
            Debug.Assert(multiTileDataProvider is not null, "MultiTileDataProvider is required!");
            Debug.Assert(tilePool is not null, "TilePool is required!");
        }
        private void InjectDependencies()
        {
            IRequiresDependencies multiTile = multiTileDataProvider;
            multiTile?.InjectDependencies(this);
        }
        
        #region Tile
        
        public TileData GetTileAt(Vector2Int worldPos) =>
            tileDataProvider?.GetTileAt(worldPos);

        public void SetTileAt(Vector2Int worldPos, TileData tile) =>
            tileDataProvider?.SetTileAt(worldPos, tile);

        public TileData SetTileAtWithUndo(Vector2Int worldPos, TileData tile) =>
            tileDataProvider?.SetTileAtWithUndo(worldPos, tile);

        #endregion

        #region Wall

        public WallData GetWallAt(Vector2Int worldPos) =>
            wallDataProvider?.GetWallAt(worldPos);

        public void SetWallAt(Vector2Int worldPos, WallData wall) =>
            wallDataProvider?.SetWallAt(worldPos, wall);

        public WallData SetWallAtWithUndo(Vector2Int worldPos, WallData wall) =>
            wallDataProvider?.SetWallAtWithUndo(worldPos, wall);

        public bool HasWallInArea(Vector2Int startPos, Vector2Int size) =>
            wallDataProvider?.HasWallInArea(startPos, size) ?? false;

        public bool HasContinuousWall(Vector2Int startPos, Vector2Int direction, int length) =>
            wallDataProvider?.HasContinuousWall(startPos, direction, length) ?? false;

        public bool HasWallOfType(Vector2Int position, WallData requiredWall = null) =>
            wallDataProvider?.HasWallOfType(position, requiredWall) ?? false;

        #endregion

        #region MultiTile

        public bool CanPlaceMultiTile(MultiTileData data, Vector2Int rootPosition) =>
            multiTileDataProvider?.CanPlaceMultiTile(data, rootPosition) ?? false;

        public bool PlaceMultiTile(MultiTileData data, Vector2Int rootPosition) =>
            multiTileDataProvider?.PlaceMultiTile(data, rootPosition) ?? false;

        public bool RemoveMultiTileAt(Vector2Int anyPosition) =>
            multiTileDataProvider?.RemoveMultiTileAt(anyPosition) ?? false;

        public Core.MultiTile GetMultiTileAt(Vector2Int position) =>
            multiTileDataProvider?.GetMultiTileAt(position);

        #endregion

        public void SetTilePool(TilePool pool)
        {
            tilePool = pool;
            // TODO: Передать pool провайдерам, которые в нем нуждаются
        }

        public ChunkData GetChunkData(Vector2Int chunkCoord) =>
            tileDataProvider?.GetChunkData(chunkCoord);

        public void SetChunkData(Vector2Int chunkCoord, ChunkData chunk) =>
            tileDataProvider?.SetChunkData(chunkCoord, chunk);
    }
}
