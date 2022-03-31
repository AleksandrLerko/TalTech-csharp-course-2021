using System.Text.Json;
using BattleShipBrain;
using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using WebApp.Helpers;

namespace WebApp.Pages.Game;

public class Index : PageModel
{
    private readonly DAL.ApplicationDbContext _context;
    
    public Index(DAL.ApplicationDbContext context)
    {
        _context = context;
    }
    [BindProperty]
    public BsBrain GameBrain { get; set; } = default!;

    public GameConfigBrain GameConfig { get; set; } = default!;

    public int PlayerId { get; set; }

    public bool IsStartedProp { get; set; } = false;

    public bool Hidde { get; set; } = false;

    public string Message { get; set; } = default!;

    public string ActionBomb { get; set; } = "gameStart";
    public int Score0 { get; set; } = 0;
    public int Score1 { get; set; } = 0;

    public string Win { get; set; } = default!;
    public bool IsFinished { get; set; } = false;

    public string Replay { get; set; } = default!;

    public int ReplayCount { get; set; } = -1;

    //public int ShipCount { get; set; } = 0;



    public IActionResult OnGet(string gameName, int x, int y, string isVertical, bool isPlaceable,  string isStarted,
        string shipX, string shipY, string shipName, string end, string place, string saveGame, string save, string type,
        string playerId)
    {
        if ("true".Equals(end))
        {
            Console.WriteLine(HttpContext.Session.GetString("test array"));
            //InitializeGame(gameName);
            DeleteSave(gameName);
            return Redirect("/Index");
        }


        gameName = Request.Query["GameName"];
        InitializeGame(gameName?? save, isStarted);
        SetShipConfigs("true");
        if (Request.Query["Replay"].ToString() != "")
        {
            
            //var hisData = GetDataFromJson(gameName, "GameHistoryTemp");
            var data = RestoreHistoryDataJsonDTO(gameName, "GameHistoryTemp");
            
            if (Request.Query["Replay"].ToString().Equals("Continue"))
            {
                List<SaveGameDTO.GamePlayDTO> tempData = new List<SaveGameDTO.GamePlayDTO>();
                var count = -1;
                foreach (var gamePlayDto in data.GamePlayDto)
                {
                    if (count == int.Parse(Request.Query["ReplayCount"].ToString()))
                    {
                        break;
                    }
                    tempData.Add(gamePlayDto);
                    count++;
                }

                data.GamePlayDto = tempData;
            }

            if (Request.Query["ReplayCount"].ToString() != "")
            {
                ReplayCount = int.Parse(Request.Query["ReplayCount"].ToString());
            }
            if (ReplayCount == -1)
            {
                ReplayCount = data.GamePlayDto.Count - 1;
            }
            if (Request.Query["Replay"].ToString().Equals("Undo"))
            {
                Replay = "Undo";
                ReplayCount--;
            }            
            if (Request.Query["Replay"].ToString().Equals("Redo"))
            {
                Replay = "Redo";
                ReplayCount++;
            }
            if (ReplayCount >= 0)
            {
                SessionHelper.SetObjectAsJson(HttpContext.Session, "ReplayCount", ReplayCount);
                Console.WriteLine(ReplayCount);
                if (Request.Query["Replay"].ToString().Equals("Continue"))
                {
                    BindValuesToProperties(data.GamePlayDto[data.GamePlayDto.Count - 1]);
                    SaveGamePlayJson("GamePlaySave", false);
                }
                else
                {
                    BindValuesToProperties(data.GamePlayDto[ReplayCount]);
                    SaveGamePlayJson("GamePlaySave", false);
                }
                return Page();
            }
        }
        if (Request.Query["Score0"].ToString() != "" && Request.Query["Score1"].ToString() != "")
        {
            Score0 = SessionHelper.GetObjectFromJson<int>(HttpContext.Session, "Score0");
            Score1 = SessionHelper.GetObjectFromJson<int>(HttpContext.Session, "Score1");
            if (Score0 == GameConfig.BoardSizeX / 2 || Score1 == GameConfig.BoardSizeX / 2)
            {
                IsFinished = true;
                SessionHelper.SetObjectAsJson(HttpContext.Session, "IsFinished", IsFinished);
            }
            else
            {
                IsFinished = false;
            }
            
        }
        if (save == null && type == null)
        {
            var filePath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "GamePlaySave" +
                           Path.DirectorySeparatorChar + gameName + ".json";
            if (System.IO.File.Exists(filePath))
            {
                var data = RestoreDataJsonDTO(gameName, "GamePlaySave");
                BindValuesToProperties(data);
            }
        }
        
        if (save != null && type != null)
        {
            LoadGame(save, type);
        }
        
        if ("true".Equals(saveGame))
        {
            var data = RestoreDataJsonDTO(gameName, "GamePlaySave");
            BindValuesToProperties(data);
            switch (type)
            {
                case "json":
                    SaveGamePlayJson("GameSaves", true);
                    SetHistory();
                    break;
                case "db":
                    SaveGameToDb(gameName);
                    break;
            }
        }


        if (shipX != null && shipY != null || Request.Query["Action"].Equals("Set ships automatically"))
        {
            if (Request.Query["Action"].Equals("Set ships automatically"))
            {
                Console.WriteLine("auto");
                SetShipsAutomatically();
            }
            else
            {
                if (int.Parse(Request.Query["shipX"].ToString()) < 0 || int.Parse(Request.Query["shipY"].ToString()) < 0)
                {
                    Message = int.Parse(Request.Query["shipX"].ToString()) < 0
                        ? "X value should be 0 or bigger"
                        : "Y value should be 0 or bigger";
                    return Page();
                }

                if (int.Parse(Request.Query["shipX"].ToString()) > GameConfig.BoardSizeX - 1 || int.Parse(Request.Query["shipY"].ToString()) > GameConfig.BoardSizeY - 1)
                {
                    Message = int.Parse(Request.Query["shipX"].ToString()) < 0
                        ? "X value should be smaller than game board size"
                        : "Y value should be smaller than game board size";
                    return Page();
                }

                if (isPlaceable)
                {
                    Console.WriteLine("auto");
                    var xNum = int.Parse(Request.Query["shipX"]);
                    var yNum = int.Parse(Request.Query["shipY"]);
                    var currentShipType = GameBrain.GetShipConfig(int.Parse(playerId))
                        .Find(g => g.Name == Request.Query["shipName"].ToString());
                    Console.WriteLine(GameBrain.GetBoard(PlayerId, "A", null).GetLength(0));
                    Console.WriteLine(GameBrain.GetBoard(PlayerId, "A", null).GetLength(1));
                    if (yNum + currentShipType!.ShipSizeY > GameBrain.GetBoard(PlayerId, "A", null).GetLength(1) && isVertical != null)
                    {
                        return Page();
                    }

                    if (xNum + currentShipType.ShipSizeY > GameBrain.GetBoard(PlayerId, "A", null).GetLength(0))
                    {
                        return Page();
                    }
                
                    SetShips(playerId, isVertical);
                    
                }
            }
            
        }
        
        if (playerId != null)
        {
            PlayerId = int.Parse(playerId);
        }
        else
        {
            PlayerId = 0;
        }

        if ("true".Equals(isStarted) && "true".Equals(place))
        {
            var ship = GameBrain.SetBomb(x, y, PlayerId);
            Console.WriteLine(ship);
            ActionBomb = ship == "" ? "miss" : "hit";
            var i = 0;
            var currentScore = GameBrain.ShipChange(PlayerId, ship);
            switch (PlayerId)
            {
                case 0:
                    Score0 += currentScore;
                    break;
                case 1:
                    Score1 += currentScore;
                    break;
            }
            SessionHelper.SetObjectAsJson(HttpContext.Session, "Score0", Score0);
            SessionHelper.SetObjectAsJson(HttpContext.Session, "Score1", Score1);
            
        }
        

        GameBrain.SetPlayerNo(PlayerId);

        SaveGamePlayJson("GamePlaySave", true);

        SessionHelper.SetObjectAsJson(HttpContext.Session, "IsFinished", IsFinished);
        
        
        return Page();
    }

    private void SetShips(string playerId, string isVertical)
    {
        foreach (var shipConfig in GameBrain.GetShipConfig(int.Parse(playerId)))
        {
            var shipX = int.Parse(Request.Query["shipX"].ToString());
            var shipY = int.Parse(Request.Query["shipY"].ToString());
            if (Request.Query["shipName"].ToString() == shipConfig.Name)
            {
                List<Coordinate> coordinates = new();
                var lenght = shipConfig.ShipSizeX;
                var height = shipConfig.ShipSizeY;
                var direction = "";
                switch (isVertical)
                {
                    case "on":
                        for (var num = shipY; num < shipY + height; num++)
                        {
                            coordinates.Add(new Coordinate(){X = int.Parse(Request.Query["shipX"].ToString()), Y = num});
                        }

                        direction = "Vertical";
                        break;
                    case null:
                        for (var num = shipX; num < shipX + height; num++)
                        {
                            coordinates.Add(new Coordinate(){X = num, Y = int.Parse(Request.Query["shipY"].ToString())});
                        }

                        direction = "Horizontal";
                        break;
                }
                //Console.WriteLine(shipConfig.Quantity);
                if (shipConfig.Quantity > 0)
                {
                    ShipBrain ship = new(
                        Request.Query["shipName"].ToString(),
                        coordinates,
                        lenght,
                        height,
                        direction);
                    GameBrain.SetPlayerNo(PlayerId);
                    GameBrain.SetShip(ship);
                    //Console.WriteLine($"Setting - {shipConfig.Name}");
                }
                
                var exist = SessionHelper.GetObjectFromJson<int>(HttpContext.Session, "Player" + "Count") != null;
                var value = SessionHelper.GetObjectFromJson<int>(HttpContext.Session, "Player" + "Count");
                if (exist)
                {
                    Console.WriteLine(SessionHelper.GetObjectFromJson<int>(HttpContext.Session, "Player" + "Count"));
                    value += 1;
                }
                SessionHelper.SetObjectAsJson(HttpContext.Session, "Player" + "Count", value);
                
                break;
            }
        }
        var i = GameBrain.GetShipConfig(int.Parse(playerId));
        SessionHelper.SetObjectAsJson(HttpContext.Session, "Player" + Request.Query["PlayerId"].ToString(), i);
    }

    private void SetShipsAutomatically()
    {
        var rnd = new Random();
        var minimumNum = 0;
        List<string> directions = new List<string>() {"Vertical", "Horizontal"};
        do
        {
            var valid = false;
            SetShipConfigs("true");
            do
            {
                int shipX;
                int shipY;
                var randomShipType = GameBrain.GetShipConfig(PlayerId)[rnd.Next(0, GameBrain.GetShipConfig(PlayerId).Count - 1)];
                do
                {
                    shipY = GetRandomNumber();
                } while (shipY + randomShipType.ShipSizeY > GameBrain.GetBoard(PlayerId, "A", null).GetLength(1) && directions[0].Equals("Vertical"));                
                do
                {
                    shipX = GetRandomNumber();
                } while (shipX + randomShipType.ShipSizeY > GameBrain.GetBoard(PlayerId, "A", null).GetLength(0));
                var direction = "";
                List<Coordinate> coordinates = new();
                if (randomShipType.Quantity > 0)
                {
                    switch (rnd.Next(0, directions.Count))
                    {
                        case 0:
                            for (var num = shipY; num < shipY + randomShipType.ShipSizeY; num++)
                            {
                                coordinates.Add(new Coordinate(){X = shipX, Y = num});
                            }
                            direction = directions[0];
                            break;
                        case 1:
                            for (var num = shipX; num < shipX + randomShipType.ShipSizeY; num++)
                            {
                                coordinates.Add(new Coordinate() {X = num, Y = shipY});
                            }
                            direction = directions[1];
                            break;
                    }
                    ShipBrain ship = new(
                        randomShipType.Name,
                        coordinates,
                        randomShipType.ShipSizeX,
                        randomShipType.ShipSizeY,
                        direction);
                
                    GameBrain.SetPlayerNo(PlayerId);
                    valid = GameBrain.SetShip(ship);
                }
            } while (valid == false);
            var i = GameBrain.GetShipConfig(PlayerId);
            SessionHelper.SetObjectAsJson(HttpContext.Session, "Player" + Request.Query["PlayerId"].ToString(), i);
            minimumNum++;
        } while (minimumNum != 5);

        var exist = SessionHelper.GetObjectFromJson<int>(HttpContext.Session, "Player" + "Count") != null;
        Console.WriteLine(minimumNum);
        if (exist)
        {
            Console.WriteLine(SessionHelper.GetObjectFromJson<int>(HttpContext.Session, "Player" + "Count"));
            minimumNum +=
                SessionHelper.GetObjectFromJson<int>(HttpContext.Session,
                    "Player" + "Count");
        }
        SessionHelper.SetObjectAsJson(HttpContext.Session, "Player" + "Count", minimumNum);
        //GameBrain.ShipCount += minimumNum;
    }

    private int GetRandomNumber()
    {
        var rnd = new Random();
        return rnd.Next(0, GameConfig.BoardSizeX);
    }

    private IActionResult ToPage()
    {
        return Page();
    }

    private void LoadGame(string gameName, string type)
    {
        switch (type)
        {
            case "json":
                var i = RestoreDataJsonDTO(gameName, "GameHistory");
                BindValuesToProperties(i);
                break;
            case "db":
                var x = _context.GameConfigs
                    .Include(g => g.GameBoards)
                    .ThenInclude(x => x.Ships)
                    .Include(g => g.ShipConfigs)
                    .ThenInclude(g => g.ShipQuantities)
                    .Include(x => x.GameHistories);
                var gameConfig = new GameConfig();
                foreach (var gameConf in x)
                {
                    if (gameConf.GameName.Equals(gameName))
                    {
                        gameConfig = gameConf;
                    }
                }
                RestoreDataFromDb(gameConfig);
                break;
        }
    }

    private void RestoreDataFromDb(GameConfig gameConfig)
    {
        var gameBoard = gameConfig.GameBoards!.FirstOrDefault();
        var gameHistory = gameConfig.GameHistories!.FirstOrDefault();
        var shipList = gameBoard!.Ships;
        var shipConfig = gameConfig.ShipConfigs;
        var shipQuantityList = new List<ShipQuantity>();
        foreach (var config in shipConfig!)
        {
            foreach (var quantity in config.ShipQuantities!)
            {
                var shipQuant = new ShipQuantity()
                {
                    ShipName = quantity.ShipName,
                    PlayerId = quantity.PlayerId,
                    Quantity = quantity.Quantity,
                    ShipSizeX = quantity.ShipSizeX,
                    ShipSizeY = quantity.ShipSizeY
                };
                shipQuantityList.Add(shipQuant);
            }
        }
        var listOfShipConfigs = new List<ShipConfigBrain>();
        foreach (var config in shipConfig!)
        {
            var shipConfigBrain = new ShipConfigBrain()
            {
                Name = config.ShipName,
                Quantity = config.Quantity,
                ShipSizeX = config.ShipSizeX,
                ShipSizeY = config.ShipSizeY
            };
            listOfShipConfigs.Add(shipConfigBrain);
        }

        EShipTouchRule touchRule = EShipTouchRule.NoTouch;
        switch (gameConfig.EShipTouchRule)
        {
            case 0:
                touchRule = EShipTouchRule.NoTouch;
                break;
            case 1:
                touchRule = EShipTouchRule.CornerTouch;
                break;
            case 2:
                touchRule = EShipTouchRule.SideTouch;
                break;
        }
        var gameConfigBrain = new GameConfigBrain()
        {
            GameName = gameConfig.GameName,
            BoardSizeX = gameConfig.BoardSizeX,
            BoardSizeY = gameConfig.BoardSizeY,
            EShipTouchRule = touchRule,
            ShipConfigs = listOfShipConfigs
        };
        var gameBoardBrain = JsonSerializer.Deserialize<List<List<BoardSquareState>>>(gameBoard.BoardData);

        var listOfShipsBrain = new List<ShipBrain>();
        foreach (var ship in shipList!)
        {
            var coordinatesDto = JsonSerializer.Deserialize<List<Coordinate>>(ship.Coordinates);
            var shipBrain = new ShipBrain(ship.ShipName, coordinatesDto, ship.Length, ship.Height, ship.Direction);
            listOfShipsBrain.Add(shipBrain);
        }

        var gameBoardDto = new SaveGameDTO.GameBoardDTO()
        {
            BoardA = gameBoardBrain,
            Ships = listOfShipsBrain,
            Score0 = gameBoard.Score0,
            Score1 = gameBoard.Score1,
            PlayerCount = gameBoard.PlayerCount
        };

        var listOfPlayersShipConfigs = new List<List<ShipConfigBrain>>();
        var listOfPlayer0 = new List<ShipConfigBrain>();
        var listOfPlayer1 = new List<ShipConfigBrain>();
        var x = 0;
        foreach (var quantity in shipQuantityList!)
        {
            var shipQuantity = new ShipConfigBrain()
            {
                Name = quantity.ShipName,
                Quantity = quantity.Quantity,
                ShipSizeX = quantity.ShipSizeX,
                ShipSizeY = quantity.ShipSizeY
            };
            if (quantity.PlayerId == 0)
            {
                listOfPlayer0.Add(shipQuantity);
            }
            else
            {
                listOfPlayer1.Add(shipQuantity);
            }
        }
        listOfPlayersShipConfigs.Add(listOfPlayer0);
        listOfPlayersShipConfigs.Add(listOfPlayer1);

        var gamePlayDto = new SaveGameDTO.GamePlayDTO()
        {
            GameBoardDTO = gameBoardDto,
            GameConfigDTO = gameConfigBrain,
            IsStartedDTO = false,
            PlayerIdDTO = PlayerId,
            ShipConfigBrainDTO = listOfPlayersShipConfigs
        };

        var deserializedHistory = JsonSerializer.Deserialize<SaveGameDTO.GameHistory>(gameHistory!.Data);
        var gameHistoryDto = new SaveGameDTO.GameHistory()
        {
            GamePlayDto = deserializedHistory!.GamePlayDto
        };
        
        var pathHistory = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "GameHistoryTemp"
                          + Path.DirectorySeparatorChar + gameConfig.GameName + ".json";
        //Console.WriteLine(path);
        var jsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true
        };
        var serialized = JsonSerializer.Serialize(gameHistoryDto, jsonOptions);

        System.IO.File.WriteAllTextAsync(pathHistory, serialized);

        BindValuesToProperties(gamePlayDto);
        
        Console.WriteLine(shipList);
    }

    private void DeleteSave(string gameName)
    {
        var filePath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "GamePlaySave" +
                       Path.DirectorySeparatorChar + gameName + ".json";
        System.IO.File.Delete(filePath);        
        
        var filePath2 = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "ConfigSaves" +
                       Path.DirectorySeparatorChar + gameName + ".json";
        System.IO.File.Delete(filePath2);
        
        var filePath3 = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "GameHistoryTemp" +
                        Path.DirectorySeparatorChar + gameName + ".json";
        
        System.IO.File.Delete(filePath3);
        HttpContext.Session.Remove("Player0");
        HttpContext.Session.Remove("Player1");
        HttpContext.Session.Remove("PlayerCount");
        HttpContext.Session.Remove("Score0");
        HttpContext.Session.Remove("Score1");
        HttpContext.Session.Remove("ReplayCount");
    }
    
    private void SaveGameToDb(string gameName)
    {
        var jsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true
        };

        using var db = new ApplicationDbContext();

        var IsDelete = false;
        GameConfig gm = null!;
        foreach (var config in db.GameConfigs)
        {
            if (config.GameName.Equals(gameName))
            {
                IsDelete = true;
                gm = config;
            }
        }

        if (IsDelete)
        {
            if (gm != null)
            {
                db.GameConfigs.Remove(gm);
                db.SaveChanges();
            }
        }

        var board = GameBrain.GetBrainBoardAsArrays();
        var boardAsString = JsonSerializer.Serialize(board, jsonOptions);

        var touchRule = 0;
        switch (GameBrain.GetConfigBrain().EShipTouchRule)
        {
            case EShipTouchRule.NoTouch:
                touchRule = 0;
                break;
            case EShipTouchRule.CornerTouch:
                touchRule = 1;
                break;
            case EShipTouchRule.SideTouch:
                touchRule = 2;
                break;
        }
        
        var gameConfig = new GameConfig()
        {
            BoardSizeX = GameBrain.GetConfigBrain().BoardSizeX,
            BoardSizeY = GameBrain.GetConfigBrain().BoardSizeY,
            EShipTouchRule = touchRule,
            GameName = gameName
        };

        var gameBoard = new GameBoard()
        {
            BoardData = boardAsString,
            GameConfig = gameConfig,
            Score0 = Score0,
            Score1 = Score1,
            PlayerCount = SessionHelper.GetObjectFromJson<int>(HttpContext.Session, "Player" + "Count")
        };
        

        Console.WriteLine(GameBrain.GetListOfShips().Count);
        foreach (var shipBrain in GameBrain.GetListOfShips())
        {
            Console.WriteLine("shipBrain.Name");
            //var coordDto = new SaveGameDTO.CoordinatesDTO();
            var coordinates = new List<Coordinate>();
            foreach (var coordinate in shipBrain.Coordinates)
            {
                coordinates.Add(coordinate);
            }

            //coordDto.Coordinates = coordinates;

            var coordinatesAsString = JsonSerializer.Serialize(coordinates, jsonOptions);

            var ship = new Ship()
            {
                ShipName = shipBrain.Name,
                Coordinates = coordinatesAsString,
                Direction = shipBrain.Direction,
                Height = shipBrain.Height,
                Length = shipBrain.Length,
                GameBoard = gameBoard
            };
            
            db.Ship.Add(ship);
        }
        
        foreach (var shipConfig in GameBrain.GetConfigBrain().ShipConfigs)
        {
            var shipConf = new ShipConfig()
            {
                ShipName = shipConfig.Name,
                Quantity = shipConfig.Quantity,
                ShipSizeX = shipConfig.ShipSizeX,
                ShipSizeY = shipConfig.ShipSizeY,
                GameConfig = gameConfig
            };
            
            var i = 0;
            foreach (var allShipConfig in GameBrain.GetAllShipConfigs())
            {
                foreach (var configBrain in allShipConfig)
                {
                    if (configBrain.Name.Equals(shipConf.ShipName))
                    {
                        var shipQuantity = new ShipQuantity()
                        {
                            ShipName = configBrain.Name,
                            PlayerId = i == 0 ? 0 : 1,
                            Quantity = configBrain.Quantity,
                            ShipSizeX = configBrain.ShipSizeX,
                            ShipSizeY = configBrain.ShipSizeY,
                            ShipConfig = shipConf
                        };
                        
                        db.ShipQuantities.Add(shipQuantity);
                        break;
                    }
                }

                i++;
            }

            db.ShipConfigs.Add(shipConf);

        }

        var gameHistoryData = RestoreHistoryDataJsonDTO(GameConfig.GameName, "GameHistoryTemp");
        var serializedGameHistory = JsonSerializer.Serialize(gameHistoryData, jsonOptions);
        var gameHistory = new GameHistory()
        {
            Data = serializedGameHistory,
            GameConfig = gameConfig
        };
        db.GameHistories.Add(gameHistory);
        
        db.GameConfigs.Add(gameConfig);
        db.GameBoards.Add(gameBoard);


        db.SaveChanges();

    }
    
    private void SaveGamePlayJson(string filePath, bool notSave)
    {
        var jsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true
        };

        var gBoard = new SaveGameDTO.GameBoardDTO()
        {
            BoardA = GameBrain.GetBrainBoardAsArrays(),
            Ships = GameBrain.GetListOfShips(),
            Score0 = Score0,
            Score1 = Score1,
            PlayerCount = SessionHelper.GetObjectFromJson<int>(HttpContext.Session, "Player" + "Count")
        };
        
        var gamePlay = new SaveGameDTO.GamePlayDTO()
        {
            GameBoardDTO = gBoard,
            GameConfigDTO = GameConfig,
            IsStartedDTO = IsStartedProp,
            PlayerIdDTO = PlayerId,
            ShipConfigBrainDTO = GameBrain.GetAllShipConfigs()
        };

        var lst = new List<SaveGameDTO.GamePlayDTO>();
        lst.Add(gamePlay);
        var gameHistory = new SaveGameDTO.GameHistory()
        {
            GamePlayDto = lst
        };
        var gameConfigBrainJsonData = JsonSerializer.Serialize(gamePlay, jsonOptions);
        var gameConfigBrainJsonData2 = JsonSerializer.Serialize(gameHistory, jsonOptions);
        
        var pathGamePlay = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + $"{filePath}"
                   + Path.DirectorySeparatorChar + GameConfig.GameName + ".json";        
        
        var pathHistoryTemp = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "GameHistoryTemp"
                   + Path.DirectorySeparatorChar + GameConfig.GameName + ".json";    
        var pathHistory = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "GameHistoryTemp"
                   + Path.DirectorySeparatorChar + GameConfig.GameName + "History" + ".json";        

        //Console.WriteLine(path);
        System.IO.File.WriteAllTextAsync(pathGamePlay, gameConfigBrainJsonData);
        if (notSave)
        {
            if (System.IO.File.Exists(pathHistoryTemp) && System.IO.File.Exists(pathHistory))
            {
                var data = RestoreHistoryDataJsonDTO(GameConfig.GameName, "GameHistoryTemp");
                data.GamePlayDto.Add(gamePlay);
                gameConfigBrainJsonData2 = JsonSerializer.Serialize(data, jsonOptions);
                ReplayCount = data.GamePlayDto.Count - 1;
                SessionHelper.SetObjectAsJson(HttpContext.Session, "ReplayCount", ReplayCount);
                Console.WriteLine("ReplayCount value is " + ReplayCount);
                System.IO.File.WriteAllTextAsync(pathHistoryTemp, gameConfigBrainJsonData2);
                System.IO.File.WriteAllTextAsync(pathHistory, gameConfigBrainJsonData2);
            }
            else
            {
                System.IO.File.WriteAllTextAsync(pathHistoryTemp, gameConfigBrainJsonData2);
                System.IO.File.WriteAllTextAsync(pathHistory, gameConfigBrainJsonData2);
            }
        }

        if (!notSave && Request.Query["Replay"].ToString().Equals("Continue"))
        {
            System.IO.File.Delete(pathHistoryTemp);
            System.IO.File.WriteAllTextAsync(pathHistoryTemp, gameConfigBrainJsonData2);
        }
        

    }

    private void SetHistory()
    {
        var jsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true
        };
        var pathHistory = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "GameHistory"
                      + Path.DirectorySeparatorChar + GameConfig.GameName + "History" + ".json";
        Console.WriteLine(pathHistory);
        //var dezerialized = GetDataFromJson(GameConfig.GameName, "GameHistoryTemp");
        var gameHistoryData = RestoreHistoryDataJsonDTO(GameConfig.GameName + "History", "GameHistoryTemp");
        var serializedGameHistory = JsonSerializer.Serialize(gameHistoryData, jsonOptions);
        System.IO.File.WriteAllTextAsync(pathHistory, serializedGameHistory);
        //SessionHelper.SetObjectAsJson(HttpContext.Session, "History", serializedGameHistory);
    }
    private GameConfigBrain RestoreDataJson(string name)
    {
        var i = SessionHelper.GetObjectFromJson<GameConfigBrain>(HttpContext.Session,
            Request.Query["GameName"].ToString());
        return i!;
    }    
    
    private SaveGameDTO.GamePlayDTO RestoreDataJsonDTO(string name, string path)
    {
        var gameName = GetDataFromJson(name, path);
        SaveGameDTO.GamePlayDTO i;
        if (path.Equals("GameHistory"))
        {
            var currentData = JsonSerializer.Deserialize<SaveGameDTO.GameHistory>(gameName);
            var pathHistory = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "GameHistoryTemp"
                              + Path.DirectorySeparatorChar + GameConfig.GameName + ".json";
            //Console.WriteLine(path);
            var jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            var serialized = JsonSerializer.Serialize(currentData, jsonOptions);

            System.IO.File.WriteAllTextAsync(pathHistory, serialized);
            i = currentData!.GamePlayDto[currentData.GamePlayDto.Count - 1];
        }
        else
        {
            i = JsonSerializer.Deserialize<SaveGameDTO.GamePlayDTO>(gameName)!;
        }
        
        return i!;
    }    
    
    private SaveGameDTO.GameHistory RestoreHistoryDataJsonDTO(string name, string path)
    {
        var gameName = GetDataFromJson(name, path);
        //Console.WriteLine(gameName);
        var i = JsonSerializer.Deserialize<SaveGameDTO.GameHistory>(gameName);
        return i!;
    }

    private string GetDataFromJson(string name, string path)
    {
        var gameFilesSave = Directory
            .EnumerateFiles(
                Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + $"{path}" +
                Path.DirectorySeparatorChar, "*.json").ToList();
        var gameName = "";
        foreach (var gameFile in gameFilesSave)
        {
            var fileData1 = gameFile.Split(".");
            var fileData2 = fileData1[0].Split(@"\");
            if (fileData2[^1].Equals(name))
            {
                gameName = System.IO.File.ReadAllText(gameFile);
            }
        }

        return gameName;
    }
    
    
    private void InitializeGame(string gameName, string isStarted)
    {
        //IsStarted = true;
        GameConfig = new GameConfigBrain();
        var i = RestoreDataJson(gameName);
        if (i != null)
        {
            GameConfig.GameName = i.GameName;
            GameConfig.BoardSizeX = i.BoardSizeX;
            GameConfig.BoardSizeY = i.BoardSizeY;
            GameConfig.ShipConfigs = i.ShipConfigs;
            GameConfig.EShipTouchRule = i.EShipTouchRule;
        }

        GameBrain = new BsBrain(GameConfig);
    }
    
    private void BindValuesToProperties(SaveGameDTO.GamePlayDTO brainLogic)
    {
        GameBrain.WebRestoreBrainFromJson(brainLogic.GameConfigDTO, brainLogic.GameBoardDTO);
        Score0 = brainLogic.GameBoardDTO.Score0;
        Score1 = brainLogic.GameBoardDTO.Score1;
        SessionHelper.SetObjectAsJson(HttpContext.Session, "Score0", Score0);
        SessionHelper.SetObjectAsJson(HttpContext.Session, "Score1", Score1);
        SessionHelper.SetObjectAsJson(HttpContext.Session, "Player" + "Count", brainLogic.GameBoardDTO.PlayerCount);
        //GameBrain.SetGameConf(data.GameConfigDTO);
        GameConfig = brainLogic.GameConfigDTO;
        IsStartedProp = brainLogic.IsStartedDTO;
        PlayerId = brainLogic.PlayerIdDTO;
        if (brainLogic.ShipConfigBrainDTO != null)
        {
            GameBrain.SetShipConf(0, brainLogic.ShipConfigBrainDTO[0]);
            GameBrain.SetShipConf(1, brainLogic.ShipConfigBrainDTO[1]);
        }
    }

    private void SetShipConfigs(string isStarted)
    {
        var getFromSession0 = IsExist(SessionHelper.GetObjectFromJson<List<ShipConfigBrain>>(HttpContext.Session, "Player0"));
        var getFromSession1 = IsExist(SessionHelper.GetObjectFromJson<List<ShipConfigBrain>>(HttpContext.Session, "Player1"));
        switch (getFromSession0, getFromSession1)
        {
            case (true, true):
                GameBrain.SetShipConf(0, SessionHelper.GetObjectFromJson<List<ShipConfigBrain>>(HttpContext.Session, "Player0"));
                GameBrain.SetShipConf(1, SessionHelper.GetObjectFromJson<List<ShipConfigBrain>>(HttpContext.Session, "Player1"));
                break;
            case (false, true):
                GameBrain.SetShipConf(1, SessionHelper.GetObjectFromJson<List<ShipConfigBrain>>(HttpContext.Session, "Player1"));
                break;
            case (true, false):
                GameBrain.SetShipConf(0, SessionHelper.GetObjectFromJson<List<ShipConfigBrain>>(HttpContext.Session, "Player0"));
                break;
        }
    }

    private bool IsExist(List<ShipConfigBrain>? element)
    {
        return element != null;
    }

}