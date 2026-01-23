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
            set {
                if (_selectedMultiTile != value)
                {
                    _selectedMultiTile = value;
                    Cleanup();
                }
            }
        }

        private MultiTileData _selectedMultiTile;
        private Vector2Int _lastPreviewPosition;
        private bool _lastPlacementValid;

        public override void HandleInput(PaintMode mode)
        {
            Event e = Event.current;
            if (!e.control
                || e.type is not EventType.MouseDown
                && e.type is not EventType.MouseDrag)
                return;

            Vector2Int gridPos = CalculateGridPosition(false);

            if (WorldFacade is null)
                return;

            Paint(mode, gridPos);
            Erase(mode, gridPos);

            e.Use();
        }
        public override void DrawPreview()
        {
            if (_selectedMultiTile?.DefaultSprite is null)
            {
                Cleanup();
                return;
            }

            var spritePosition = GetSpritePosition();

            SpriteRenderer renderer = PreviewManager.GetOrCreateSpriteRenderer("multitile_preview");
            if (renderer == null)
            {
                Debug.LogError("Failed to create sprite renderer for preview");
                return;
            }

            PreviewManager.SetPreviewTransform("multitile_preview", spritePosition);
            PreviewManager.SetPreviewSprite("multitile_preview", _selectedMultiTile.DefaultSprite,
                _lastPlacementValid ? new Color(1, 1, 1, 0.6f) : new Color(1, 0.5f, 0.5f, 0.6f));

            DrawPreviewVisuals(spritePosition);
        }
        public void Cleanup()
        {
            PreviewManager.DestroyPreview("multitile_preview");
            _lastPreviewPosition = Vector2Int.zero;
            _lastPlacementValid = false;
        }

        private void Paint(PaintMode mode, Vector2Int gridPos)
        {
            if (_selectedMultiTile is not null && mode == PaintMode.Paint)
                if (WorldFacade.TrySetMultiTile(_selectedMultiTile, gridPos))
                    Debug.Log($"Placed {_selectedMultiTile.DisplayName} at {gridPos}");
        }
        private void Erase(PaintMode mode, Vector2Int gridPos)
        {
            if (mode == PaintMode.Erase)
                if (WorldFacade.RemoveMultiTileAt(gridPos))
                    Debug.Log($"Removed multi tile at {gridPos}");
        }

        private void DrawPreviewVisuals(Vector3 spritePosition)
        {
            Color wireColor = _lastPlacementValid ? Color.green : Color.red;
            Handles.color = wireColor;
            Handles.DrawWireCube(spritePosition,
                new Vector3(_selectedMultiTile.size.x, _selectedMultiTile.size.y, 0));
            
            Handles.color = Color.cyan;
            Handles.DrawSolidDisc(spritePosition, Vector3.forward, 0.05f); // Уменьшили радиус
        }
        private  Vector3 GetSpritePosition()
        {
            Vector2Int newPos = CalculateGridPosition(false);

            if (!_lastPreviewPosition.Equals(newPos))
                _lastPreviewPosition = newPos;

            Vector3 bottomLeftPosition = WorldGrid.GridToWorldPosition(newPos, false)
                                         - new Vector3(0.5f, 0.5f, 0);

            Vector3 spritePosition = bottomLeftPosition
                                     + new Vector3(
                                         _selectedMultiTile.pivotOffset.x,
                                         _selectedMultiTile.pivotOffset.y,
                                         0);

            return spritePosition;
        }
    }
}
