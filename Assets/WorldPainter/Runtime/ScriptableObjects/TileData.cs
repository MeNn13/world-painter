using UnityEngine;

namespace WorldPainter.Runtime.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Tile Data", menuName = "WorldPainter/TileData", order = 0)]
    public class TileData : ScriptableObject
    {
        [SerializeField] private string displayName;
            
        public string TileId { get; }
        public string DisplayName => displayName;

        public void OnValidate()
        {
            
        }
    }
}
