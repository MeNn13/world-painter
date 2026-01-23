using System;
using UnityEditor;
using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Editor.Windows.Search
{
    public class TileSearchResultsView
    {
        private Vector2 _scrollPosition;
        private Action<TileData> _onTileSelected;
        private GUIStyle _tileButtonStyle;
        private GUIStyle _iconStyle;

        public event Action<TileData> OnTileSelected
        {
            add => _onTileSelected += value;
            remove => _onTileSelected -= value;
        }

        public void DrawResults(TileData[] results, float maxHeight = 300f)
        {
            if (results == null || results.Length == 0)
            {
                EditorGUILayout.HelpBox("Тайлы не найдены", MessageType.Info);
                return;
            }

            _tileButtonStyle ??= new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = 40,
                padding = new RectOffset(45, 10, 0, 0)
            };

            EditorGUILayout.LabelField($"Найдено: {results.Length} тайлов", EditorStyles.boldLabel);

            _scrollPosition = EditorGUILayout.BeginScrollView(
                _scrollPosition,
                GUILayout.MaxHeight(maxHeight));

            foreach (var tile in results)
                DrawTileResult(tile);

            EditorGUILayout.EndScrollView();
        }

        private void DrawTileResult(TileData tile)
        {
            if (tile is null) return;
    
            EditorGUILayout.BeginHorizontal(GUILayout.Height(40));
    
            GUILayout.Space(15);
            
            var buttonWithIconStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(40, 10, 0, 0),
                fixedHeight = 36
            };
            
            if (GUILayout.Button(tile.DisplayName ?? "Unnamed", buttonWithIconStyle))
                _onTileSelected?.Invoke(tile);
    
            Rect buttonRect = GUILayoutUtility.GetLastRect();
            
            Rect iconRect = new Rect(
                buttonRect.x + 8,
                buttonRect.y + 6,
                24,
                24);
            
            if (tile.DefaultSprite is not null)
            {
                Rect uvRect = GetNormalizedRect(tile.DefaultSprite);
                GUI.DrawTextureWithTexCoords(iconRect, tile.DefaultSprite.texture, uvRect);
                
                EditorGUI.DrawRect(new Rect(iconRect.x - 1, iconRect.y - 1, iconRect.width + 2, iconRect.height + 2), 
                    new Color(0.4f, 0.4f, 0.4f, 0.5f));
            }
            else
            {
                EditorGUI.DrawRect(iconRect, new Color(0.3f, 0.3f, 0.3f, 1f));
                GUI.Label(iconRect, "?", EditorStyles.centeredGreyMiniLabel);
            }
    
            EditorGUILayout.EndHorizontal();
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
    }
}
