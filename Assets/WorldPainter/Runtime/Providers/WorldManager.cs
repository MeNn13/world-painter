using System;
using UnityEngine;
using WorldPainter.Runtime.Core;
using WorldPainter.Runtime.Data;
using WorldPainter.Runtime.Providers.MultiTile;
using WorldPainter.Runtime.Providers.Tile;
using WorldPainter.Runtime.Providers.Wall;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Providers
{
    public class WorldManager : MonoBehaviour, IWorldDataProvider
    {
        [Header("Data Providers")] [SerializeField] private TileDataProvider tileDataProvider;
        [SerializeField] private WallDataProvider wallDataProvider;
        [SerializeField] private MultiTileDataProvider multiTileDataProvider;

        [Header("Dependencies")] [SerializeField] private TilePool tilePool;

        public ITileDataProvider TileProvider => tileDataProvider;
        public IWallDataProvider WallProvider => wallDataProvider;
        public IMultiTileDataProvider MultiTileProvider => multiTileDataProvider;

        private void Awake()
        {
            InitializeProviders();
            LinkProviders();
        }

        [Obsolete("Obsolete")]
        private void InitializeProviders()
        {
            tileDataProvider ??= GetComponentInChildren<TileDataProvider>();
            wallDataProvider ??= GetComponentInChildren<WallDataProvider>();
            multiTileDataProvider ??= GetComponentInChildren<MultiTileDataProvider>();
            tilePool ??= FindObjectOfType<TilePool>();
        }

        private void LinkProviders()
        {
            // Можно добавить связи между провайдерами при необходимости
            // Например, MultiTileDataProvider может нуждаться в ссылках на другие провайдеры
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
