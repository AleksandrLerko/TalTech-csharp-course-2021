using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class ShipConfig
    {
        public int ShipConfigId { get; set; }
        
        [MaxLength(64)]
        public string ShipName { get; set; } = default!;
        public int Quantity { get; set; }
        public int ShipSizeX { get; set; }
        public int ShipSizeY { get; set; }

        public int GameConfigId { get; set; }
        public GameConfig? GameConfig { get; set; }
        
        public ICollection<ShipQuantity>? ShipQuantities { get; set; }
    }
}