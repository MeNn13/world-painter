using UnityEngine;
using WorldPainter.Runtime.Core;
using WorldPainter.Runtime.Providers.Dependencies;
using WorldPainter.Runtime.Providers.MultiTile;
using WorldPainter.Runtime.Providers.Tile;
using WorldPainter.Runtime.Providers.Wall;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Providers
{
    public class WorldFacade : MonoBehaviour, IWorldFacadeEditor, IDependencyContainer
    {
        [Header("Dependencies")]
        [SerializeField] private TilePool tilePool;
        [SerializeField] private ChunkService chunkService;

        public bool IsInitialized { get; private set; }
        public ITileService TileService => _tileService;
        public IWallService WallService => _wallService;
        public ChunkService ChunkService => chunkService;
        public IMultiTileService MultiTileProvider => _multiTileService;
        public TilePool TilePool => tilePool;
        
        private TileService _tileService;
        private WallService _wallService;
        private MultiTileService _multiTileService;

        private void Awake()
        {
            if (!Application.isPlaying) return;
            InitializeForEditor();
        }
        
        public void InitializeForEditor()
        {
            if (IsInitialized) return;
        
            ValidateAndSetup();
            InjectDependencies();
            
            IsInitialized = true;
        }

        private void ValidateAndSetup()
        {
            tilePool ??= GetComponentInChildren<TilePool>(true);
            
            _tileService ??= new TileService(this, chunkService);
            _wallService ??= new WallService(this, chunkService);
            _multiTileService ??= new MultiTileService();
            
            Debug.Assert(_tileService is not null, "TileService is required!");
            Debug.Assert(_wallService is not null, "WallService is required!");
            Debug.Assert(_multiTileService is not null, "MultiTileService is required!");
            Debug.Assert(tilePool is not null, "TilePool is required!");
            Debug.Assert(chunkService is not null, "ChunkService is required!");
        }
        private void InjectDependencies()
        {
            IRequiresDependencies multiTile = _multiTileService;
            multiTile?.InjectDependencies(this);
        }
        
        #region TileView
        
        public void SetTileAt(Vector2Int worldPos, TileData tile) =>
            _tileService?.SetTileAt(worldPos, tile);
        
        public TileData GetTileAt(Vector2Int worldPos) =>
            _tileService?.GetTileAt(worldPos);
        

        #endregion

        #region Wall

        public WallData GetWallAt(Vector2Int worldPos) =>
            _wallService?.GetWallAt(worldPos);

        public void SetWallAt(Vector2Int worldPos, WallData wall) =>
            _wallService?.SetWallAt(worldPos, wall);

        public bool HasWallInArea(Vector2Int startPos, Vector2Int size) =>
            _wallService?.HasWallInArea(startPos, size) ?? false;

        public bool HasContinuousWall(Vector2Int startPos, Vector2Int direction, int length) =>
            _wallService?.HasContinuousWall(startPos, direction, length) ?? false;

        public bool HasWallOfType(Vector2Int position, WallData requiredWall = null) =>
            _wallService?.HasWallOfType(position, requiredWall) ?? false;

        #endregion

        #region MultiTile

        public bool TrySetMultiTile(MultiTileData data, Vector2Int rootPosition) =>
            _multiTileService?.TrySetMultiTile(data, rootPosition) ?? false;

        public bool RemoveMultiTileAt(Vector2Int position) =>
            _multiTileService?.RemoveMultiTileAt(position) ?? false;

        public Core.MultiTile GetMultiTileAt(Vector2Int position) =>
            _multiTileService?.GetMultiTileAt(position);

        #endregion
    }
}
