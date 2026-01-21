using System.Linq;
using UnityEditor;
using UnityEngine;
using WorldPainter.Editor.Tools.Overlay;
using WorldPainter.Editor.Tools.Painters;
using WorldPainter.Runtime.Providers;

namespace WorldPainter.Editor.Tools
{
    [InitializeOnLoad]
    public class ScenePainter
    {
        private static ScenePainter _instance;
        public static ScenePainter Instance => _instance ??= new ScenePainter();
        
        private WorldPainterOverlay _overlay;
        
        private readonly PreviewManager _previewManager;
        private readonly TilePainter _tilePainter;
        private readonly WallPainter _wallPainter;
        private readonly MultiTilePainter _multiTilePainter;

        private IWorldFacade _worldFacade;

        static ScenePainter()
        {
            SceneView.duringSceneGui += Instance.OnSceneGUI;
            EditorApplication.playModeStateChanged += Instance.OnPlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload += Instance.OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += Instance.OnAfterAssemblyReload;
        }

        private ScenePainter()
        {
            _previewManager = new PreviewManager();
            
            _tilePainter = new TilePainter();
            _wallPainter = new WallPainter();
            _multiTilePainter = new MultiTilePainter();
            
            _tilePainter.SetPreviewManager(_previewManager);
            _wallPainter.SetPreviewManager(_previewManager);
            _multiTilePainter.SetPreviewManager(_previewManager);
        }
        
        private void OnSceneGUI(SceneView sceneView)
        {
            InitializeIfNeeded();
    
            // Получаем Overlay напрямую через статический метод
            var overlay = WorldPainterOverlay.GetInstance();
    
            if (overlay == null || !overlay.IsPainting)
            {
                CleanupAllPreviews();
                return;
            }
    
            // Используем состояние из Overlay
            var state = overlay.GetState();
    
            if (state == null || !state.IsPainting)
            {
                CleanupAllPreviews();
                return;
            }

            UpdatePainters(state);
            HandleInput(state, sceneView);
    
            Event e = Event.current;

            if (e.control && e.type is EventType.MouseDown or EventType.MouseDrag)
                HandleInput(state, sceneView);

            if (e.type == EventType.Repaint)
                DrawPreviews(state);

            if (e.type == EventType.MouseMove)
                sceneView.Repaint();
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

        private void UpdatePainters(WorldPainterState state)
        {
            var activePainter = DetermineActivePainter(state);
            SetupPainter(activePainter, state);
            CleanupInactivePainters(activePainter);
        }

        private BasePainter DetermineActivePainter(WorldPainterState state)
        {
            if (state.SelectedMultiTile is not null)
                return _multiTilePainter;
            
            if (state.SelectedWall is not null)
                return _wallPainter;
            
            return state.SelectedTile is not null
                ? _tilePainter
                : null;
        }
        private void SetupPainter(BasePainter painter, WorldPainterState state)
        {
            if (painter == null) return;
    
            switch (painter)
            {
                case TilePainter tilePainter:
                    tilePainter.SelectedTile = state.SelectedTile;
                    tilePainter.Mode = state.CurrentPaintMode;
                    break;
                case WallPainter wallPainter:
                    wallPainter.SelectedTile = state.SelectedWall;
                    wallPainter.Mode = state.CurrentPaintMode;
                    break;
                case MultiTilePainter multiTilePainter:
                    multiTilePainter.SelectedMultiTile = state.SelectedMultiTile;
                    multiTilePainter.Mode = state.CurrentPaintMode;
                    break;
            }
        }

        private void HandleInput(WorldPainterState state, SceneView sceneView)
        {
            var activePainter = DetermineActivePainter(state);
            activePainter?.HandleInput(sceneView);
        }
        private void DrawPreviews(WorldPainterState state)
        {
            var activePainter = DetermineActivePainter(state);
            activePainter?.DrawPreview();
        }

        public void CleanupAllPreviews()
        {
            _previewManager.DestroyAllPreviews();
            _tilePainter.Cleanup();
            _wallPainter.Cleanup();
            _multiTilePainter.Cleanup();
        }
        private void CleanupInactivePainters(BasePainter activePainter)
                {
                    if (activePainter != _tilePainter) _tilePainter.Cleanup();
                    if (activePainter != _wallPainter) _wallPainter.Cleanup();
                    if (activePainter != _multiTilePainter) _multiTilePainter.Cleanup();
                }
        
        
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
    }
}
