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
        
        public static TilePaletteWindow GetOrCreateWindow()
        {
            _instance ??= GetWindow<TilePaletteWindow>("Tile Palette");
            return _instance;
        }

        [MenuItem("Tools/WorldPainter/TilePalette")]
        public static void ShowWindow()
        {
            GetOrCreateWindow();
        }
        
        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
        
        public TileData GetSelectedTile() => _selectedTile;

        private void OnEnable()
        {
            RefreshTileList();
        }

        private void OnGUI()
        {
            GUILayout.Label("Tile Palette", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Refresh Tiles"))
            {
                RefreshTileList();
            }

            EditorGUILayout.Space();
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (TileData tile in _tileAssets)
            {
                EditorGUILayout.BeginHorizontal();
                
                if (tile.AutoTileSprites is { Length: > 0 })
                {
                    Texture2D preview = AssetPreview.GetAssetPreview(tile.AutoTileSprites[0]);
                    GUILayout.Label(preview, GUILayout.Width(50), GUILayout.Height(50));
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
                EditorGUILayout.LabelField("Selected Tile:", EditorStyles.boldLabel);
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
