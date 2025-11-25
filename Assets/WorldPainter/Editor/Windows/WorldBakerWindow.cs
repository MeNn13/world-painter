using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using WorldPainter.Editor.Operations;

namespace WorldPainter.Editor.Windows
{
    public class WorldBakerWindow : EditorWindow
    {
        private Tilemap _sourceTilemap;
        private Transform _chunksParent;
        private string _chunkPrefabPath = "Assets/WorldPainter/Prefabs/Chunk.prefab";

        [MenuItem("Tools/World Painter/Bake Manager")]
        private static void ShowWindow()
        {
            GetWindow<WorldBakerWindow>("World Baker");
        }

        private void OnGUI()
        {
            GUILayout.Label("World Baking Settings", EditorStyles.boldLabel);

            _sourceTilemap = (Tilemap)EditorGUILayout.ObjectField("Source Tilemap", _sourceTilemap, typeof(Tilemap), true);
            _chunksParent = (Transform)EditorGUILayout.ObjectField("Chunks Parent", _chunksParent, typeof(Transform), true);
            _chunkPrefabPath = EditorGUILayout.TextField("Chunk Prefab Path", _chunkPrefabPath);

            EditorGUILayout.Space();

            if (GUILayout.Button("Bake World", GUILayout.Height(30)))
            {
                BakeWorld();
            }

            if (GUILayout.Button("Clear Baked Data", GUILayout.Height(25)))
            {
                ClearBakedData();
            }
        }

        private void BakeWorld()
        {
            if (!ValidateInput()) return;

            var operation = new WorldBakeOperation(_sourceTilemap, _chunksParent, _chunkPrefabPath);
            operation.BakeWorld();
        }

        private void ClearBakedData()
        {
            if (!EditorUtility.DisplayDialog("Clear Baked Data",
                    "This will delete all existing chunks. Continue?", "Yes", "No")) return;

            if (_chunksParent == null) return;

            int childCount = _chunksParent.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(_chunksParent.GetChild(i).gameObject);
            }

            Debug.Log($"Cleared {childCount} chunks");
        }

        private bool ValidateInput()
        {
            if (_sourceTilemap == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Source Tilemap", "OK");
                return false;
            }

            if (_chunksParent == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Chunks Parent transform", "OK");
                return false;
            }

            return true;
        }
    }
}