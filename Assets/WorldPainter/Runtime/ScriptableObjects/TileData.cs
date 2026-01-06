using System;
using UnityEngine;

namespace WorldPainter.Runtime.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Tile Data", menuName = "WorldPainter/TileData", order = 0)]
    public class TileData : ScriptableObject
    {
        [Header("Visual")] [SerializeField] private string displayName;
        [SerializeField] private Sprite[] autoTileSprites;
        [SerializeField] private Color tintColor = Color.white;

        public string TileId { get; private set; }
        public string DisplayName => displayName;
        public Sprite[] AutoTileSprites => autoTileSprites;
        public Color TintColor => tintColor;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(TileId))
                TileId = Guid.NewGuid().ToString();
        }
#endif
    }
}
