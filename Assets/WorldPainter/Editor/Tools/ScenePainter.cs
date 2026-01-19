using System.Linq;
using UnityEditor;
using UnityEngine;
using WorldPainter.Editor.Tools.Painters;
using WorldPainter.Editor.Tools.ToolBar;
using WorldPainter.Runtime.Providers;

namespace WorldPainter.Editor.Tools
{
    [InitializeOnLoad]
    public class ScenePainter
    {
        private static ScenePainter _instance;
        public static ScenePainter Instance => _instance ??= new ScenePainter();

        private readonly ToolbarGUI _toolbarGUI;
        private readonly PreviewManager _previewManager;
        private readonly TilePainter _tilePainter;
        private readonly WallPainter _wallPainter;
        private readonly MultiTilePainter _multiTilePainter;

        private IWorldFacade _worldFacade;

        static ScenePainter()
        {
            // Подписка на события
            SceneView.duringSceneGui += Instance.OnSceneGUI;
            EditorApplication.playModeStateChanged += Instance.OnPlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload += Instance.OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += Instance.OnAfterAssemblyReload;
        }

        private ScenePainter()
        {
            _toolbarGUI = new ToolbarGUI();
            _previewManager = new PreviewManager();

            // Создаем пейнтеры
            _tilePainter = new TilePainter();
            _wallPainter = new WallPainter();
            _multiTilePainter = new MultiTilePainter();

            // Связываем с менеджером превью
            _tilePainter.SetPreviewManager(_previewManager);
            _wallPainter.SetPreviewManager(_previewManager);
            _multiTilePainter.SetPreviewManager(_previewManager);
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            InitializeIfNeeded();

            _toolbarGUI.DrawToolbar();

            if (!_toolbarGUI.IsPainting)
            {
                CleanupAllPreviews();
                return;
            }

            UpdatePainters();

            HandleInput(sceneView);

            Event e = Event.current;

            if (e.control && e.type is EventType.MouseDown or EventType.MouseDrag)
            {
                HandleInput(sceneView);
            }

            if (e.type == EventType.Repaint)
            {
                DrawPreviews();
            }

            if (e.type == EventType.MouseMove)
            {
                sceneView.Repaint();
            }
        }

        private void InitializeIfNeeded()
        {
            if (_worldFacade is null)
            {
                IWorldFacade worldFacade = FindWorldManager();
                _worldFacade = worldFacade;
                
                _tilePainter.SetWorldProvider(_worldFacade);
                _wallPainter.SetWorldProvider(_worldFacade);
                _multiTilePainter.SetWorldProvider(_worldFacade);
            }

            if (_worldFacade is IWorldFacadeEditor { IsInitialized: false } editorProvider)
                editorProvider.InitializeForEditor();
        }

        private IWorldFacade FindWorldManager()
        {
            var worldFacades = Resources.FindObjectsOfTypeAll<WorldFacade>();
            return worldFacades.FirstOrDefault(facade => facade is not null && facade.gameObject.scene.IsValid());
        }

        private void UpdatePainters()
        {
            // Определяем активный инструмент по типу выбранного объекта
            if (_toolbarGUI.SelectedMultiTile is not null)
            {
                // MultiTile
                _multiTilePainter.SelectedMultiTile = _toolbarGUI.SelectedMultiTile;
                _multiTilePainter.Mode = _toolbarGUI.CurrentPaintMode;

                // Очищаем другие пейнтеры
                _tilePainter.Cleanup();
                _wallPainter.Cleanup();
            }
            else if (_toolbarGUI.SelectedWall is not null)
            {
                // Wall
                _wallPainter.SelectedWall = _toolbarGUI.SelectedWall;
                _wallPainter.Mode = _toolbarGUI.CurrentPaintMode;

                // Очищаем другие пейнтеры
                _tilePainter.Cleanup();
                _multiTilePainter.Cleanup();
            }
            else if (_toolbarGUI.SelectedTile is not null)
            {
                // TileView
                _tilePainter.SelectedTile = _toolbarGUI.SelectedTile;
                _tilePainter.Mode = _toolbarGUI.CurrentPaintMode;

                // Очищаем другие пейнтеры
                _wallPainter.Cleanup();
                _multiTilePainter.Cleanup();
            }
            else
            {
                // Ничего не выбрано - очищаем все
                _tilePainter.Cleanup();
                _wallPainter.Cleanup();
                _multiTilePainter.Cleanup();
            }
        }

        private void HandleInput(SceneView sceneView)
        {
            if (_toolbarGUI.SelectedMultiTile is not null)
            {
                _multiTilePainter.HandleInput(sceneView);
            }

            if (_toolbarGUI.SelectedWall is not null)
            {
                _wallPainter.HandleInput(sceneView);
            }
            if (_toolbarGUI.SelectedTile is not null)
            {
                _tilePainter.HandleInput(sceneView);
            }
        }

        private void DrawPreviews()
        {
            // Определяем какой превью рисовать по типу выбранного объекта
            if (_toolbarGUI.SelectedMultiTile != null)
            {
                _multiTilePainter.DrawPreview();
            }
            else if (_toolbarGUI.SelectedWall != null)
            {
                _wallPainter.DrawPreview();
            }
            else if (_toolbarGUI.SelectedTile != null)
            {
                _tilePainter.DrawPreview();
            }
        }

        // Методы для очистки
        public void CleanupAllPreviews()
        {
            _previewManager.DestroyAllPreviews();
            _tilePainter.Cleanup();
            _wallPainter.Cleanup();
            _multiTilePainter.Cleanup();
        }

        // Обработчики событий
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
            {
                CleanupAllPreviews();
            }
        }

        private void OnBeforeAssemblyReload()
        {
            CleanupAllPreviews();
        }

        private void OnAfterAssemblyReload()
        {
            CleanupAllPreviews();
        }

        public enum PaintMode
        {
            Paint,
            Erase
        }
    }
}
