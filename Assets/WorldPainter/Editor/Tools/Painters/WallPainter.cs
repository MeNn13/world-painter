using System;
using UnityEditor;
using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;
using WorldPainter.Runtime.Utils;

namespace WorldPainter.Editor.Tools.Painters
{
    public class WallPainter : BasePainter
    {
        private WallData _selectedWall;

        public WallData SelectedWall
        {
            get => _selectedWall;
            set
            {
                if (_selectedWall != value)
                {
                    _selectedWall = value;
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
            
            if (WorldFacade == null)
                return;
                
            switch (Mode)
            {
                case PaintMode.Paint:
                    if (_selectedWall is not null)
                    {
                        WorldFacade.SetWallAt(gridPos, _selectedWall);
                        Debug.Log($"Painted wall {_selectedWall.DisplayName} at {gridPos}");
                        e.Use();
                    }
                    break;
                    
                case PaintMode.Erase:
                    WorldFacade.SetWallAt(gridPos, null);
                    Debug.Log($"Erased wall at {gridPos}");
                    e.Use();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public override void DrawPreview()
        {
            if (_selectedWall?.DefaultSprite is null)
            {
                Cleanup();
                return;
            }
            
            Vector3 worldPoint = GetMouseWorldPosition();
            Vector2Int previewPos = CalculateGridPosition(worldPoint);
            Vector3 worldPosition = WorldGrid.GridToWorldPosition(previewPos) - new Vector3(0.5f, 0.5f, 0);
            
            SpriteRenderer renderer = PreviewManager.GetOrCreateSpriteRenderer("wall_preview");
            if (renderer == null)
            {
                Debug.LogError("Failed to create sprite renderer for preview");
                return;
            }
            PreviewManager.SetPreviewTransform("wall_preview", worldPosition);
            PreviewManager.SetPreviewSprite("wall_preview", _selectedWall.DefaultSprite,
                new Color(0.8f, 0.8f, 1f, 0.6f)); // Голубоватый оттенок
            
            Handles.color = new Color(0.3f, 0.3f, 1f, 1f); // Синий
            Handles.DrawWireCube(worldPosition, Vector3.one * 0.95f);
            
            Handles.Label(worldPosition + Vector3.up * 0.5f, "Wall", 
                new GUIStyle { normal = { textColor = Color.blue } });
        }
        
        public override void Cleanup()
        {
            PreviewManager.DestroyPreview("wall_preview");
        }
    }
}