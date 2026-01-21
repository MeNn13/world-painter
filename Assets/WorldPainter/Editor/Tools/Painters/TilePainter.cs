using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Editor.Tools.Painters
{
    public class TilePainter : SingleTilePainter<TileData>
    {
        protected override string PreviewId => "tile_preview";
        protected override Color PreviewColor => Color.cyan;
        protected override string TileTypeName => "tile";
        
        protected override void PaintTile(Vector2Int gridPos)
        {
            WorldFacade.SetTileAt(gridPos, SelectedTile);
            Debug.Log($"Painted {SelectedTile.DisplayName} at {gridPos}");
        }
        protected override void EraseTile(Vector2Int gridPos)
        {
            WorldFacade.SetTileAt(gridPos, null);
            Debug.Log($"Erased tile at {gridPos}");
        }
    }
}