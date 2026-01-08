using UnityEditor;
using UnityEngine;
using WorldPainter.Editor.Tools.Painters;
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
        
        private IWorldDataProvider _worldProvider;
        private bool _initialized = false;
        
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
            
            // Рисуем интерфейс
            _toolbarGUI.DrawToolbar();
            
            if (!_toolbarGUI.IsPainting)
            {
                CleanupAllPreviews();
                return;
            }
            
            // Обновляем пейнтеры
            UpdatePainters();
            
            // Обрабатываем ввод
            HandleInput(sceneView);
            
            // Рисуем превью
            if (Event.current.type == EventType.Repaint)
                DrawPreviews();
        }
        
        private void InitializeIfNeeded()
        {
            if (_initialized) return;
            
            // Ищем WorldManager в сцене
            var worldManager = FindWorldManager();
            if (worldManager != null)
            {
                _worldProvider = worldManager;
                _tilePainter.SetWorldProvider(_worldProvider);
                _wallPainter.SetWorldProvider(_worldProvider);
                _multiTilePainter.SetWorldProvider(_worldProvider);
                _initialized = true;
            }
        }
        
        private IWorldDataProvider FindWorldManager()
        {
            WorldManager[] providers = UnityEngine.Object.FindObjectsOfType<WorldManager>();
            return providers.Length > 0 ? providers[0] : null;
        }
        
        private void UpdatePainters()
        {
            // Обновляем выбранные объекты в пейнтерах
            switch (_toolbarGUI.ActiveTool)
            {
                case ToolbarGUI.ToolType.Tile:
                    _tilePainter.SelectedTile = _toolbarGUI.SelectedTile;
                    _tilePainter.Mode = _toolbarGUI.CurrentPaintMode;
                    break;
                    
                case ToolbarGUI.ToolType.Wall:
                    _wallPainter.SelectedWall = _toolbarGUI.SelectedWall;
                    _wallPainter.Mode = _toolbarGUI.CurrentPaintMode;
                    break;
                    
                case ToolbarGUI.ToolType.MultiTile:
                    _multiTilePainter.SelectedMultiTile = _toolbarGUI.SelectedMultiTile;
                    _multiTilePainter.Mode = _toolbarGUI.CurrentPaintMode;
                    break;
            }
        }
        
        private void HandleInput(SceneView sceneView)
        {
            switch (_toolbarGUI.ActiveTool)
            {
                case ToolbarGUI.ToolType.Tile:
                    _tilePainter.HandleInput(sceneView);
                    break;
                    
                case ToolbarGUI.ToolType.Wall:
                    _wallPainter.HandleInput(sceneView);
                    break;
                    
                case ToolbarGUI.ToolType.MultiTile:
                    _multiTilePainter.HandleInput(sceneView);
                    break;
            }
        }
        
        private void DrawPreviews()
        {
            switch (_toolbarGUI.ActiveTool)
            {
                case ToolbarGUI.ToolType.Tile:
                    _tilePainter.DrawPreview();
                    break;
                    
                case ToolbarGUI.ToolType.Wall:
                    _wallPainter.DrawPreview();
                    break;
                    
                case ToolbarGUI.ToolType.MultiTile:
                    _multiTilePainter.DrawPreview();
                    break;
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
            if (state == PlayModeStateChange.ExitingEditMode || 
                state == PlayModeStateChange.ExitingPlayMode)
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
            _initialized = false; // Принудительная переинициализация
        }
        
        // Перечисление PaintMode должно быть доступно извне
        public enum PaintMode
        {
            Paint,
            Erase
        }
    }
}