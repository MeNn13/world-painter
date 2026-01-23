using UnityEditor;
using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Editor.Editors
{
    [CustomEditor(typeof(TileData))]
    public class TileDataEditor : UnityEditor.Editor
    {
        private SerializedProperty _displayNameProp;
        private SerializedProperty _tintColorProp;
        private SerializedProperty _defaultSpriteProp;
        private SerializedProperty _tileRulesProp;

        private Vector2 _scrollPos;

        private void OnEnable()
        {
            _displayNameProp = serializedObject.FindProperty("displayName");
            _tintColorProp = serializedObject.FindProperty("tintColor");
            _defaultSpriteProp = serializedObject.FindProperty("defaultSprite");
            _tileRulesProp = serializedObject.FindProperty("tileRules");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("TileView Data", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_displayNameProp);
            EditorGUILayout.PropertyField(_tintColorProp);

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Default Sprite", EditorStyles.boldLabel);
            DrawSpriteSelector(_defaultSpriteProp);

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("TileView Rules", EditorStyles.boldLabel);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            for (int i = 0; i < _tileRulesProp.arraySize; i++)
            {
                DrawRule(i);
                EditorGUILayout.Space(10);
            }

            if (GUILayout.Button("+ Add New Rule", GUILayout.Height(30)))
                AddNewRule();

            EditorGUILayout.EndScrollView();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSpriteSelector(SerializedProperty spriteProp)
        {
            EditorGUILayout.BeginVertical();

            Sprite currentSprite = (Sprite)spriteProp.objectReferenceValue;

            Rect spriteRect = GetSpriteRect();

            Sprite newSprite = (Sprite)EditorGUI.ObjectField(
                spriteRect,
                currentSprite,
                typeof(Sprite),
                false);

            if (newSprite != currentSprite)
                spriteProp.objectReferenceValue = newSprite;

            if (currentSprite is not null)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"Selected: {currentSprite.name}", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField($"Size: {currentSprite.textureRect.width}x{currentSprite.textureRect.height}",
                    EditorStyles.miniLabel);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawRule(int ruleIndex)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var ruleProp = _tileRulesProp.GetArrayElementAtIndex(ruleIndex);
            var spriteProp = ruleProp.FindPropertyRelative("ruleSprite");

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"Rule {ruleIndex + 1}", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(20)))
            {
                _tileRulesProp.DeleteArrayElementAtIndex(ruleIndex);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.25f));
            EditorGUILayout.LabelField("Sprite for this rule:", EditorStyles.miniBoldLabel);
            DrawSpriteSelector(spriteProp);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            DrawRuleGrid(ruleProp);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void AddNewRule()
        {
            int newIndex = _tileRulesProp.arraySize;
            _tileRulesProp.arraySize++;

            var newRuleProp = _tileRulesProp.GetArrayElementAtIndex(newIndex);

            var neighborMaskProp = newRuleProp.FindPropertyRelative("neighborMask");
            neighborMaskProp.arraySize = 8;
            for (int i = 0; i < 8; i++)
                neighborMaskProp.GetArrayElementAtIndex(i).intValue = 0;
        }

        private void DrawRuleGrid(SerializedProperty ruleProp)
        {
            var neighborMaskProp = ruleProp.FindPropertyRelative("neighborMask");

            int[] gridIndices = { 7, 0, 1, 6, -1, 2, 5, 4, 3 };
            string[] arrowSymbols = { "↑", "↗", "→", "↘", "↓", "↙", "←", "↖" };

            const float cellSize = 30f;
            const float spacing = 0.5f;

            const float gridWidth = cellSize * 3 + spacing * 2;
            const float gridHeight = cellSize * 3 + spacing * 2;

            GUILayout.Space(10);
            Rect totalRect = GUILayoutUtility.GetRect(gridWidth, gridHeight, GUILayout.ExpandWidth(false));

            totalRect.x = EditorGUIUtility.currentViewWidth * 0.25f + ((EditorGUIUtility.currentViewWidth * 0.75f - gridWidth) / 2f);

            EditorGUI.DrawRect(totalRect, new Color(0.2f, 0.2f, 0.2f, 0.3f));

            for (int row = 0; row < 3; row++)
                for (int col = 0; col < 3; col++)
                {
                    int cellIndex = row * 3 + col;
                    int neighborIndex = gridIndices[cellIndex];

                    Rect cellRect = new Rect(
                        totalRect.x + col * (cellSize + spacing),
                        totalRect.y + row * (cellSize + spacing),
                        cellSize,
                        cellSize);

                    EditorGUI.DrawRect(cellRect, new Color(0.15f, 0.15f, 0.15f, 0.8f));

                    if (neighborIndex == -1)
                        continue;

                    var cellProp = neighborMaskProp.GetArrayElementAtIndex(neighborIndex);
                    int cellValue = cellProp.intValue;

                    string symbol = cellValue switch
                    {
                        0 => "", // Empty/Don't care
                        1 => arrowSymbols[neighborIndex], // Arrow (should be same tile)
                        2 => "×", // X (should NOT be same tile)
                        _ => ""
                    };

                    GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
                    {
                        fontSize = 18
                    };

                    if (cellValue == 1) // Arrow
                    {
                        buttonStyle.normal.textColor = Color.green;
                        buttonStyle.hover.textColor = Color.green;
                    }
                    else if (cellValue == 2) // X
                    {
                        buttonStyle.normal.textColor = Color.red;
                        buttonStyle.hover.textColor = Color.red;
                    }
                    else // Empty
                    {
                        buttonStyle.normal.textColor = Color.gray;
                        buttonStyle.hover.textColor = Color.white;
                    }

                    if (GUI.Button(cellRect, symbol, buttonStyle))
                    {
                        // Меняем значение: 0 → 1 → 2 → 0
                        cellProp.intValue = (cellValue + 1) % 3;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
        }

        private Rect GetSpriteRect()
        {
            float spriteFieldSize = Mathf.Min(EditorGUIUtility.currentViewWidth * 0.15f, 100f);
            Rect spriteRect = GUILayoutUtility.GetRect(spriteFieldSize, spriteFieldSize);
            spriteRect.width = spriteFieldSize;
            spriteRect.height = spriteFieldSize;
            return spriteRect;
        }
    }
}
