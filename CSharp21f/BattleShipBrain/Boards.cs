using System.Collections.Generic;

namespace BattleShipBrain
{
    public class Boards
    {
        public List<List<BoardSquareState>> BoardA { get; set; } = default!;
        public List<List<BoardSquareState>> BoardB { get; set; } = default!;
    }
}