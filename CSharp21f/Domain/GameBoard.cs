namespace Domain
{
    public class GameBoard
    {
        // PK
        public int GameBoardId { get; set; }
        public string BoardData { get; set; } = default!;
        
        public int Score0 { get; set; }
        
        public int Score1 { get; set; }
        
        public int PlayerCount { get; set; }

        // FK
        public int GameConfigId { get; set; }
        public GameConfig? GameConfig { get; set; }

        public ICollection<Ship>? Ships { get; set; }

        public override string ToString()
        {
            return $"Id {GameBoardId} BoardData ---";
        }
    }
}