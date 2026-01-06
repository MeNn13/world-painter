using System.Collections.Generic;
using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Core
{
    public class TilePool : MonoBehaviour
    {
        [SerializeField] private Tile tilePrefab;
        [SerializeField] private int initialPoolSize = 50;
        
        private readonly Queue<Tile> _pool = new();
        private Transform _poolContainer;
        
        private void Awake()
        {
            _poolContainer = new GameObject("TilePool Container").transform;
            _poolContainer.SetParent(transform);
            _poolContainer.gameObject.SetActive(false);
            
            WarmPool();
        }
        
        private void WarmPool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                Tile tile = CreateNewTile();
                tile.transform.SetParent(_poolContainer);
                _pool.Enqueue(tile);
            }
        }
        
        private Tile CreateNewTile()
        {
            Tile tile = Instantiate(tilePrefab);
            tile.gameObject.SetActive(false);
            return tile;
        }
        
        public Tile GetTile(TileData data, Vector2Int gridPosition)
        {
            Tile tile;
            
            if (_pool.Count > 0)
            {
                tile = _pool.Dequeue();
                tile.gameObject.SetActive(true);
            }
            else
            {
                tile = CreateNewTile();
            }
            
            tile.Initialize(data, gridPosition);
            return tile;
        }
        
        public void ReturnTile(Tile tile)
        {
            tile.Recycle();
            tile.transform.SetParent(_poolContainer);
            _pool.Enqueue(tile);
        }
    }
}
