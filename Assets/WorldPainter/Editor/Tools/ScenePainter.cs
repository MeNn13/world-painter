using System;
using UnityEditor;
using UnityEngine;
using WorldPainter.Editor.Windows;
using WorldPainter.Runtime.Providers;
using WorldPainter.Runtime.ScriptableObjects;
using WorldPainter.Runtime.Utils;
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

        // Добавьте новые поля для превью обычных тайлов
        private static GameObject _simpleTilePreviewGameObject;
        private static SpriteRenderer _simpleTilePreviewSpriteRenderer;

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
            DestroySimpleTilePreview(); // НОВОЕ

            // Ищем и уничтожаем ВСЕ объекты превью в сцене
            var allPreviewObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var obj in allPreviewObjects)
            {
                if (obj != null && (obj.name.Contains("Multitile Preview") || obj.name.Contains("SimpleTile Preview")))
                {
                    Object.DestroyImmediate(obj);
                }
            }

            // Сбрасываем статические поля
            _previewGameObject = null;
            _previewSpriteRenderer = null;
            _simpleTilePreviewGameObject = null; // НОВОЕ
            _simpleTilePreviewSpriteRenderer = null; // НОВОЕ
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
                DestroySimpleTilePreview(); // НОВОЕ
                return;
            }

            HandleInput(sceneView);

            // Рисуем превью
            if (Event.current.type != EventType.Repaint)
                return;

            if (_selectedMultitile is not null)
                DrawMultiTilePreview();
            else
                DrawSingleTilePreview();
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
            if (e.type == EventType.MouseMove)
            {
                if (_selectedMultitile != null)
                    UpdatePreviewPosition();

                sceneView.Repaint();
            }

            // Проверяем зажат ли Ctrl
            if (!e.control) return;

            // Обрабатываем клик мыши
            if (e.type is EventType.MouseDown or EventType.MouseDrag)
            {
                Vector3 worldPoint = GetMouseWorldPosition();

                // Используем универсальный метод для расчета позиции
                Vector2Int gridPos = CalculateGridPosition(
                    worldPoint,
                    _selectedMultitile is not null );

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
                            bool canPlace = worldData.CanPlaceMultiTile(_selectedMultitile, gridPos);
                            
                            if (canPlace && worldData.PlaceMultiTile(_selectedMultitile, gridPos))
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
                            if (worldData.RemoveMultiTileAt(gridPos))
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
            Vector3 worldPoint = GetMouseWorldPosition();

            // Используем универсальный метод
            Vector2Int newPos = WorldGrid.WorldToGridPosition(worldPoint, false);
                
            if (_lastPreviewPosition != newPos)
            {
                _lastPreviewPosition = newPos;

                if (_cachedWorldData != null)
                {
                    _lastPlacementValid = _cachedWorldData.CanPlaceMultiTile(_selectedMultitile, newPos);
                }
            }
        }

        private static void DrawMultiTilePreview()
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
            
            Vector3 bottomLeftPosition = WorldGrid.GridToWorldPosition(_lastPreviewPosition, false) - new Vector3(0.5f, 0.5f, 0);

            // Позиция спрайта с учётом pivotOffset (как в Tile.cs)
            Vector3 spritePosition = bottomLeftPosition
                                     + new Vector3(
                                         _selectedMultitile.pivotOffset.x,
                                         _selectedMultitile.pivotOffset.y,
                                         0
                                         );

            _previewGameObject.transform.position = spritePosition;
            _previewSpriteRenderer.sprite = _selectedMultitile.DefaultSprite;
            _previewSpriteRenderer.color = _lastPlacementValid
                ? new Color(1, 1, 1, 0.6f)
                : new Color(1, 0.5f, 0.5f, 0.6f);

            //ИСПРАВЛЕНИЕ: Теперь контур рисуется ТАКЖЕ относительно pivotOffset
            // Контур области тайлов (левого нижнего угла)
            Vector3 tileAreaCenter = bottomLeftPosition
                                     + new Vector3(
                                         _selectedMultitile.pivotOffset.x,
                                         _selectedMultitile.pivotOffset.y,
                                         0
                                         );

            Handles.color = _lastPlacementValid ? Color.green : Color.red;
            Handles.DrawWireCube(tileAreaCenter, new Vector3(_selectedMultitile.size.x, _selectedMultitile.size.y, 0));

            // ✅ Дополнительно: Показываем точку привязки (pivot point)
            Handles.color = Color.cyan;
            Handles.DrawSolidDisc(spritePosition, Vector3.forward, 0.1f);

            // ✅ И точку левого нижнего угла
            Handles.color = Color.yellow;
            Handles.DrawWireCube(bottomLeftPosition, new Vector3(0.1f, 0.1f, 0));


            // ✅ Для отладки: Показываем сетку занимаемых тайлов
            if (_selectedMultitile.size.x > 1 || _selectedMultitile.size.y > 1)
            {
                Handles.color = _lastPlacementValid ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);
                for (int x = 0; x < _selectedMultitile.size.x; x++)
                {
                    for (int y = 0; y < _selectedMultitile.size.y; y++)
                    {
                        Vector3 cellCenter = bottomLeftPosition + new Vector3(x + 0.5f, y + 0.5f, 0);
                        Handles.DrawWireCube(cellCenter, new Vector3(0.95f, 0.95f, 0));
                    }
                }
            }
        }

        private static void DrawSingleTilePreview()
        {
            if (_selectedTile == null || _selectedTile.DefaultSprite == null)
            {
                DestroySimpleTilePreview();
                return;
            }

            // Рассчитываем позицию для обычного тайла
            Vector2Int previewPos = CalculateGridPosition(GetMouseWorldPosition(), false);
    
            // ✅ Используем WorldGrid для преобразования в мировые координаты
            Vector3 worldPosition = WorldGrid.GridToWorldPosition(previewPos) - new Vector3(0.5f, 0.5f, 0);

            // Создаем превью если нужно
            if (_simpleTilePreviewGameObject == null)
            {
                _simpleTilePreviewGameObject = new GameObject("SimpleTile Preview");
                _simpleTilePreviewSpriteRenderer = _simpleTilePreviewGameObject.AddComponent<SpriteRenderer>();
                _simpleTilePreviewGameObject.hideFlags = HideFlags.HideAndDontSave;
            }

            _simpleTilePreviewGameObject.transform.position = worldPosition;
            _simpleTilePreviewSpriteRenderer.sprite = _selectedTile.DefaultSprite;
            _simpleTilePreviewSpriteRenderer.color = new Color(1, 1, 1, 0.6f);

            // Контур клетки
            Handles.color = Color.cyan;
            Handles.DrawWireCube(worldPosition, Vector3.one * 0.95f);
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

        // Добавьте метод для уничтожения превью обычных тайлов
        private static void DestroySimpleTilePreview()
        {
            if (_simpleTilePreviewGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_simpleTilePreviewGameObject);
                _simpleTilePreviewGameObject = null;
                _simpleTilePreviewSpriteRenderer = null;
            }
        }

        // Метод для получения позиции мыши в мировых координатах
        private static Vector3 GetMouseWorldPosition()
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);

            if (groundPlane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }

        // Универсальный метод расчета позиции в сетке
        private static Vector2Int CalculateGridPosition(Vector3 worldPoint, bool isMultiTile = false) =>
            WorldGrid.WorldToGridPosition(worldPoint, !isMultiTile);

        private enum PaintMode
        {
            Paint,
            Erase
        }
    }
}
