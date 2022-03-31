using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Ship
    {
        // PK
        public int ShipId { get; set; }

        [MaxLength(64)]
        public string ShipName { get; set; } = default!;

        public string Coordinates { get; set; } = default!;
        public int Length { get; set; }
        public int Height { get; set; }
        [MaxLength(16)]
        public string Direction { get; set; } = default!;
        
        // FK
        public int GameBoardId { get; set; }
        public GameBoard? GameBoard { get; set; }

        public override string ToString()
        {
            return $"Id {ShipId} ShipName {ShipName} Length {Length} Height {Height} Direction {Direction} GameBoardId {GameBoard?.GameBoardId} ---";
        }
    }
}