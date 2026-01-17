using System;
using System.Collections.Generic;
using UnityEngine;
using WorldPainter.Runtime.Core;
using WorldPainter.Runtime.Providers.Dependencies;
using WorldPainter.Runtime.Providers.Tile;
using WorldPainter.Runtime.Providers.Wall;
using WorldPainter.Runtime.ScriptableObjects;
using WorldPainter.Runtime.Utils;

namespace WorldPainter.Runtime.Providers.MultiTile
{
    public class MultiTileService : IMultiTileService, IRequiresDependencies
    {
        private readonly Dictionary<Vector2Int, Core.MultiTile> _multiTiles = new();
        private readonly Dictionary<Vector2Int, Vector2Int> _positionToRoot = new();

        private ITileService _tileService;
        private IWallService _wallService;
        private ChunkService _chunkService;
        private TilePool _tilePool;

        private bool _dependenciesInjected;

        public void InjectDependencies(IDependencyContainer container)
        {
            if (_dependenciesInjected) return;

            _tileService = container?.TileService;
            _wallService = container?.WallService;
            _tilePool = container?.TilePool;
            _chunkService = container?.ChunkService;

            _dependenciesInjected = true;
        }
        private void ValidateDependencies()
        {
            if (!_dependenciesInjected)
            {
                Debug.LogError("Dependencies not injected for MultiTileService!");
                throw new InvalidOperationException(
                    "MultiTileService requires dependencies to be injected via IRequiresDependencies");
            }
        }

        public bool TrySetMultiTile(MultiTileData data, Vector2Int rootPosition)
        {
            ValidateDependencies();
            
            if (data is null) return false;

            if (!CanPlaceMultiTile(data, rootPosition))
                return false;

            Core.MultiTile multiTile = _tilePool.GetMultiTile(data, rootPosition);
            if (multiTile is null)
            {
                Debug.LogError($"Failed to get MultiTile from pool for {data.DisplayName}");
                return false;
            }

            _multiTiles[rootPosition] = multiTile;

            var occupiedPositions = data.GetAllOccupiedPositions(rootPosition);
            foreach (var pos in occupiedPositions)
            {
                _positionToRoot[pos] = rootPosition;
                AddMultiTileReferenceToChunks(pos, rootPosition);
            }

            multiTile.transform.SetParent(_chunkService.transform);

            Debug.Log($"Placed MultiTile '{data.DisplayName}' at {rootPosition} (size: {data.size})");
            return true;
        }
        public bool RemoveMultiTileAt(Vector2Int position)
        {
            ValidateDependencies();

            if (!_positionToRoot.TryGetValue(position, out Vector2Int rootPosition))
                return false;

            if (!_multiTiles.TryGetValue(rootPosition, out Core.MultiTile multiTile))
                return false;

            var occupiedPositions = multiTile.GetAllOccupiedPositions();
            foreach (var pos in occupiedPositions)
            {
                _positionToRoot.Remove(pos);

                RemoveMultiTileReferenceFromChunks(pos, rootPosition);
            }

            _multiTiles.Remove(rootPosition);

            _tilePool.ReturnMultiTile(multiTile);

            Debug.Log($"Removed MultiTile {multiTile.Data.DisplayName}");
            return true;
        }
        public Core.MultiTile GetMultiTileAt(Vector2Int position)
        {
            if (_positionToRoot.TryGetValue(position, out Vector2Int rootPosition) && _multiTiles.TryGetValue(rootPosition, out Core.MultiTile multiTile))
            {
                return multiTile;
            }
            return null;
        }

        private bool CanPlaceMultiTile(MultiTileData data, Vector2Int rootPosition)
        {
            ValidateDependencies();

            if (data is null) return false;

            var occupiedPositions = data.GetAllOccupiedPositions(rootPosition);
            foreach (var pos in occupiedPositions)
            {
                if (_tileService?.GetTileAt(pos) is not null)
                    return false;

                if (_positionToRoot.ContainsKey(pos))
                    return false;
            }

            return CheckAttachmentRules(data, rootPosition);
        }
        private bool CheckAttachmentRules(MultiTileData data, Vector2Int rootPosition) =>
            data.attachmentType switch
            {
                AttachmentType.None => true,
                AttachmentType.Ground => CheckGroundAttachment(data, rootPosition),
                AttachmentType.Ceiling => CheckCeilingAttachment(data, rootPosition),
                AttachmentType.Wall => CheckWallAttachment(data, rootPosition),
                AttachmentType.GroundAndCeiling => CheckGroundAttachment(data, rootPosition)
                                                   && CheckCeilingAttachment(data, rootPosition),
                _ => true
            };
        private bool CheckGroundAttachment(MultiTileData data, Vector2Int rootPosition)
        {
            for (int x = 0; x < data.size.x; x++)
            {
                Vector2Int groundPos = rootPosition + new Vector2Int(x, -1);
                if (_tileService?.GetTileAt(groundPos) is null)
                    return false;
            }
            return true;
        }
        private bool CheckCeilingAttachment(MultiTileData data, Vector2Int rootPosition)
        {
            for (int x = 0; x < data.size.x; x++)
            {
                Vector2Int ceilingPos = rootPosition + new Vector2Int(x, data.size.y);
                if (_tileService?.GetTileAt(ceilingPos) is null)
                    return false;
            }
            return true;
        }
        private bool CheckWallAttachment(MultiTileData data, Vector2Int rootPosition)
        {
            // TODO: Полная реализация проверки стен
            // Пока базовая проверка для WallAttachmentSide.Back
            if (data.wallAttachmentSide == WallAttachmentSide.Back)
            {
                for (int y = 0; y < data.size.y; y++)
                {
                    Vector2Int wallPos = rootPosition + new Vector2Int(-1, y);
                    if (_wallService?.GetWallAt(wallPos) is null)
                    {
                        Debug.Log($"No wall at {wallPos} for MultiTile");
                        return false;
                    }
                }
                return true;
            }

            Debug.LogWarning($"Wall attachment side {data.wallAttachmentSide} not fully implemented");
            return true; // Временно разрешаем
        }
        private void AddMultiTileReferenceToChunks(Vector2Int worldPos, Vector2Int rootPosition)
        {
            if (_chunkService is null) return;
            
            Vector2Int chunkCoord = WorldGrid.WorldToChunkCoord(worldPos);
            var chunkData = _chunkService.GetChunkData(chunkCoord);
            chunkData?.AddMultiTileReference(rootPosition);
        }
        private void RemoveMultiTileReferenceFromChunks(Vector2Int worldPos, Vector2Int rootPosition)
        {
            if (_chunkService is null) return;
            
            Vector2Int chunkCoord = WorldGrid.WorldToChunkCoord(worldPos);
            var chunkData = _chunkService.GetChunkData(chunkCoord);
            chunkData?.RemoveMultiTileReference(rootPosition);
        }
    }
}
