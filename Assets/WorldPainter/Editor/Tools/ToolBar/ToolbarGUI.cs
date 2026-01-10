using UnityEditor;
using UnityEngine;
using WorldPainter.Editor.Windows;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Editor.Tools.ToolBar
{
    public class ToolbarGUI
    {
        public bool IsPainting { get; private set; }
        public PaintMode CurrentPaintMode { get; private set; } = PaintMode.Paint;
        public ToolType ActiveTool { get; private set; } = ToolType.Tile;

        public TileData SelectedTile { get; private set; }
        public MultiTileData SelectedMultiTile { get; private set; }
        public WallData SelectedWall { get; private set; }
        
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
            
            // Выбор режима
            CurrentPaintMode = (PaintMode)EditorGUILayout.EnumPopup("Mode:", CurrentPaintMode);
            
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
            var window = TilePaletteWindow.GetWindowIfOpen();
            if (window == null)
            {
                SelectedTile = null;
                SelectedMultiTile = null;
                SelectedWall = null;
                return;
            }
            
            var newTile = window.GetSelectedTile();
            if (newTile != SelectedTile)
            {
                SelectedTile = newTile;
                SelectedMultiTile = newTile as MultiTileData;
                SelectedWall = newTile as WallData;
                
                // Автоматически переключаем инструмент по типу выбранного объекта
                if (SelectedMultiTile != null)
                    ActiveTool = ToolType.MultiTile;
                else if (SelectedWall != null)
                    ActiveTool = ToolType.Wall;
                else if (SelectedTile != null)
                    ActiveTool = ToolType.Tile;
                    
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
        
        private void DrawPaintButton()
        {
            bool wasPainting = IsPainting;
            IsPainting = GUILayout.Toggle(IsPainting, "Paint (Hold Ctrl)", "Button");
            
            if (wasPainting && !IsPainting)
            {
                ScenePainter.Instance?.CleanupAllPreviews();
            }
        }
    }
}