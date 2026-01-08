using System;
using UnityEditor;
using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;
using WorldPainter.Runtime.Utils;

namespace WorldPainter.Editor.Tools.Painters
{
    public class MultiTilePainter : BasePainter
    {
        public MultiTileData SelectedMultiTile
        {
            get => _selectedMultiTile;
            set
            {
                if (_selectedMultiTile != value)
                {
                    _selectedMultiTile = value;
                    Cleanup();
                }
            }
        }
        
        public PaintMode Mode { get; set; } = PaintMode.Paint;
        
        private MultiTileData _selectedMultiTile;
        private Vector2Int _lastPreviewPosition;
        private bool _lastPlacementValid;

        public override void HandleInput(SceneView sceneView)
        {
            Event e = Event.current;
            if (!e.control 
                || e.type is not EventType.MouseDown 
                && e.type is not EventType.MouseDrag)
                return;
                
            Vector3 worldPoint = GetMouseWorldPosition();
            Vector2Int gridPos = CalculateGridPosition(worldPoint, false);
            
            if (WorldProvider is null || _selectedMultiTile is null)
                return;
                
            switch (Mode)
            {
                case PaintMode.Paint:
                    bool canPlace = WorldProvider.CanPlaceMultiTile(_selectedMultiTile, gridPos);
                    if (canPlace && WorldProvider.PlaceMultiTile(_selectedMultiTile, gridPos))
                        Debug.Log($"Placed {_selectedMultiTile.DisplayName} at {gridPos}");
                    else if (!canPlace)
                        Debug.LogWarning($"Cannot place {_selectedMultiTile.DisplayName} at {gridPos}");
                    break;
                    
                case PaintMode.Erase:
                    if (WorldProvider.RemoveMultiTileAt(gridPos))
                        Debug.Log($"Removed multi tile at {gridPos}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            e.Use();
        }
        
        public override void DrawPreview()
        {
            if (_selectedMultiTile?.DefaultSprite is null)
            {
                Cleanup();
                return;
            }
            
            Vector3 worldPoint = GetMouseWorldPosition();
            Vector2Int newPos = CalculateGridPosition(worldPoint, false);
            
            if (_lastPreviewPosition != newPos)
            {
                _lastPreviewPosition = newPos;
                _lastPlacementValid = WorldProvider?.CanPlaceMultiTile(_selectedMultiTile, newPos) ?? false;
            }
            
            Vector3 bottomLeftPosition = WorldGrid.GridToWorldPosition(newPos, false) - new Vector3(0.5f, 0.5f, 0);
            Vector3 spritePosition = bottomLeftPosition + new Vector3(
                _selectedMultiTile.pivotOffset.x,
                _selectedMultiTile.pivotOffset.y,
                0);
            
            PreviewManager.GetOrCreateSpriteRenderer("multitile");
            PreviewManager.SetPreviewTransform("multitile", spritePosition);
            PreviewManager.SetPreviewSprite("multitile", _selectedMultiTile.DefaultSprite,
                _lastPlacementValid ? new Color(1, 1, 1, 0.6f) : new Color(1, 0.5f, 0.5f, 0.6f));
            
            DrawPreviewVisuals(bottomLeftPosition, spritePosition);
        }
        
        private void DrawPreviewVisuals(Vector3 bottomLeftPosition, Vector3 spritePosition)
        {
            Vector3 tileAreaCenter = bottomLeftPosition + new Vector3(
                _selectedMultiTile.pivotOffset.x,
                _selectedMultiTile.pivotOffset.y,
                0);
            
            Handles.color = _lastPlacementValid ? Color.green : Color.red;
            Handles.DrawWireCube(tileAreaCenter, 
                new Vector3(_selectedMultiTile.size.x, _selectedMultiTile.size.y, 0));
            
            // Точка привязки
            Handles.color = Color.cyan;
            Handles.DrawSolidDisc(spritePosition, Vector3.forward, 0.1f);
            
            // Левый нижний угол
            Handles.color = Color.yellow;
            Handles.DrawWireCube(bottomLeftPosition, new Vector3(0.1f, 0.1f, 0));
            
            // Сетка клеток
            if (_selectedMultiTile.size.x > 1 || _selectedMultiTile.size.y > 1)
            {
                Handles.color = _lastPlacementValid ? 
                    new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);
                    
                for (int x = 0; x < _selectedMultiTile.size.x; x++)
                    for (int y = 0; y < _selectedMultiTile.size.y; y++)
                    {
                        Vector3 cellCenter = bottomLeftPosition + new Vector3(x + 0.5f, y + 0.5f, 0);
                        Handles.DrawWireCube(cellCenter, new Vector3(0.95f, 0.95f, 0));
                    }
            }
        }
        
        public override void Cleanup()
        {
            PreviewManager.DestroyPreview("multi tile");
            _lastPreviewPosition = Vector2Int.zero;
            _lastPlacementValid = false;
        }
    }
}