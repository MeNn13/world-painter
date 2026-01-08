using UnityEditor;
using UnityEngine;
using WorldPainter.Editor.Windows;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Editor.Tools
{
    public class ToolbarGUI
    {
        private readonly TilePaletteWindow _paletteWindow;
        
        private bool _isPainting = false;
        private PaintMode _paintMode = PaintMode.Paint;
        private ToolType _activeTool = ToolType.Tile;
        
        public bool IsPainting => _isPainting;
        public PaintMode CurrentPaintMode => _paintMode;
        public ToolType ActiveTool => _activeTool;
        
        public TileData SelectedTile { get; private set; }
        public MultiTileData SelectedMultiTile { get; private set; }
        public WallData SelectedWall { get; private set; }
        
        public enum ToolType { Tile, Wall, MultiTile }
        
        public ToolbarGUI()
        {
            _paletteWindow = TilePaletteWindow.GetOrCreateWindow();
        }
        
        public void DrawToolbar()
        {
            Handles.BeginGUI();
            
            GUILayout.BeginArea(new Rect(10, 10, 220, 180));
            GUILayout.BeginVertical("Box");
            
            // Заголовок
            GUILayout.Label("World Painter", EditorStyles.boldLabel);
            
            // Кнопка палитры
            if (GUILayout.Button("Tile Palette"))
                TilePaletteWindow.ShowWindow();
            
            // Получение выбранного тайла из палитры
            UpdateSelectedFromPalette();
            
            // Информация о выбранном объекте
            DrawSelectionInfo();
            
            // Выбор инструмента
            DrawToolSelection();
            
            // Выбор режима
            _paintMode = (PaintMode)EditorGUILayout.EnumPopup("Mode:", _paintMode);
            
            // Кнопка рисования
            DrawPaintButton();
            
            // Кнопка очистки
            if (GUILayout.Button("Cleanup Previews", GUILayout.Height(20)))
            {
                ScenePainter.Instance?.CleanupAllPreviews();
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
            
            Handles.EndGUI();
        }
        
        private void UpdateSelectedFromPalette()
        {
            var newTile = _paletteWindow.GetSelectedTile();
            if (newTile != SelectedTile)
            {
                SelectedTile = newTile;
                SelectedMultiTile = newTile as MultiTileData;
                SelectedWall = newTile as WallData;
                
                // Автоматически переключаем инструмент по типу выбранного объекта
                if (SelectedMultiTile != null)
                    _activeTool = ToolType.MultiTile;
                else if (SelectedWall != null)
                    _activeTool = ToolType.Wall;
                else if (SelectedTile != null)
                    _activeTool = ToolType.Tile;
                    
                ScenePainter.Instance?.CleanupAllPreviews();
            }
        }
        
        private void DrawSelectionInfo()
        {
            string info = "No selection";
            
            if (SelectedMultiTile != null)
                info = $"MultiTile: {SelectedMultiTile.DisplayName}\nSize: {SelectedMultiTile.size.x}x{SelectedMultiTile.size.y}";
            else if (SelectedWall != null)
                info = $"Wall: {SelectedWall.DisplayName}";
            else if (SelectedTile != null)
                info = $"Tile: {SelectedTile.DisplayName}";
                
            GUILayout.Label(info, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(5);
        }
        
        private void DrawToolSelection()
        {
            GUILayout.Label("Tool:", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Toggle(_activeTool == ToolType.Tile, "Tile", "Button"))
                _activeTool = ToolType.Tile;
                
            if (GUILayout.Toggle(_activeTool == ToolType.Wall, "Wall", "Button"))
                _activeTool = ToolType.Wall;
                
            if (GUILayout.Toggle(_activeTool == ToolType.MultiTile, "MultiTile", "Button"))
                _activeTool = ToolType.MultiTile;
                
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
        }
        
        private void DrawPaintButton()
        {
            bool wasPainting = _isPainting;
            _isPainting = GUILayout.Toggle(_isPainting, "Paint (Hold Ctrl)", "Button");
            
            if (wasPainting && !_isPainting)
            {
                ScenePainter.Instance?.CleanupAllPreviews();
            }
        }
    }
}