using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Editor.Tools.Overlay
{
    internal class WorldPainterState
    {
        public PaintMode PaintMode { get; set; } = PaintMode.Paint;
        public ToolType ActiveTool { get; set; } = ToolType.None;
        
        public TileData SelectedTile { get; set; }
        public MultiTileData SelectedMultiTile { get; set; }
        public WallData SelectedWall { get; set; }

        public void ClearSelectedData()
        {
            SelectedTile = null;
            SelectedMultiTile = null;
            SelectedWall = null;
        }
    }
}
