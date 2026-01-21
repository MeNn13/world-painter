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
            
            if (GUILayout.Button("+ Add New Rule", GUILayout.Height(30)))
                AddNewRule();
            
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            for (int i = 0; i < _tileRulesProp.arraySize; i++)
            {
                DrawRule(i);
                EditorGUILayout.Space(10);
            }

            EditorGUILayout.EndScrollView();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSpriteSelector(SerializedProperty spriteProp)
        {
            EditorGUILayout.BeginHorizontal();
            
            Rect rect = EditorGUILayout.GetControlRect(
                GUILayout.Width(64),
                GUILayout.Height(64));

            Sprite currentSprite = (Sprite)spriteProp.objectReferenceValue;
            
            Rect objectFieldRect = rect;
            objectFieldRect.width = 200;

            Sprite newSprite = (Sprite)EditorGUI.ObjectField(
                objectFieldRect,
                "",
                currentSprite,
                typeof(Sprite),
                false);

            if (newSprite != currentSprite)
                spriteProp.objectReferenceValue = newSprite;

            EditorGUILayout.EndHorizontal();
            
            if (currentSprite is not null)
            {
                EditorGUILayout.LabelField($"Selected: {currentSprite.name}", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField($"Size: {currentSprite.textureRect.width}x{currentSprite.textureRect.height}",
                    EditorStyles.miniLabel);
            }
        }

        private void AddNewRule()
        {
            int newIndex = _tileRulesProp.arraySize;
            _tileRulesProp.arraySize++;

            var newRuleProp = _tileRulesProp.GetArrayElementAtIndex(newIndex);

            var neighborMaskProp = newRuleProp.FindPropertyRelative("neighborMask");
            neighborMaskProp.arraySize = 8;
            for (int i = 0; i < 8; i++)
            {
                neighborMaskProp.GetArrayElementAtIndex(i).intValue = 0;
            }
        }

        private void DrawRule(int ruleIndex)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Rule {ruleIndex + 1}", EditorStyles.boldLabel);

            if (GUILayout.Button("×", GUILayout.Width(25)))
            {
                _tileRulesProp.DeleteArrayElementAtIndex(ruleIndex);
                return;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;

            var ruleProp = _tileRulesProp.GetArrayElementAtIndex(ruleIndex);
            var spriteProp = ruleProp.FindPropertyRelative("ruleSprite");

            EditorGUILayout.LabelField("Sprite for this rule:", EditorStyles.miniBoldLabel);
            DrawSpriteSelector(spriteProp);

            DrawRuleGrid(ruleProp);

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void DrawRuleGrid(SerializedProperty ruleProp)
        {
            var neighborMaskProp = ruleProp.FindPropertyRelative("neighborMask");

            EditorGUILayout.Space(5);

            // ПРАВИЛЬНЫЕ индексы для сетки 3x3 (чтобы стрелки отображались правильно):
            // ↖ ↑ ↗  = [7] [0] [1]
            // ← X →  = [6] [-1] [2]
            // ↙ ↓ ↘  = [5] [4] [3]
            int[] gridIndices = { 7, 0, 1, 6, -1, 2, 5, 4, 3 };

            // Стрелки для каждого направления (индекса 0-7):
            // 0 = Up, 1 = UpRight, 2 = Right, 3 = DownRight, 
            // 4 = Down, 5 = DownLeft, 6 = Left, 7 = UpLeft
            string[] arrowSymbols = { "↑", "↗", "→", "↘", "↓", "↙", "←", "↖" };
            
            const int cellSize = 28;
            const int spacing = 1;
            
            Rect totalRect = EditorGUILayout.GetControlRect(false, cellSize * 3 + spacing * 2);
            totalRect.width = cellSize * 3 + spacing * 2;
            totalRect.x = (EditorGUIUtility.currentViewWidth - totalRect.width) / 2;
            
            for (int row = 0; row < 3; row++)
                for (int col = 0; col < 3; col++)
                {
                    int cellIndex = row * 3 + col;
                    int neighborIndex = gridIndices[cellIndex];

                    Rect cellRect = new Rect(
                        totalRect.x + col * (cellSize + spacing),
                        totalRect.y + row * (cellSize + spacing),
                        cellSize,
                        cellSize
                        );

                    // Центральная ячейка
                    if (neighborIndex == -1)
                    {
                        GUI.Box(cellRect, "X");
                        continue;
                    }

                    // Получаем значение и символ
                    var cellProp = neighborMaskProp.GetArrayElementAtIndex(neighborIndex);
                    int cellValue = cellProp.intValue;

                    // Используем ПРАВИЛЬНЫЙ символ для этого индекса
                    string symbol = cellValue switch
                    {
                        0 => // Empty/Don't care
                            "",
                        1 => // Arrow (should be same tile)
                            arrowSymbols[neighborIndex],
                        2 => // X (should NOT be same tile)
                            "×",
                        _ => ""
                    };

                    // Кнопка
                    if (GUI.Button(cellRect, symbol))
                    {
                        // Меняем значение: 0 → 1 → 2 → 0
                        cellProp.intValue = (cellValue + 1) % 3;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
        }
    }
}
