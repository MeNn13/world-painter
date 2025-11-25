using UnityEngine;
using UnityEngine.Tilemaps;
using WorldPainter.Runtime.Data;

namespace WorldPainter.Runtime.Chunking
{
    public class WorldChunk : MonoBehaviour
    {
        public const int ChunkSize = 16;

        [SerializeField] private TileCell[,] data = new TileCell[ChunkSize, ChunkSize];

        [SerializeField] private Tilemap visualTilemap;
        [SerializeField] private Tilemap collisionTilemap;
        [SerializeField] private CompositeCollider2D chunkCollider;

        public bool IsDirty { get; set; }
        public Tilemap VisualTilemap => visualTilemap;
        public Tilemap CollisionTilemap => collisionTilemap;
        public CompositeCollider2D ChunkCollider => chunkCollider;

        public void Initialize(Tilemap visual, Tilemap collision, CompositeCollider2D collider)
        {
            visualTilemap = visual;
            collisionTilemap = collision;
            chunkCollider = collider;
        }

        public TileCell GetCell(int x, int y)
        {
            if (CheckChunkBounds(x, y))
            {
                Debug.LogWarning($"Requested cell ({x}, {y}) is out of chunk bounds!");
                return new TileCell(0, 0);
            }

            return data[x, y];
        }

        public void SetCell(int x, int y, TileCell cell)
        {
            if (CheckChunkBounds(x, y))
            {
                Debug.LogWarning($"Trying to set cell ({x}, {y}) out of chunk bounds!");
                return;
            }

            data[x, y] = cell;
            IsDirty = true;
        }

        private bool CheckChunkBounds(int x, int y)
        {
            return x is >= 0 and < ChunkSize && y is >= 0 and < ChunkSize;
        }
    }
}
