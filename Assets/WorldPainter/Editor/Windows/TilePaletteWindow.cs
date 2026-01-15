using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Editor.Windows
{
    public class TilePaletteWindow : EditorWindow
    {
        private readonly List<TileData> _tileAssets = new();
        private TileData _selectedTile;
        private Vector2 _scrollPosition;
        
        private static TilePaletteWindow _instance;


        [MenuItem("Tools/WorldPainter/TilePalette")]
        public static void ShowWindow()
        {
            GetOrCreateWindow();
        }

        private static TilePaletteWindow GetOrCreateWindow()
        {
            _instance ??= GetWindow<TilePaletteWindow>("TileView Palette");
            return _instance;
        }
        
        public static TilePaletteWindow GetWindowIfOpen()
        {
            // Ищем среди всех открытых окон
            var windows = Resources.FindObjectsOfTypeAll<TilePaletteWindow>();
            return windows.Length > 0 ? windows[0] : null;
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
        
        public TileData GetSelectedTile() => _selectedTile;

        private void OnEnable()
        {
            _instance = this;
            RefreshTileList();
        }

        private void OnGUI()
        {
            GUILayout.Label("TileView Palette", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Refresh Tiles"))
            {
                RefreshTileList();
            }

            EditorGUILayout.Space();
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (TileData tile in _tileAssets)
            {
                EditorGUILayout.BeginHorizontal();
                
                // ИСПРАВЛЯЕМ: Используем DefaultSprite вместо AutoTileSprites
                if (tile.DefaultSprite is not null)
                {
                    Texture2D preview = AssetPreview.GetAssetPreview(tile.DefaultSprite);
                    GUILayout.Label(preview, GUILayout.Width(50), GUILayout.Height(50));
                }
                else
                {
                    GUILayout.Label("No Sprite", GUILayout.Width(50), GUILayout.Height(50));
                }
                
                EditorGUILayout.BeginVertical();
                GUILayout.Label(tile.DisplayName, EditorStyles.boldLabel);
                GUILayout.Label($"ID: {tile.TileId}");
                EditorGUILayout.EndVertical();
                
                bool isSelected = _selectedTile == tile;
                if (GUILayout.Button(isSelected ? "Selected" : "Select", GUILayout.Width(80)))
                {
                    _selectedTile = tile;
                    Debug.Log($"Selected tile: {tile.DisplayName}");
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();
            
            if (_selectedTile is not null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Selected TileView:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Name: {_selectedTile.DisplayName}");
                EditorGUILayout.LabelField($"ID: {_selectedTile.TileId}");
            }
        }

        private void RefreshTileList()
        {
            _tileAssets.Clear();
            
            string[] guids = AssetDatabase.FindAssets("t:TileData");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TileData tile = AssetDatabase.LoadAssetAtPath<TileData>(path);
                if (tile is not null)
                    _tileAssets.Add(tile);
            }

            Debug.Log($"Found {_tileAssets.Count} tiles");
        }
    }
}