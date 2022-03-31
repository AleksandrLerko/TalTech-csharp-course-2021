namespace Domain;

public class GameHistory
{
    public int GameHistoryId { get; set; }

    public string Data { get; set; } = default!;
    
    public int GameConfigId { get; set; }
    public GameConfig? GameConfig { get; set; }
}