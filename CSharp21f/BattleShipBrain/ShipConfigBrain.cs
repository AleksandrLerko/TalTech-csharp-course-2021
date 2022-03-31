using System;

namespace BattleShipBrain
{
    [Serializable]
    public class ShipConfigBrain
    {
        public string Name { get; set; } = default!;
        
        public int Quantity { get; set; }
        
        public int ShipSizeX { get; set; }
        public int ShipSizeY { get; set; }
    }
}