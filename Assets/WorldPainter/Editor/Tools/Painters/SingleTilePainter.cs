using UnityEditor;
using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;
using WorldPainter.Runtime.Utils;

namespace WorldPainter.Editor.Tools.Painters
{
    public abstract class SingleTilePainter<T> : BasePainter
        where T : TileData
    {
        private T _selectedTile;
        public T SelectedTile
        {
            get => _selectedTile;
            set {
                if (_selectedTile != value)
                {
                    _selectedTile = value;
                    Cleanup();
                }
            }
        }

        protected abstract string PreviewId { get; }
        protected abstract Color PreviewColor { get; }
        protected abstract string TileTypeName { get; }
        protected abstract void PaintTile(Vector2Int gridPos);
        protected abstract void EraseTile(Vector2Int gridPos);
        protected Vector3 GetWorldPosition()
        {
            Vector2Int previewPos = CalculateGridPosition();
            return WorldGrid.GridToWorldPosition(previewPos) - new Vector3(0.5f, 0.5f, 0);
        }

        public override void HandleInput(PaintMode mode)
        {
            Event e = Event.current;
            if (!e.control
                || e.type is not EventType.MouseDown
                && e.type is not EventType.MouseDrag)
                return;

            Vector2Int gridPos = CalculateGridPosition();

            if (WorldFacade is null)
                return;

            Paint(gridPos, mode, e);
        }
        public override void DrawPreview()
        {
            if (_selectedTile?.DefaultSprite is null)
            {
                Cleanup();
                return;
            }

            Vector3 worldPosition = GetWorldPosition();

            SetupSpritePreview(worldPosition);

            Handles.color = PreviewColor;
            Handles.DrawWireCube(worldPosition, Vector3.one * 0.95f);
        }
        public void Cleanup()
        {
            PreviewManager.DestroyPreview(PreviewId);
        }

        private void Paint(Vector2Int gridPos, PaintMode mode, Event e)
        {
            if (_selectedTile is not null && mode == PaintMode.Paint)
            {
                PaintTile(gridPos);
                e.Use();
            }

            if (mode == PaintMode.Erase)
            {
                EraseTile(gridPos);
                e.Use();
            }
        }
        private void SetupSpritePreview(Vector3 worldPosition)
        {
            SpriteRenderer renderer = PreviewManager.GetOrCreateSpriteRenderer(PreviewId);
            if (renderer == null)
            {
                Debug.LogError($"Failed to create sprite renderer for {TileTypeName} preview");
                return;
            }
            
            PreviewManager.SetPreviewTransform(PreviewId, worldPosition);
            PreviewManager.SetPreviewSprite(PreviewId, _selectedTile.DefaultSprite,
                PreviewColor * new Color(1, 1, 1, 0.6f));
        }
    }
}
