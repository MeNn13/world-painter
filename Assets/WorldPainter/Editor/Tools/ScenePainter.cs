using System.Linq;
using UnityEditor;
using UnityEngine;
using WorldPainter.Editor.Tools.Overlay;
using WorldPainter.Editor.Tools.Painters;
using WorldPainter.Runtime.Providers;

namespace WorldPainter.Editor.Tools
{
    [InitializeOnLoad]
    internal class ScenePainter
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

            var overlay = WorldPainterOverlay.GetInstance();

            UpdatePainters(overlay.State);
            HandleInput(overlay.State);

            Event e = Event.current;

            if (e.control && e.type is EventType.MouseDown or EventType.MouseDrag)
                HandleInput(overlay.State);

            if (e.type == EventType.Repaint)
                DrawPreviews(overlay.State);

            if (e.type == EventType.MouseMove)
                sceneView.Repaint();
        }
        
        public void CleanupAllPreviews()
                {
                    _previewManager.DestroyAllPreviews();
                    _tilePainter.Cleanup();
                    _wallPainter.Cleanup();
                    _multiTilePainter.Cleanup();
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
            return state.ActiveTool switch
            {
                ToolType.Tile => _tilePainter,
                ToolType.Wall => _wallPainter,
                ToolType.MultiTile => _multiTilePainter,
                _ => null
            };
        }
        private void SetupPainter(BasePainter painter, WorldPainterState state)
        {
            if (painter == null) return;

            if (painter is TilePainter tilePainter)
                tilePainter.SelectedTile = state.SelectedTile;
            
            if (painter is WallPainter wallPainter)
                wallPainter.SelectedTile = state.SelectedWall;
            
            if (painter is MultiTilePainter multiTilePainter)
                multiTilePainter.SelectedMultiTile = state.SelectedMultiTile;
        }

        private void HandleInput(WorldPainterState state)
        {
            var activePainter = DetermineActivePainter(state);
            activePainter?.HandleInput(state.PaintMode);
        }
        private void DrawPreviews(WorldPainterState state)
        {
            var activePainter = DetermineActivePainter(state);
            activePainter?.DrawPreview();
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
