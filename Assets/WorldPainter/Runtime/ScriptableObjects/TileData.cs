using System;
using System.Collections.Generic;
using UnityEngine;
using WorldPainter.Runtime.Data;
using WorldPainter.Runtime.Providers;

namespace WorldPainter.Runtime.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Tile Data", menuName = "WorldPainter/TileData", order = 0)]
    public class TileData : ScriptableObject
    {
        [Header("Visual")] [SerializeField] private string displayName;
        [SerializeField] private Color tintColor = Color.white;

        [Header("Rule-based Tiling")] [SerializeField] private List<TileRule> tileRules = new();
        [SerializeField] private Sprite defaultSprite; // Один дефолтный спрайт

        public string TileId { get; private set; }
        public string DisplayName => displayName;
        public Color TintColor => tintColor;
        public List<TileRule> TileRules => tileRules;
        public Sprite DefaultSprite => defaultSprite;

        public Sprite GetSpriteForNeighbors(Vector2Int position, IWorldDataProvider provider) // ИЗМЕНИЛ НАЗВАНИЕ!
        {
            // Ищем первое подходящее правило
            foreach (TileRule rule in tileRules)
            {
                if (rule.CheckRule(position, this, provider))
                {
                    return rule.RuleSprite; // ДОЛЖНО БЫТЬ Sprite, а не int
                }
            }
            
            // Если нет подходящих правил - дефолтный спрайт
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
