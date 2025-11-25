using UnityEngine;
using UnityEngine.Tilemaps;

namespace WorldPainter.Runtime.ScriptableObjects
{
    [CreateAssetMenu(fileName = "New TileData", menuName = "World Painter/Tile Data")]
    public class TileData : ScriptableObject
    {
        [Header("Visual Settings")]
        [SerializeField] private Sprite sprite;
        [SerializeField] private TileBase ruleTile;

        [Header("Gameplay Properties")]
        [SerializeField] private bool isSolid = true;
        [SerializeField] private int hardness = 100;
        [SerializeField] private bool isDynamic = true;
        
        [Header("Object Spawning")]
        [SerializeField] private GameObject prefabLink;
        [SerializeField] private Vector2Int objectSize = Vector2Int.one;
    }
}
