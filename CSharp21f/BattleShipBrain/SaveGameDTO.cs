using System.Collections.Generic;

namespace BattleShipBrain
{
    public class SaveGameDTO
    {
        private int _currentPlayerNo { get; set; } = 0;
        private GameBoardBrain[] GameBoards { get; set; } = new GameBoardBrain[2];
        public class GameBoardDTO
        {
            public List<List<BoardSquareState>> BoardA { get; set; } = default!;
            public List<ShipBrain> Ships { get; set; } = default!;
            
            public int Score0 { get; set; }
            public int Score1 { get; set; }
            
            public int PlayerCount { get; set; }
        }        
        
        public class OnlyGameBoardDTO
        {
            public List<List<BoardSquareState>> BoardA { get; set; } = default!;
        }        
        
        public class ShipsDTO
        {
            public List<ShipBrain> Ships { get; set; } = default!;
        }
        
        public class GameConfigDTO
        {
            public int BoardSizeX { get; set; } = default!;
            public int BoardSizeY { get; set; } = default!;
        }
        
        public class GamePlayDTO
        {
            public GameBoardDTO GameBoardDTO { get; set; } = default!;
            public GameConfigBrain GameConfigDTO { get; set; } = default!;
            public int PlayerIdDTO { get; set; } = default!;
            public bool IsStartedDTO { get; set; } = default!;

            public List<List<ShipConfigBrain>>? ShipConfigBrainDTO { get; set; }
        }
        
        public class GameHistory
        {
            public List<GamePlayDTO> GamePlayDto { get; set; } = default!;
        }
        
        public class CoordinatesDTO
        {
            public List<Coordinate> Coordinates { get; set; } = default!;
        }

    }
}