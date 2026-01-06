using UnityEngine;
using WorldPainter.Runtime.Core;
using WorldPainter.Runtime.Providers;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Samples.Scripts
{
    public class ChunkTester : MonoBehaviour
    {
        [SerializeField] private Chunk chunkPrefab;
        [SerializeField] private SimpleWorldData worldData;
        [SerializeField] private TileData testTile;
        [SerializeField] private TilePool tilePool;
        
        private void Start()
        {
            if (chunkPrefab is null || worldData is null) return;
            
            Vector2Int chunkCoord = Vector2Int.zero;
            var chunkData = worldData.GetChunkData(chunkCoord);
            
            for (int x = 0; x < 16; x++)
                for (int y = 0; y < 16; y++)
                    if ((x + y) % 2 == 0)
                    {
                        chunkData.SetTile(new Vector2Int(x, y), testTile);
                    }
            
            Chunk chunk = Instantiate(chunkPrefab);
            chunk.tilePool = tilePool;
            chunk.Initialize(chunkCoord, chunkData);
        }
    }
}
