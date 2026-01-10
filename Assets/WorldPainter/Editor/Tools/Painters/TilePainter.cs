using System;
using UnityEditor;
using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;
using WorldPainter.Runtime.Utils;

namespace WorldPainter.Editor.Tools.Painters
{
    public class TilePainter : BasePainter
    {
        private TileData _selectedTile;

        public TileData SelectedTile
        {
            get => _selectedTile;
            set
            {
                if (_selectedTile != value)
                {
                    _selectedTile = value;
                    Cleanup();
                }
            }
        }
        
        public PaintMode Mode { get; set; } = PaintMode.Paint;

        public override void HandleInput(SceneView sceneView)
        {
            Event e = Event.current;
            if (!e.control 
                || e.type is not EventType.MouseDown 
                && e.type is not EventType.MouseDrag)
                return;
                
            Vector3 worldPoint = GetMouseWorldPosition();
            Vector2Int gridPos = CalculateGridPosition(worldPoint);
            
            if (WorldProvider == null)
                return;
                
            switch (Mode)
            {
                case PaintMode.Paint:
                    if (_selectedTile is not null)
                    {
                        WorldProvider.SetTileAt(gridPos, _selectedTile);
                        Debug.Log($"Painted {_selectedTile.DisplayName} at {gridPos}");
                        e.Use();
                    }
                    break;
                    
                case PaintMode.Erase:
                    WorldProvider.SetTileAt(gridPos, null);
                    Debug.Log($"Erased tile at {gridPos}");
                    e.Use();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public override void DrawPreview()
        {
            if (_selectedTile?.DefaultSprite is null)
            {
                Cleanup();
                return;
            }
            
            Vector3 worldPoint = GetMouseWorldPosition();
            Vector2Int previewPos = CalculateGridPosition(worldPoint);
            Vector3 worldPosition = WorldGrid.GridToWorldPosition(previewPos) - new Vector3(0.5f, 0.5f, 0);
            
            SpriteRenderer renderer = PreviewManager.GetOrCreateSpriteRenderer("tile_preview");
            if (renderer == null)
            {
                Debug.LogError("Failed to create sprite renderer for preview");
                return;
            }
            PreviewManager.SetPreviewTransform("tile_preview", worldPosition);
            PreviewManager.SetPreviewSprite("tile_preview", _selectedTile.DefaultSprite, 
                new Color(1, 1, 1, 0.6f));
            
            Handles.color = Color.cyan;
            Handles.DrawWireCube(worldPosition, Vector3.one * 0.95f);
        }
        
        public override void Cleanup()
        {
            PreviewManager.DestroyPreview("tile_preview");
        }
    }
}