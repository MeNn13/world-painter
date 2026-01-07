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
        private static MultiTileData _selectedMultitile;

        // Поля для превью
        private static GameObject _previewGameObject;
        private static SpriteRenderer _previewSpriteRenderer;

        // Кэшированные данные
        private static Vector2Int _lastPreviewPosition;
        private static bool _lastPlacementValid;
        private static SimpleWorldData _cachedWorldData;

        static ScenePainter()
        {
            SceneView.duringSceneGui += OnSceneGUI;

            // Подписываемся на события очистки
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

            // Очищаем при старте
            CleanupAllPreviews();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            // При изменении состояния Play Mode чистим превью
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
            {
                CleanupAllPreviews();
            }
        }

        private static void OnBeforeAssemblyReload()
        {
            CleanupAllPreviews();
        }

        private static void OnAfterAssemblyReload()
        {
            CleanupAllPreviews();
        }

        // Метод для полной очистки ВСЕХ превью в сцене
        private static void CleanupAllPreviews()
        {
            // Уничтожаем текущее превью
            DestroyPreview();

            // Ищем и уничтожаем ВСЕ объекты превью в сцене
            var allPreviewObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var obj in allPreviewObjects)
            {
                if (obj != null && obj.name.Contains("Multitile Preview"))
                {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }

            // Сбрасываем статические поля
            _previewGameObject = null;
            _previewSpriteRenderer = null;
            _selectedTile = null;
            _selectedMultitile = null;
            _isPainting = false;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            DrawToolbar();

            if (!_isPainting)
            {
                DestroyPreview();
                return;
            }

            HandleInput(sceneView);

            // Рисуем превью
            if (_selectedMultitile != null && Event.current.type == EventType.Repaint)
            {
                DrawMultitilePreview();
            }
        }

        private static void DrawToolbar()
        {
            Handles.BeginGUI();

            GUILayout.BeginArea(new Rect(10, 10, 200, 120));
            GUILayout.BeginVertical("Box");

            GUILayout.Label("World Painter", EditorStyles.boldLabel);

            if (GUILayout.Button("Tile Palette"))
                TilePaletteWindow.ShowWindow();

            // Получаем выбранный тайл из окна палитры
            var windows = Resources.FindObjectsOfTypeAll<TilePaletteWindow>();
            if (windows.Length > 0)
            {
                TilePaletteWindow paletteWindow = windows[0];
                var newTile = paletteWindow.GetSelectedTile();

                if (newTile != _selectedTile)
                {
                    _selectedTile = newTile;
                    _selectedMultitile = _selectedTile as MultiTileData;
                    DestroyPreview(); // Уничтожаем старое превью
                }
            }

            // Отображение выбранного тайла
            string tileInfo = _selectedTile is not null
                ? $"Selected: {_selectedTile.DisplayName}"
                : "No tile selected";

            if (_selectedMultitile != null)
            {
                tileInfo += $"\nSize: {_selectedMultitile.size.x}x{_selectedMultitile.size.y}";
            }

            GUILayout.Label(tileInfo, EditorStyles.wordWrappedMiniLabel);

            // Переключатель режима
            _paintMode = (PaintMode)EditorGUILayout.EnumPopup("Mode:", _paintMode);

            // Включение/выключение рисования
            bool wasPainting = _isPainting;
            _isPainting = GUILayout.Toggle(_isPainting, "Paint (Hold Ctrl)", "Button");

            // Если только что выключили рисование - чистим превью
            if (wasPainting && !_isPainting)
            {
                DestroyPreview();
            }

            // Кнопка для принудительной очистки превью (на всякий случай)
            if (GUILayout.Button("Cleanup Previews", GUILayout.Height(20)))
            {
                CleanupAllPreviews();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();

            Handles.EndGUI();
        }

        private static void HandleInput(SceneView sceneView)
        {
            Event e = Event.current;

            // Обновляем превью при движении мыши
            if (e.type == EventType.MouseMove && _selectedMultitile != null)
            {
                UpdatePreviewPosition();
                sceneView.Repaint();
            }

            // Проверяем зажат ли Ctrl
            if (!e.control) return;

            // Обрабатываем клик мыши
            if (e.type is EventType.MouseDown or EventType.MouseDrag)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);

                if (groundPlane.Raycast(ray, out float distance))
                {
                    Vector3 worldPoint = ray.GetPoint(distance);
                    Vector2Int gridPos = new Vector2Int(
                        Mathf.FloorToInt(worldPoint.x + 0.5f),
                        Mathf.FloorToInt(worldPoint.y + 0.5f)
                        );

                    IWorldDataProvider dataProvider = FindWorldDataProvider();
                    if (dataProvider == null)
                    {
                        Debug.LogWarning("No IWorldDataProvider found in scene!");
                        return;
                    }

                    if (_selectedMultitile != null)
                    {
                        if (_paintMode == PaintMode.Paint)
                        {
                            if (dataProvider is SimpleWorldData worldData)
                            {
                                bool canPlace = worldData.CanPlaceMultitile(_selectedMultitile, gridPos);

                                if (canPlace && worldData.PlaceMultitile(_selectedMultitile, gridPos))
                                {
                                    Debug.Log($"Placed {_selectedMultitile.DisplayName} at {gridPos}");
                                }
                                else if (!canPlace)
                                {
                                    Debug.LogWarning($"Cannot place {_selectedMultitile.DisplayName} at {gridPos}");
                                }
                            }
                        }
                        else if (_paintMode == PaintMode.Erase)
                        {
                            if (dataProvider is SimpleWorldData worldData)
                            {
                                if (worldData.RemoveMultitileAt(gridPos))
                                {
                                    Debug.Log($"Removed multitile at {gridPos}");
                                }
                            }
                        }
                    }
                    else
                    {
                        // Обычные тайлы
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
                    }
                    e.Use();
                }
            }
        }

        [Obsolete("Obsolete")]
        private static IWorldDataProvider FindWorldDataProvider()
        {
            if (_cachedWorldData != null)
                return _cachedWorldData;

            _cachedWorldData = FindSimpleWorldData();
            return _cachedWorldData;
        }

        [Obsolete("Obsolete")]
        private static SimpleWorldData FindSimpleWorldData()
        {
            SimpleWorldData[] providers = Object.FindObjectsOfType<SimpleWorldData>();
            return providers.Length > 0 ? providers[0] : null;
        }

        private static void UpdatePreviewPosition()
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);

            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 worldPoint = ray.GetPoint(distance);
                Vector2Int newPos = new Vector2Int(
                    Mathf.FloorToInt(worldPoint.x + 0.5f),
                    Mathf.FloorToInt(worldPoint.y + 0.5f)
                    );

                if (_lastPreviewPosition != newPos)
                {
                    _lastPreviewPosition = newPos;

                    if (_cachedWorldData != null)
                    {
                        _lastPlacementValid = _cachedWorldData.CanPlaceMultitile(_selectedMultitile, newPos);
                    }
                }
            }
        }

        private static void DrawMultitilePreview()
        {
            if (_selectedMultitile == null || _selectedMultitile.DefaultSprite == null)
            {
                DestroyPreview();
                return;
            }

            // Создаем превью если нужно
            if (_previewGameObject == null)
            {
                _previewGameObject = new GameObject("Multitile Preview");
                _previewSpriteRenderer = _previewGameObject.AddComponent<SpriteRenderer>();
                _previewGameObject.hideFlags = HideFlags.HideAndDontSave;
            }

            // Позиция ЛЕВОГО НИЖНЕГО УГЛА мультитайла
            Vector3 bottomLeftPosition = new Vector3(
                _lastPreviewPosition.x,
                _lastPreviewPosition.y,
                0
                );

            // Позиция спрайта с учётом pivotOffset (как в Tile.cs)
            Vector3 spritePosition = bottomLeftPosition + new Vector3(
                _selectedMultitile.pivotOffset.x,
                _selectedMultitile.pivotOffset.y,
                0
                );

            _previewGameObject.transform.position = spritePosition;
            _previewSpriteRenderer.sprite = _selectedMultitile.DefaultSprite;
            _previewSpriteRenderer.color = _lastPlacementValid
                ? new Color(1, 1, 1, 0.6f)
                : new Color(1, 0.5f, 0.5f, 0.6f);

            // Контур области тайлов (левого нижнего угла)
            Vector3 tileAreaCenter = bottomLeftPosition + new Vector3(
                _selectedMultitile.size.x / 2f,
                _selectedMultitile.size.y / 2f,
                0
                );
    
            Handles.color = _lastPlacementValid ? Color.green : Color.red;
            Handles.DrawWireCube(tileAreaCenter, new Vector3(_selectedMultitile.size.x, _selectedMultitile.size.y, 0));
    
            // Дополнительно показываем левый нижний угол
            Handles.color = Color.yellow;
            Handles.DrawWireCube(bottomLeftPosition, new Vector3(0.1f, 0.1f, 0));
        }

        private static void DestroyPreview()
        {
            if (_previewGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_previewGameObject);
                _previewGameObject = null;
                _previewSpriteRenderer = null;
            }
        }

        private enum PaintMode
        {
            Paint,
            Erase
        }
    }
}
