using UnityEngine;
using WorldPainter.Runtime.Chunking;
using WorldPainter.Runtime.Data;

namespace WorldPainter.Editor.Processors
{
    public class ChunkDataProcessor
    {
        public bool StoreTileInChunk(WorldChunk chunk, int localX, int localY, ushort tileId, byte health)
        {
            if (localX < 0 || localX >= WorldChunk.ChunkSize || localY < 0 || localY >= WorldChunk.ChunkSize)
            {
                Debug.LogWarning($"Trying to store tile at invalid local position: ({localX}, {localY})");
                return false;
            }

            TileCell cell = new TileCell(tileId, health);
            chunk.SetCell(localX, localY, cell);

            return true;
        }

        public void MarkAllChunksDirty(Transform chunksParent)
        {
            foreach (Transform chunkTransform in chunksParent)
            {
                WorldChunk chunk = chunkTransform.GetComponent<WorldChunk>();
                if (chunk is not null)
                    chunk.IsDirty = true;
            }

            Debug.Log($"Marked {chunksParent.childCount} chunks as dirty");
        }
    }
}
