using System.Collections.Generic;

namespace BattleShipBrain
{
    public class GameBoardBrain
    {
        public BoardSquareState[,] BoardA { get; set; } = default!;

        public List<ShipBrain> Ships { get; set; } = new List<ShipBrain>();
        
    }
}