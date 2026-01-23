using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;
using WorldPainter.Editor.Windows;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Editor.Tools.Overlay
{
    [Overlay(typeof(SceneView), "World Painter", "World Painting Tools")]
    internal class WorldPainterOverlay : UnityEditor.Overlays.Overlay
    {
        public WorldPainterState State { get; } = new();
        
        private VisualElement _root;

        private static WorldPainterOverlay _instance;
        
        public override VisualElement CreatePanelContent()
        {
            _instance = this;
            _root = new IMGUIContainer(OnOverlayGUI);
            _root.style.minWidth = 200;
            _root.style.maxWidth = 250;
            _root.name = "World Painter Overlay";
            
            return _root;
        }
        public override void OnWillBeDestroyed()
        {
            _instance = null;
            base.OnWillBeDestroyed();
        }
        public static WorldPainterOverlay GetInstance() => 
            _instance;
        
        private void OnOverlayGUI()
        {
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Tile Palette", GUILayout.Height(25)))
            {
                var window = TilePaletteWindow.GetWindowIfOpen();
                if (window == null)
                    TilePaletteWindow.ShowWindow();
                else
                    window.Focus();
            }
            
            EditorGUILayout.Space(5);
            
            UpdateSelectedFromPalette();
            
            EditorGUILayout.Space(5);
            
            DrawModeSection();
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Cleanup Previews", GUILayout.Height(20)))
                ScenePainter.Instance?.CleanupAllPreviews();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawModeSection()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Mode:", GUILayout.Width(40));
            
            bool isPaintMode = State.PaintMode == PaintMode.Paint;
            bool isEraseMode = State.PaintMode == PaintMode.Erase;
            
            bool paintPressed = GUILayout.Toggle(isPaintMode, "Paint", "Button", GUILayout.ExpandWidth(true));
            bool erasePressed = GUILayout.Toggle(isEraseMode, "Erase", "Button", GUILayout.ExpandWidth(true));
            
            if (paintPressed && !isPaintMode)
                State.PaintMode = PaintMode.Paint;
            
            if (erasePressed && !isEraseMode)
            {
                State.PaintMode = PaintMode.Erase;
                
                if (State.ActiveTool == ToolType.None)
                    State.ActiveTool = ToolType.Tile;
            }
            
            GUILayout.EndHorizontal();
        }

        private void UpdateSelectedFromPalette()
        {
            var window = TilePaletteWindow.GetWindowIfOpen();
            if (window == null) return;
            
            var selected = window.GetSelectedTile();
            State.ClearSelectedData();
            
            if (selected is not null)
            {
                State.ActiveTool = ToolType.Tile;
                State.SelectedTile = selected;
            }
            
            if (selected is WallData wall)
            {
                State.ActiveTool = ToolType.Wall;
                State.SelectedWall = wall;
            }
            
            if (selected is MultiTileData multiTile)
            {
                State.ActiveTool = ToolType.MultiTile;
                State.SelectedMultiTile = multiTile;
            }
        }
    }
}