using UnityEngine;
using WorldPainter.Runtime.Core;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Samples.Scripts
{
    public class BlockTester : MonoBehaviour
    {
        [SerializeField] private TilePool blockPool;
        [SerializeField] private TileData testTileData;
    
        private void Start()
        {
            if (blockPool == null) return;
            
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    Tile tile = blockPool.GetTile(testTileData, new Vector2Int(x, y));
                    tile.transform.SetParent(transform);
                }
            }
        }
    }
}
