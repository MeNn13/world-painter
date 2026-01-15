using UnityEngine;
using WorldPainter.Runtime.Providers;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Core
{
    public class TileView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private TileData _data;
        private Vector2Int _gridPosition;
        private IWorldFacade _worldProvider;

        public void Initialize(TileData data, Vector2Int gridPosition, IWorldFacade worldProvider)
        {
            _data = data;
            _gridPosition = gridPosition;
            _worldProvider = worldProvider;

            spriteRenderer ??= GetComponent<SpriteRenderer>();

            if (data is not null &&  worldProvider is not null)
            {
                Sprite sprite = data.GetSpriteForNeighbors(gridPosition, worldProvider);
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = data.TintColor;
            }

            transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
        }

        public void UpdateSprite()
        {
            if (_data is not null && _worldProvider is not null)
            {
                Sprite sprite = _data.GetSpriteForNeighbors(_gridPosition, _worldProvider);
                spriteRenderer.sprite = sprite;
            }
        }

        public void Recycle()
        {
            _data = null;
            spriteRenderer.sprite = null;
            gameObject.SetActive(false);
        }
    }
}
