using System;

namespace WorldPainter.Runtime.Data
{
    [Serializable]
    public class TileCell
    {
        //Id = 0 is air (empty)
        private ushort _tileId;
        private byte _health;

        public TileCell(ushort tileId, byte health)
        {
            this._tileId = tileId;
            this._health = health;
        }
        
        public ushort TileId => _tileId;
        
        public byte Health => _health;
        
        public bool IsEmpty => TileId is 0;
    }
}
