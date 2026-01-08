using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WorldPainter.Runtime.Core;
using WorldPainter.Runtime.Data;
using WorldPainter.Runtime.Providers.Tile;
using WorldPainter.Runtime.Providers.Wall;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Providers.MultiTile
{
    public class MultiTileDataProvider : BaseWorldDataProvider, IMultiTileDataProvider
    {
        private readonly Dictionary<Vector2Int, Core.MultiTile> _multiTiles = new();
        private readonly Dictionary<Vector2Int, Vector2Int> _positionToMultiTileRoot  = new();
        
        private ITileDataProvider _tileProvider;
        private IWallDataProvider _wallProvider;
        private TilePool _tilePool;
        
        [Obsolete("Obsolete")]
        private void Awake()
        {
            _tileProvider = FindObjectOfType<WorldManager>()?.TileProvider;
            _wallProvider = FindObjectOfType<WorldManager>()?.WallProvider;
            _tilePool = FindObjectOfType<TilePool>();
        }

        public bool CanPlaceMultiTile(MultiTileData data, Vector2Int rootPosition)
        {
            if (data is null) return false;
            
            var occupiedPositions = data.GetAllOccupiedPositions(rootPosition);
            foreach (var pos in occupiedPositions)
            {
                if (_tileProvider?.GetTileAt(pos) is not null)
                    return false;
                
                if (_multiTiles.ContainsKey(pos))
                    return false;
            }
            
            return CheckAttachmentRules(data, rootPosition);
        }
        public bool PlaceMultiTile(MultiTileData data, Vector2Int rootPosition)
        {
            if (!CanPlaceMultiTile(data, rootPosition))
                return false;

            Undo.RegisterCompleteObjectUndo(this, $"Place {data.DisplayName}");

            if (_tilePool is null)
            {
                Debug.LogError("TilePool not found in scene!");
                return false;
            }
            
            Core.MultiTile multiTile = _tilePool.GetMultiTile(data, rootPosition);
            if (multiTile is null)
                return false;
            
            var occupiedPositions = data.GetAllOccupiedPositions(rootPosition);
            foreach (var pos in occupiedPositions)
            {
                _multiTiles[pos] = multiTile;
                _positionToMultiTileRoot[pos] = rootPosition;

                Vector2Int chunkCoord = WorldToChunkCoord(pos);
                Vector2Int localPos = WorldToLocalInChunk(pos);

                ChunkData chunkData = GetOrCreateChunk(chunkCoord);
                chunkData.SetTile(localPos, data);
            }
            
            multiTile.transform.SetParent(transform);

            Debug.Log($"Placed MultiTile {data.DisplayName} at {rootPosition}");
            return true;
        }
        public bool RemoveMultiTileAt(Vector2Int anyPosition)
        {
            if (!_positionToMultiTileRoot.TryGetValue(anyPosition, out Vector2Int rootPosition))
                return false;

            if (!_multiTiles.TryGetValue(anyPosition, out Core.MultiTile multiTile))
                return false;

            Undo.RegisterCompleteObjectUndo(this, $"Remove {multiTile.Data.DisplayName}");
            
            var occupiedPositions = multiTile.GetAllOccupiedPositions();
            foreach (var pos in occupiedPositions)
            {
                _multiTiles.Remove(pos);
                _positionToMultiTileRoot.Remove(pos);
                
                Vector2Int chunkCoord = WorldToChunkCoord(pos);
                Vector2Int localPos = WorldToLocalInChunk(pos);

                if (Chunks.TryGetValue(chunkCoord, out ChunkData chunkData))
                    chunkData.SetTile(localPos, null);
            }

            if (_tilePool is not null)
                _tilePool.ReturnMultiTile(multiTile);
            else
                DestroyImmediate(multiTile.gameObject);

            Debug.Log($"Removed MultiTile {multiTile.Data.DisplayName}");
            return true;
        }
        public Core.MultiTile GetMultiTileAt(Vector2Int position)
        {
            _multiTiles.TryGetValue(position, out Core.MultiTile multiTile);
            return multiTile;
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
                if (_tileProvider?.GetTileAt(groundPos) is null)
                    return false;
            }
            return true;
        }
        private bool CheckCeilingAttachment(MultiTileData data, Vector2Int rootPosition)
        {
            for (int x = 0; x < data.size.x; x++)
            {
                Vector2Int ceilingPos = rootPosition + new Vector2Int(x, data.size.y);
                if (_tileProvider?.GetTileAt(ceilingPos) is null)
                    return false;
            }
            return true;
        }
        private bool CheckWallAttachment(MultiTileData data, Vector2Int rootPosition)
        {
            // TODO: Реализовать проверку крепления к стенам
            Debug.LogWarning("Wall attachment check not fully implemented yet");
            
            // Временная реализация - проверяем наличие любой стены сзади
            if (data.wallAttachmentSide == WallAttachmentSide.Back)
            {
                for (int y = 0; y < data.size.y; y++)
                {
                    Vector2Int wallPos = rootPosition + new Vector2Int(-1, y);
                    if (_wallProvider?.GetWallAt(wallPos) is null)
                        return false;
                }
            }
            
            return true;
        }
        
        public ChunkData GetChunkData(Vector2Int chunkCoord) => GetChunk(chunkCoord);
        public void SetChunkData(Vector2Int chunkCoord, ChunkData chunk) => 
            Chunks[chunkCoord] = chunk;
    }
}
