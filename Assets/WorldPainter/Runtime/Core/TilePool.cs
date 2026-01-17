using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WorldPainter.Runtime.Providers;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Core
{
    public class TilePool : MonoBehaviour
    {
        [SerializeField] private TileView tileViewPrefab;
        [SerializeField] private MultiTile multiTilePrefab;
        [SerializeField] private int initialPoolSize = 50;
        
        [SerializeField] private WorldFacade worldFacade;

        private readonly Queue<TileView> _pool = new();
        private readonly Queue<MultiTile> _multiTilePool = new();
        private Transform _poolContainer;
        private Transform _multiTilePoolContainer;

        private void Awake()
        {
            _poolContainer = new GameObject("TilePool Container").transform;
            _poolContainer.SetParent(transform);
            _poolContainer.gameObject.SetActive(false);
            
            _multiTilePoolContainer = new GameObject("MultiTilePool Container").transform;
            _multiTilePoolContainer.SetParent(transform);
            _multiTilePoolContainer.gameObject.SetActive(false);

            WarmPool();
        }

        private void WarmPool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                TileView tileView = CreateNewTile();
                tileView.transform.SetParent(_poolContainer);
                _pool.Enqueue(tileView);
                
                if (multiTilePrefab is not null)
                {
                    MultiTile multiTile = CreateNewMultiTile(false);
                    multiTile.transform.SetParent(_multiTilePoolContainer);
                    _multiTilePool.Enqueue(multiTile);
                }
            }
        }

        private TileView CreateNewTile(bool setActive = true)
        {
            TileView tileView = Instantiate(tileViewPrefab);
            tileView.gameObject.SetActive(setActive);
            return tileView;
        }

        public TileView GetTile(TileData data, Vector2Int gridPosition)
        {
            TileView tileView;
            
            while (_pool.Count > 0)
            {
                tileView = _pool.Dequeue();
                
                if (tileView is not null)
                {
                    try
                    {
                        var go = tileView.gameObject;
                        if (go != null)
                        {
                            go.SetActive(true);
                            tileView.Initialize(data, gridPosition, worldFacade);
                            return tileView;
                        }
                    }
                    catch
                    {
                        // Тайл уничтожен - продолжаем искать
                        continue;
                    }
                }
            }

            // Если в пуле нет живых тайлов - создаем новый
            tileView = CreateNewTile();
            tileView.Initialize(data, gridPosition, worldFacade);
            return tileView;
        }

        public void ReturnTile(TileView tileView)
        {
            if (tileView == null) return;

                #if UNITY_EDITOR
            // В РЕДАКТОРЕ УНИЧТОЖАЕМ ТАЙЛЫ
            if (!Application.isPlaying)
            {
                DestroyImmediate(tileView.gameObject);
                return;
            }
    #endif

            tileView.Recycle();
            tileView.transform.SetParent(_poolContainer);
            _pool.Enqueue(tileView);
        }

        public MultiTile GetMultiTile(MultiTileData data, Vector2Int rootPosition)
        {
            MultiTile multiTile;

            // Пытаемся взять из пула
            while (_multiTilePool.Count > 0)
            {
                multiTile = _multiTilePool.Dequeue();

                if (multiTile != null && multiTile.gameObject != null)
                {
                    multiTile.gameObject.SetActive(true);
                    multiTile.Initialize(data, rootPosition);
                    return multiTile;
                }
            }

            // Если в пуле нет - создаём новый
            multiTile = CreateNewMultiTile(true);
            multiTile.Initialize(data, rootPosition);
            return multiTile;
        }
        
        public void ReturnMultiTile(MultiTile multiTile)
        {
            if (multiTile == null) return;

#if UNITY_EDITOR
            // В редакторе уничтожаем
            if (!Application.isPlaying)
            {
                DestroyImmediate(multiTile.gameObject);
                return;
            }
#endif

            multiTile.Recycle();
            multiTile.transform.SetParent(_multiTilePoolContainer);
            _multiTilePool.Enqueue(multiTile);
        }
        
        private MultiTile CreateNewMultiTile(bool setActive = true)
        {
            if (multiTilePrefab == null)
            {
                Debug.LogError("MultiTile prefab is not assigned in TilePool!");
                return null;
            }

            MultiTile multiTile = Instantiate(multiTilePrefab);
            multiTile.gameObject.SetActive(setActive);
            return multiTile;
        }

    }
}
