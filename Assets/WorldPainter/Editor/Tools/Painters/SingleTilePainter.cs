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

        public PaintMode Mode { get; set; } = PaintMode.Paint;

        protected abstract string PreviewId { get; }
        protected abstract Color PreviewColor { get; }
        protected abstract string TileTypeName { get; }
        protected abstract void PaintTile(Vector2Int gridPos);
        protected abstract void EraseTile(Vector2Int gridPos);

        public override void HandleInput(SceneView sceneView)
        {
            Event e = Event.current;
            if (!e.control
                || e.type is not EventType.MouseDown
                && e.type is not EventType.MouseDrag)
                return;

            Vector3 worldPoint = GetMouseWorldPosition();
            Vector2Int gridPos = CalculateGridPosition(worldPoint);

            if (WorldFacade is null)
                return;

            Paint(gridPos, e);
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
        public override void Cleanup()
        {
            PreviewManager.DestroyPreview(PreviewId);
        }
        protected Vector3 GetWorldPosition()
        {
            Vector3 worldPoint = GetMouseWorldPosition();
            Vector2Int previewPos = CalculateGridPosition(worldPoint);
            return WorldGrid.GridToWorldPosition(previewPos) - new Vector3(0.5f, 0.5f, 0);
        }

        private void Paint(Vector2Int gridPos, Event e)
        {
            if (_selectedTile is not null && Mode == PaintMode.Paint)
            {
                PaintTile(gridPos);
                e.Use();
            }

            if (Mode == PaintMode.Erase)
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
