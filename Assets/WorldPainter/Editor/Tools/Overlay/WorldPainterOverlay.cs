using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;
using WorldPainter.Editor.Windows;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Editor.Tools.Overlay
{
    [Overlay(typeof(SceneView), "World Painter", "World Painting Tools")]
    public class WorldPainterOverlay : UnityEditor.Overlays.Overlay
    {
        private VisualElement _root;
        private bool _isExpanded = true;
        
        public bool IsPainting => ActiveTool != ToolType.None;
        public PaintMode CurrentPaintMode { get; private set; } = PaintMode.Paint;
        public ToolType ActiveTool { get; private set; } = ToolType.None;
        
        public TileData SelectedTile { get; private set; }
        public MultiTileData SelectedMultiTile { get; private set; }
        public WallData SelectedWall { get; private set; }
        
        private static WorldPainterOverlay _instance;
        
        public override VisualElement CreatePanelContent()
        {
            _instance = this;
            _root = new IMGUIContainer(OnOverlayGUI);
            _root.style.minWidth = 220;
            _root.style.maxWidth = 300;
            _root.name = "World Painter Overlay";
            
            return _root;
        }
        
        public override void OnWillBeDestroyed()
        {
            _instance = null;
            base.OnWillBeDestroyed();
        }
        
        public static WorldPainterOverlay GetInstance()
        {
            return _instance;
        }
        
        public WorldPainterState GetState()
        {
            return new WorldPainterState
            {
                CurrentPaintMode = CurrentPaintMode,
                ActiveTool = ActiveTool,
                SelectedTile = SelectedTile,
                SelectedWall = SelectedWall,
                SelectedMultiTile = SelectedMultiTile
            };
        }
        
        private void OnOverlayGUI()
        {
            if (!_isExpanded)
            {
                DrawCollapsed();
                return;
            }
            
            DrawExpanded();
        }
        
        private void DrawCollapsed()
        {
            EditorGUILayout.BeginVertical(GUILayout.Height(40));
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("World Painter", EditorStyles.boldLabel);
            
            if (GUILayout.Button("▼", GUILayout.Width(20)))
            {
                _isExpanded = true;
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Кнопка палитры
            if (GUILayout.Button("Open Palette", GUILayout.Height(25)))
            {
                TilePaletteWindow.ShowWindow();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawExpanded()
        {
            EditorGUILayout.BeginVertical(GUILayout.Height(180));
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("World Painter", EditorStyles.boldLabel);
            
            if (GUILayout.Button("▲", GUILayout.Width(20)))
            {
                _isExpanded = false;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Tile Palette", GUILayout.Height(25)))
            {
                var window = TilePaletteWindow.GetWindowIfOpen();
                if (window == null)
                {
                    TilePaletteWindow.ShowWindow();
                }
                else
                {
                    window.Focus();
                }
            }
            
            EditorGUILayout.Space(5);
            
            // Обновление из палитры
            UpdateSelectedFromPalette();
            
            // Информация о выборе
            DrawSelectionInfo();
            
            EditorGUILayout.Space(5);
            
            // Режим рисования
            DrawModeSection();
            
            EditorGUILayout.Space(5);
            
            // Выбор инструмента
            DrawToolSection();
            
            EditorGUILayout.Space(5);
            
            // Кнопка очистки
            if (GUILayout.Button("Cleanup Previews", GUILayout.Height(20)))
            {
                ScenePainter.Instance?.CleanupAllPreviews();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawSelectionInfo()
        {
            string info = "No selection";
            Color textColor = Color.gray;
            
            if (SelectedMultiTile != null)
            {
                info = $"MultiTile: {SelectedMultiTile.DisplayName}";
                textColor = Color.green;
            }
            else if (SelectedWall != null)
            {
                info = $"Wall: {SelectedWall.DisplayName}";
                textColor = Color.blue;
            }
            else if (SelectedTile != null)
            {
                info = $"Tile: {SelectedTile.DisplayName}";
                textColor = Color.white;
            }
            
            var originalColor = GUI.color;
            GUI.color = textColor;
            
            var style = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            
            GUILayout.Label(info, style);
            GUI.color = originalColor;
        }
        
        private void DrawModeSection()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Mode:", GUILayout.Width(40));
            
            bool isPaintMode = CurrentPaintMode == PaintMode.Paint;
            bool isEraseMode = CurrentPaintMode == PaintMode.Erase;
            
            if (GUILayout.Toggle(isPaintMode, "Paint", "Button", GUILayout.ExpandWidth(true)))
            {
                CurrentPaintMode = PaintMode.Paint;
            }
            
            if (GUILayout.Toggle(isEraseMode, "Erase", "Button", GUILayout.ExpandWidth(true)))
            {
                CurrentPaintMode = PaintMode.Erase;
            }
            
            GUILayout.EndHorizontal();
        }
        
        private void DrawToolSection()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Tool:", GUILayout.Width(40));
            
            ToolType oldTool = ActiveTool;
            
            // Кнопка Tile
            bool isTileActive = GUILayout.Toggle(ActiveTool == ToolType.Tile, 
                "Tile", "Button", GUILayout.ExpandWidth(true));
            
            if (isTileActive && ActiveTool != ToolType.Tile)
            {
                ActiveTool = ToolType.Tile;
                OnToolChanged();
            }
            
            // Кнопка Wall
            bool isWallActive = GUILayout.Toggle(ActiveTool == ToolType.Wall, 
                "Wall", "Button", GUILayout.ExpandWidth(true));
            
            if (isWallActive && ActiveTool != ToolType.Wall)
            {
                ActiveTool = ToolType.Wall;
                OnToolChanged();
            }
            
            // Кнопка MultiTile
            bool isMultiTileActive = GUILayout.Toggle(ActiveTool == ToolType.MultiTile, 
                "MultiTile", "Button", GUILayout.ExpandWidth(true));
            
            if (isMultiTileActive && ActiveTool != ToolType.MultiTile)
            {
                ActiveTool = ToolType.MultiTile;
                OnToolChanged();
            }
            
            // Кнопка None
            bool isNone = GUILayout.Toggle(ActiveTool == ToolType.None, 
                "None", "Button", GUILayout.ExpandWidth(true));
            
            if (isNone && ActiveTool != ToolType.None)
            {
                ActiveTool = ToolType.None;
                OnToolChanged();
            }
            
            GUILayout.EndHorizontal();
        }
        
        private void OnToolChanged()
        {
            ScenePainter.Instance?.CleanupAllPreviews();
            UpdateSelectionBasedOnActiveTool();
        }
        
        private void UpdateSelectionBasedOnActiveTool()
        {
            var window = TilePaletteWindow.GetWindowIfOpen();
            if (window == null) return;
            
            var selected = window.GetSelectedTile();
            
            switch (ActiveTool)
            {
                case ToolType.Tile:
                    SelectedTile = selected;
                    SelectedWall = null;
                    SelectedMultiTile = null;
                    break;
                    
                case ToolType.Wall:
                    SelectedWall = selected as WallData;
                    SelectedTile = null;
                    SelectedMultiTile = null;
                    break;
                    
                case ToolType.MultiTile:
                    SelectedMultiTile = selected as MultiTileData;
                    SelectedTile = null;
                    SelectedWall = null;
                    break;
                    
                case ToolType.None:
                    SelectedTile = null;
                    SelectedWall = null;
                    SelectedMultiTile = null;
                    break;
            }
        }
        
        public void UpdateSelectedFromPalette()
        {
            var window = TilePaletteWindow.GetWindowIfOpen();
            if (window == null) return;
            
            var selected = window.GetSelectedTile();
            
            if (selected is MultiTileData multiTile)
            {
                ActiveTool = ToolType.MultiTile;
                SelectedMultiTile = multiTile;
                SelectedWall = null;
                SelectedTile = null;
            }
            else if (selected is WallData wall)
            {
                ActiveTool = ToolType.Wall;
                SelectedWall = wall;
                SelectedMultiTile = null;
                SelectedTile = null;
            }
            else if (selected is TileData tile)
            {
                ActiveTool = ToolType.Tile;
                SelectedTile = tile;
                SelectedMultiTile = null;
                SelectedWall = null;
            }
        }
    }

}