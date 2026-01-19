using UnityEditor;
using UnityEngine;
using WorldPainter.Runtime.Configurations;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Editor.Windows
{
    public class TilePaletteWindow : EditorWindow
    {
        private TileDataConfiguration _tileConfig;
        private TileData _selectedTile;
        private Vector2 _scrollPosition;

        private static TilePaletteWindow _instance;

        [MenuItem("Tools/WorldPainter/TilePalette")]
        public static void ShowWindow()
        {
            GetOrCreateWindow();
        }

        private static void GetOrCreateWindow()
        {
            _instance ??= GetWindow<TilePaletteWindow>("TileView Palette");
        }

        public static TilePaletteWindow GetWindowIfOpen()
        {
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
            GetTileConfig();
        }

        private void OnGUI()
        {
            GetTileConfig();

            if (_tileConfig is null)
                return;

            EditorGUILayout.Space();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            SelectTileData();

            EditorGUILayout.EndScrollView();

            if (_selectedTile is not null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Selected TileData:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Name: {_selectedTile.DisplayName}");
                EditorGUILayout.LabelField($"ID: {_selectedTile.TileId}");
            }
        }

        private void GetTileConfig()
        {
            _tileConfig = EditorGUILayout.ObjectField(
                _tileConfig is not null ? "Configuration" : "Select configuration:",
                _tileConfig, typeof(TileDataConfiguration), false) as TileDataConfiguration;
        }
        private void SelectTileData()
        {
            EditorGUILayout.BeginHorizontal();

            foreach (TileData tile in _tileConfig.Config)
                RenderButton(tile);

            EditorGUILayout.EndHorizontal();
        }

        private void RenderButton(TileData tile)
        {
            GUILayout.BeginVertical(GUILayout.Width(70));

            if (GUILayout.Button("", GUILayout.Width(64), GUILayout.Height(64)))
                _selectedTile = tile;

            Rect rect = GUILayoutUtility.GetLastRect();

            if (_selectedTile == tile)
                DrawBox(rect, Color.softGreen, 4);

            if (tile?.DefaultSprite is not null)
            {
                Rect spriteRect = new Rect(rect.x + 10, rect.y + 10, rect.width - 20, rect.height - 20);
    
                if (_selectedTile == tile)
                {
                    GUI.color = new Color(0.4f, 0.4f, 0.4f, 1f); 
                    GUI.DrawTextureWithTexCoords(spriteRect, tile.DefaultSprite.texture, GetNormalizedRect(tile.DefaultSprite));
                    GUI.color = Color.white;
                }
                else
                {
                    GUI.DrawTextureWithTexCoords(spriteRect, tile.DefaultSprite.texture, GetNormalizedRect(tile.DefaultSprite));
                }
            }

            GUILayout.Label(tile?.name ?? "Empty");

            GUILayout.EndVertical();
        }
        private Rect GetNormalizedRect(Sprite sprite)
        {
            Texture tex = sprite.texture;
            return new Rect(
                sprite.rect.x / tex.width,
                sprite.rect.y / tex.height,
                sprite.rect.width / tex.width,
                sprite.rect.height / tex.height);
        }
        private void DrawBox(Rect rect, Color color, int thickness)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), color);
        }
    }
}
