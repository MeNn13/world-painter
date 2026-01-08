using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WorldPainter.Runtime.Data;
using WorldPainter.Runtime.Providers;

namespace WorldPainter.Runtime.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Tile Data", menuName = "WorldPainter/TileData", order = 0)]
    public class TileData : ScriptableObject
    {
        [Header("Visual")] 
        [SerializeField] private string displayName;
        [SerializeField] private Color tintColor = Color.white;

        [Header("Rule-based Tiling")]
        [SerializeField] private List<TileRule> tileRules = new();
        [SerializeField] private Sprite defaultSprite;

        public string TileId { get; private set; }
        public string DisplayName => displayName;
        public Color TintColor => tintColor;
        public Sprite DefaultSprite => defaultSprite;

        public Sprite GetSpriteForNeighbors(Vector2Int position, IWorldDataProvider provider)
        {
            foreach (TileRule rule in tileRules.Where(rule => rule.CheckRule(position, this, provider)))
                return rule.RuleSprite;

            return defaultSprite;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(TileId))
                TileId = Guid.NewGuid().ToString();
        }
#endif
    }
}
