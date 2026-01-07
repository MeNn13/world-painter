using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Core
{
    public class TilePool : MonoBehaviour
    {
        [SerializeField] private Tile tilePrefab;
        [SerializeField] private MultiTile multiTilePrefab;
        [SerializeField] private int initialPoolSize = 50;

        private Queue<Tile> _pool = new();
        private Queue<MultiTile> _multiTilePool = new(); // НОВЫЙ пул
        private Transform _poolContainer;
        private Transform _multiTilePoolContainer; // НОВЫЙ контейнер

        private void Awake()
        {
            // Контейнер для обычных тайлов
            _poolContainer = new GameObject("TilePool Container").transform;
            _poolContainer.SetParent(transform);
            _poolContainer.gameObject.SetActive(false);

            // Контейнер для мультитайлов
            _multiTilePoolContainer = new GameObject("MultiTilePool Container").transform;
            _multiTilePoolContainer.SetParent(transform);
            _multiTilePoolContainer.gameObject.SetActive(false);

            WarmPool();
        }

        private void WarmPool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                Tile tile = CreateNewTile();
                tile.transform.SetParent(_poolContainer);
                _pool.Enqueue(tile);
            
                // Можно также предзаполнить пул мультитайлов
                if (multiTilePrefab != null)
                {
                    MultiTile multiTile = CreateNewMultiTile(false);
                    multiTile.transform.SetParent(_multiTilePoolContainer);
                    _multiTilePool.Enqueue(multiTile);
                }
            }
        }

        private Tile CreateNewTile(bool setActive = true)
        {
            Tile tile = Instantiate(tilePrefab);
            tile.gameObject.SetActive(setActive);
            return tile;
        }

        public Tile GetTile(TileData data, Vector2Int gridPosition)
        {
            Tile tile;

            // ПРОБУЕМ БРАТЬ ИЗ ПУЛА, ПОКА НЕ НАЙДЕМ ЖИВОЙ ТАЙЛ
            while (_pool.Count > 0)
            {
                tile = _pool.Dequeue();

                // БЕЗОПАСНАЯ ПРОВЕРКА
                if (tile != null)
                {
                    try
                    {
                        // Если можем получить gameObject - тайл жив
                        var go = tile.gameObject;
                        if (go != null)
                        {
                            go.SetActive(true);
                            tile.Initialize(data, gridPosition);
                            return tile;
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
            tile = CreateNewTile();
            tile.Initialize(data, gridPosition);
            return tile;
        }

        public void ReturnTile(Tile tile)
        {
            if (tile == null) return;

                #if UNITY_EDITOR
            // В РЕДАКТОРЕ УНИЧТОЖАЕМ ТАЙЛЫ
            if (!Application.isPlaying)
            {
                DestroyImmediate(tile.gameObject);
                return;
            }
    #endif

            tile.Recycle();
            tile.transform.SetParent(_poolContainer);
            _pool.Enqueue(tile);
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

        /// <summary>
        /// Возвращает мультитайл в пул
        /// </summary>
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

        /// <summary>
        /// Создаёт новый мультитайл
        /// </summary>
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
