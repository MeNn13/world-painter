using UnityEditor;
using UnityEngine;
using WorldPainter.Runtime.Providers;
using WorldPainter.Runtime.Utils;

namespace WorldPainter.Editor.Tools.Painters
{
    public abstract class BasePainter
    {
        protected IWorldFacade WorldFacade;
        protected PreviewManager PreviewManager;
        
        public abstract void HandleInput(SceneView sceneView);
        public abstract void DrawPreview();
        public abstract void Cleanup();
        
        public virtual void SetWorldProvider(IWorldFacade provider) => 
            WorldFacade = provider;
        public virtual void SetPreviewManager(PreviewManager manager) => 
            PreviewManager = manager;
            
        protected Vector3 GetMouseWorldPosition()
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);
            
            return groundPlane.Raycast(ray, out float distance) 
                ? ray.GetPoint(distance) 
                : Vector3.zero;
        }
        
        protected Vector2Int CalculateGridPosition(Vector3 worldPoint, bool snapToCenter = true) => 
            WorldGrid.WorldToGridPosition(worldPoint, snapToCenter);
    }
}
