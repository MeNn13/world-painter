using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Editor.Tools.Overlay
{
    public class WorldPainterState
    {
        public bool IsPainting => ActiveTool != ToolType.None;
        public PaintMode CurrentPaintMode { get; set; } = PaintMode.Paint;
        public ToolType ActiveTool { get; set; } = ToolType.None;
        
        public TileData SelectedTile { get; set; }
        public MultiTileData SelectedMultiTile { get; set; }
        public WallData SelectedWall { get; set; }
    }
}
