using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class GameConfig
    {
        public int GameConfigId { get; set; }
        [MaxLength(64)] public string GameName { get; set; } = default!;
        public int BoardSizeX { get; set; }
        public int BoardSizeY { get; set; }

        public int EShipTouchRule { get; set; }

        public ICollection<GameBoard>? GameBoards { get; set; }
        public ICollection<ShipConfig>? ShipConfigs { get; set; }
        public ICollection<GameHistory>? GameHistories { get; set; }
    }
}