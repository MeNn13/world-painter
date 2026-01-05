using NUnit.Framework;
using UnityEngine;
using WorldPainter.Runtime.Interfaces;
using WorldPainter.Runtime.Providers;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Tests.Editor
{
    public class WorldDataProviderTests
    {
        private IWorldDataProvider _worldDataProvider;
        private TileData _tileData;
        private GameObject _obj;

        [SetUp]
        public void Setup()
        {
            _obj = new GameObject();
            _obj.AddComponent<SimpleWorldData>();
            _worldDataProvider = _obj.GetComponent<SimpleWorldData>();
            
            _tileData = ScriptableObject.CreateInstance<TileData>();
        }

        [TearDown]
        public void TearDown()
        {
            _worldDataProvider = null;
            
            Object.DestroyImmediate(_obj);
            Object.DestroyImmediate(_tileData);
        }
        
        [Test]
        public void SetAndGetTileIdTest()
        {
            Vector2Int position = Vector2Int.down;
            _worldDataProvider.SetTileIdAt(position, _tileData.TileId);

            string tileId = _worldDataProvider.GetTileIdAt(position);
            
            Assert.AreEqual(tileId, _tileData.TileId);
        }
    }
}
