using System;
using UnityEditor;
using UnityEngine;
using WorldPainter.Editor.Windows;
using WorldPainter.Runtime.Providers;
using WorldPainter.Runtime.ScriptableObjects;
using Object = UnityEngine.Object;

namespace WorldPainter.Editor.Tools
{
    [InitializeOnLoad]
    public static class ScenePainter
    {
        private static bool _isPainting;
        private static PaintMode _paintMode = PaintMode.Paint;
        private static TileData _selectedTile;
        
        static ScenePainter()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            DrawToolbar();

            if (!_isPainting) return;

            HandleInput(sceneView);
        }

        private static void DrawToolbar()
        {
            Handles.BeginGUI();

            GUILayout.BeginArea(new Rect(10, 10, 200, 100));
            GUILayout.BeginVertical("Box");

            GUILayout.Label("World Painter", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Tile Palette"))
                TilePaletteWindow.ShowWindow();

            // Получаем выбранный тайл из окна палитры
            var windows = Resources.FindObjectsOfTypeAll<TilePaletteWindow>();
            if (windows.Length > 0)
            {
                TilePaletteWindow paletteWindow = windows[0];
                _selectedTile = paletteWindow.GetSelectedTile();
            }

            // Отображение выбранного тайла
            GUILayout.Label(_selectedTile is not null ? $"Selected: {_selectedTile.DisplayName}" : "No tile selected");

            // Переключатель режима
            _paintMode = (PaintMode)EditorGUILayout.EnumPopup("Mode:", _paintMode);

            // Включение/выключение рисования
            _isPainting = GUILayout.Toggle(_isPainting, "Paint (Hold Ctrl)", "Button");

            GUILayout.EndVertical();
            GUILayout.EndArea();

            Handles.EndGUI();
        }

        private static void HandleInput(SceneView sceneView)
        {
            Event e = Event.current;

            // Проверяем зажат ли Ctrl
            if (!e.control) return;

            // Обрабатываем клик мыши
            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
            {
                // Получаем позицию клика в мире
                Vector2 mousePosition = e.mousePosition;
                mousePosition.y = sceneView.camera.pixelHeight - mousePosition.y;
                Ray ray = sceneView.camera.ScreenPointToRay(mousePosition);

                // Конвертируем в координаты сетки (2D, Z=0)
                Vector3 worldPos = ray.origin - ray.direction * (ray.origin.z / ray.direction.z);
                Vector2Int gridPos = new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));

                // Находим IWorldDataProvider в сцене
                IWorldDataProvider dataProvider = FindWorldDataProvider();
                if (dataProvider == null)
                {
                    Debug.LogWarning("No IWorldDataProvider found in scene!");
                    return;
                }

                // Выполняем действие в зависимости от режима
                switch (_paintMode)
                {
                    case PaintMode.Paint:
                        if (_selectedTile != null)
                        {
                            dataProvider.SetTileAt(gridPos, _selectedTile);
                            Debug.Log($"Painted {_selectedTile.DisplayName} at {gridPos}");
                        }
                        break;

                    case PaintMode.Erase:
                        dataProvider.SetTileAt(gridPos, null);
                        Debug.Log($"Erased tile at {gridPos}");
                        break;
                }

                e.Use();
            }
        }

        [Obsolete("Obsolete")]
        private static IWorldDataProvider FindWorldDataProvider()
        {
            // Ищем любой MonoBehaviour, реализующий IWorldDataProvider
            MonoBehaviour[] allObjects = Object.FindObjectsOfType<MonoBehaviour>();
            foreach (MonoBehaviour obj in allObjects)
            {
                if (obj is IWorldDataProvider provider)
                    return provider;
            }

            return null;
        }

        private enum PaintMode
        {
            Paint,
            Erase
        }
    }
}
