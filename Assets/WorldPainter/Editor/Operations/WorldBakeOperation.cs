using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using WorldPainter.Editor.Coordinators;
using WorldPainter.Editor.Factories;
using WorldPainter.Editor.Processors;
using WorldPainter.Editor.Resolvers;
using WorldPainter.Editor.Systems;
using WorldPainter.Runtime.Chunking;
using TileData = WorldPainter.Runtime.ScriptableObjects.TileData;

namespace WorldPainter.Editor.Operations
{
    public class WorldBakeOperation
    {
        private readonly Tilemap _sourceTilemap;
        private readonly Transform _chunksParent;
        private readonly ChunkFactory _chunkFactory;
        private readonly TileDataResolver _tileDataResolver;
        private readonly ChunkCoordinator _coordinator;
        private readonly ChunkDataProcessor _dataProcessor;
        private readonly ChunkUpdateSystem _updateSystem;
        private readonly List<TileData> _tileDatabase;

        public WorldBakeOperation(Tilemap sourceTilemap, Transform chunksParent, string chunkPrefabPath)
        {
            _sourceTilemap = sourceTilemap;
            _chunksParent = chunksParent;
            _chunkFactory = new ChunkFactory(chunkPrefabPath);
            _tileDatabase = LoadTileDatabase();
            _tileDataResolver = new TileDataResolver(_tileDatabase);
            _coordinator = new ChunkCoordinator();
            _dataProcessor = new ChunkDataProcessor();
            _updateSystem = new ChunkUpdateSystem(_tileDatabase);
        }

        public void BakeWorld()
        {
            Debug.Log("Starting world bake process...");

            ClearBakedData();

            _sourceTilemap.CompressBounds();
            BoundsInt bounds = _sourceTilemap.cellBounds;

            Debug.Log($"Source tilemap bounds: {bounds}");

            CreateChunksFromBounds(bounds);
            FillChunksWithTilemapData(bounds);

            _updateSystem.UpdateDirtyChunks(_chunksParent);

            Debug.Log("World bake completed!");
        }

        private List<TileData> LoadTileDatabase()
        {
            var tileDatabase = new List<TileData>();
            var allTileData = Resources.FindObjectsOfTypeAll<TileData>();
            tileDatabase.AddRange(allTileData);
            Debug.Log($"Loaded {tileDatabase.Count} tile definitions");
            return tileDatabase;
        }

        private void ClearBakedData()
        {
            if (_chunksParent is null) return;

            int childCount = _chunksParent.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(_chunksParent.GetChild(i).gameObject);
            }

            Debug.Log($"Cleared {childCount} chunks");
        }

        private void CreateChunksFromBounds(BoundsInt bounds)
        {
            var (chunksX, chunksY) = _coordinator.CalculateChunkCount(bounds);
            Debug.Log($"Creating {chunksX}x{chunksY} chunks...");

            GameObject chunkPrefab = _chunkFactory.CreateChunkPrefab();

            for (int chunkX = 0; chunkX < chunksX; chunkX++)
            {
                for (int chunkY = 0; chunkY < chunksY; chunkY++)
                {
                    CreateSingleChunk(chunkX, chunkY, bounds, chunkPrefab);
                }
            }
        }

        private void CreateSingleChunk(int chunkX, int chunkY, BoundsInt worldBounds, GameObject chunkPrefab)
        {
            string chunkName = $"Chunk_{chunkX}_{chunkY}";
            GameObject chunkInstance = _chunkFactory.CreateChunkInstance(chunkPrefab, _chunksParent, chunkName);

            Vector3 chunkPosition = _coordinator.CalculateChunkPosition(chunkX, chunkY, worldBounds);
            chunkInstance.transform.position = chunkPosition;

            WorldChunk worldChunk = chunkInstance.GetComponent<WorldChunk>();
            _chunkFactory.InitializeChunkComponents(chunkInstance, worldChunk);

            Debug.Log($"Created chunk at position: {chunkPosition}");
        }

        private void FillChunksWithTilemapData(BoundsInt bounds)
        {
            Debug.Log("Filling chunks with tilemap data...");

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    ProcessTileAtPosition(x, y, bounds);
                }
            }

            _dataProcessor.MarkAllChunksDirty(_chunksParent);
        }

        private void ProcessTileAtPosition(int worldX, int worldY, BoundsInt bounds)
        {
            Vector3Int cellPosition = new Vector3Int(worldX, worldY, 0);
            TileBase sourceTile = _sourceTilemap.GetTile(cellPosition);

            if (sourceTile is null) return;

            TileData tileData = _tileDataResolver.FindTileDataForTile(sourceTile);
            if (tileData is null)
            {
                Debug.LogWarning($"No TileData found for tile at position ({worldX}, {worldY})");
                return;
            }

            WorldChunk targetChunk = FindChunkForWorldPosition(worldX, worldY, bounds);
            if (targetChunk is null) return;

            StoreTileInChunk(targetChunk, worldX, worldY, bounds, tileData);
        }

        private WorldChunk FindChunkForWorldPosition(int worldX, int worldY, BoundsInt worldBounds)
        {
            var (chunkX, chunkY) = _coordinator.WorldToChunkCoords(worldX, worldY, worldBounds);
            string chunkName = $"Chunk_{chunkX}_{chunkY}";
            Transform chunkTransform = _chunksParent.Find(chunkName);
            return chunkTransform?.GetComponent<WorldChunk>();
        }

        private void StoreTileInChunk(WorldChunk chunk, int worldX, int worldY, BoundsInt worldBounds, TileData tileData)
        {
            var (localX, localY) = _coordinator.WorldToLocalCoords(worldX, worldY, worldBounds);
            
            if (!_coordinator.IsPositionInChunkBounds(localX, localY))
            {
                Debug.LogWarning($"Position ({localX}, {localY}) is out of chunk bounds for world position ({worldX}, {worldY})");
                return;
            }

            ushort tileId = _tileDataResolver.GetTileId(tileData, _tileDatabase);
            byte health = _tileDataResolver.CalculateHealth(tileData);

            _dataProcessor.StoreTileInChunk(chunk, localX, localY, tileId, health);
        }
    }
}
