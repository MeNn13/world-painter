using UnityEngine;
using WorldPainter.Runtime.Core;
using WorldPainter.Runtime.Providers.Dependencies;
using WorldPainter.Runtime.Providers.MultiTile;
using WorldPainter.Runtime.Providers.Tile;
using WorldPainter.Runtime.Providers.Wall;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Providers
{
    public class WorldFacade : MonoBehaviour, IWorldFacadeEditor
    {
        [Header("Dependencies")] 
        [SerializeField] private TilePool tilePool;
        [SerializeField] private ChunkService chunkService;

        public bool IsInitialized => _multiTileService is not null
                                     && _tileService is not null
                                     && _wallService is not null;

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
            if(IsInitialized) return;
            
            _tileService = new TileService();
            _wallService = new WallService();
            _multiTileService = new MultiTileService();

            var container = new DependencyContainer(
                tileService: _tileService,
                wallService: _wallService,
                multiTileService: _multiTileService,
                chunkService: chunkService,
                tilePool: tilePool,
                worldFacade: this
                );

            InitializeAllComponents(container);
        }

        private void InitializeAllComponents(IDependencyContainer container)
        {
            _tileService.Initialize(container);
            _wallService.Initialize(container);
            _multiTileService.Initialize(container);
            tilePool.Initialize(container);
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
