using System.ComponentModel.DataAnnotations;

namespace Domain;

public class ShipQuantity
{
    public int ShipQuantityId { get; set; }

    [MaxLength(64)]
    public string ShipName { get; set; } = default!;
    public int PlayerId { get; set; }

    public int Quantity { get; set; }
    
    public int ShipSizeX { get; set; }
    public int ShipSizeY { get; set; }
    
    public int ShipConfigId { get; set; }
    public ShipConfig? ShipConfig { get; set; }
}