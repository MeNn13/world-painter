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
        
        public abstract void HandleInput(PaintMode mode);
        public abstract void DrawPreview();

        public void SetWorldProvider(IWorldFacade provider) => 
            WorldFacade = provider;
        public void SetPreviewManager(PreviewManager manager) => 
            PreviewManager = manager;
            
        protected Vector3 GetMouseWorldPosition()
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);
            
            return groundPlane.Raycast(ray, out float distance) 
                ? ray.GetPoint(distance) 
                : Vector3.zero;
        }
        
        protected Vector2Int CalculateGridPosition(bool snapToCenter = true)
        {
            var worldPoint = GetMouseWorldPosition();
            
            return WorldGrid.WorldToGridPosition(worldPoint, snapToCenter);
        }
    }
}
