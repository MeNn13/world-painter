using UnityEngine;

namespace WorldPainter.Runtime.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Wall Data", menuName = "WorldPainter/WallData", order = 1)]
    public class WallData : TileData
    {
        [Header("Properties")] [SerializeField] private bool isSolid;
        [SerializeField] private bool blocksLight = true;
        [SerializeField] private bool canPlaceObjectsOn = true; 
        
        public bool IsSolid => isSolid;
        public bool BlocksLight => blocksLight;
        public bool CanPlaceObjectsOn => canPlaceObjectsOn;
    }
}
