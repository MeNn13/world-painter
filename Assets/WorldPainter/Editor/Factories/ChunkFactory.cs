using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using WorldPainter.Runtime.Chunking;

namespace WorldPainter.Editor.Factories
{
    public class ChunkFactory
    {
        private readonly string _prefabPath;

        public ChunkFactory(string prefabPath)
        {
            _prefabPath = prefabPath;
        }

        public GameObject CreateChunkPrefab()
        {
            GameObject chunkPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath);
            return chunkPrefab ?? CreateNewChunkPrefab();
        }

        public GameObject CreateChunkInstance(GameObject chunkPrefab, Transform parent, string name)
        {
            GameObject chunkInstance = (GameObject)PrefabUtility.InstantiatePrefab(chunkPrefab, parent);
            chunkInstance.name = name;
            return chunkInstance;
        }

        public void InitializeChunkComponents(GameObject chunkInstance, WorldChunk worldChunk)
        {
            Tilemap visualTilemap = chunkInstance.GetComponent<Tilemap>();
            Tilemap collisionTilemap = chunkInstance.GetComponent<Tilemap>();

            TilemapRenderer renderer = chunkInstance.GetComponent<TilemapRenderer>();
            renderer.sortingOrder = 0;

            Rigidbody2D rb = chunkInstance.GetComponent<Rigidbody2D>();
            rb.simulated = true;
            rb.useAutoMass = true;

            CompositeCollider2D collider = chunkInstance.GetComponent<CompositeCollider2D>();
            collider.geometryType = CompositeCollider2D.GeometryType.Polygons;

            worldChunk.Initialize(visualTilemap, collisionTilemap, collider);
        }

        private GameObject CreateNewChunkPrefab()
        {
            GameObject newChunk = new GameObject("Chunk_Template");

            newChunk.AddComponent<WorldChunk>();
            newChunk.AddComponent<Tilemap>();
            newChunk.AddComponent<TilemapRenderer>();

            Rigidbody2D rb = newChunk.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;

            newChunk.AddComponent<CompositeCollider2D>();

            string folderPath = System.IO.Path.GetDirectoryName(_prefabPath);
            if (!System.IO.Directory.Exists(folderPath))
                System.IO.Directory.CreateDirectory(folderPath ?? string.Empty);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(newChunk, _prefabPath);
            Object.DestroyImmediate(newChunk);

            Debug.Log($"Created chunk prefab at: {_prefabPath}");
            return prefab;
        }

    }
}
