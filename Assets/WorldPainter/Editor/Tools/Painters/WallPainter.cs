using UnityEditor;
using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Editor.Tools.Painters
{
    public class WallPainter : SingleTilePainter<WallData>
    {
        protected override string PreviewId => "wall_preview";
        protected override Color PreviewColor => new(0.3f, 0.3f, 1f);
        protected override string TileTypeName => "wall";
        
        protected override void PaintTile(Vector2Int gridPos)
        {
            WorldFacade.SetWallAt(gridPos, SelectedTile);
            Debug.Log($"Painted wall {SelectedTile.DisplayName} at {gridPos}");
        }
        protected override void EraseTile(Vector2Int gridPos)
        {
            WorldFacade.SetWallAt(gridPos, null);
            Debug.Log($"Erased wall at {gridPos}");
        }

        public override void DrawPreview()
        {
            base.DrawPreview();
            
            if (SelectedTile?.DefaultSprite is null) return;
            
            Vector3 worldPosition = GetWorldPosition();
            
            Handles.Label(worldPosition + Vector3.up * 0.5f, "Wall", 
                new GUIStyle { normal = { textColor = Color.blue } });
        }
    }
}