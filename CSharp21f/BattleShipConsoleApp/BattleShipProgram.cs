using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using BattleShipBrain;
using BattleShipConsoleUI;
using DAL;
using Domain;
using MenuSystem;
using Microsoft.AspNetCore.Http;
using WebApp.Helpers;

namespace BattleShipConsoleApp
{

    class BattleShipProgram
    {

        private static GameConfigBrain GameConfig { get; set; } = default!;
        //private static GameJsonOptions _jsonOptions = default!;
        private static BsBrain _gameBrain = default!;
        private static Menu _currentMenu = default!;
        private static Menu _mainMenu = default!;

        private static string _shortCut = "";

        private static string _basePath = default!;

        private static int ShipCount = 0;

        private static int Score0 = 0;
        
        private static int Score1 = 0;

        private static bool IsFinished = false;

        private static int ReplayCount = -1;
        private static string Replay = "";

        private static int i = 0;

        private static int PlayerId { get; set; } = 0;

        static void Main(string[] args)
        {
            Console.Clear();
            //using var db = new ApplicationDbContext();
            //db.Database.EnsureDeleted();
            //db.SaveChanges();

            _basePath = args.Length == 1 ? args[0] : System.IO.Directory.GetCurrentDirectory();
            
            GameConfig = new GameConfigBrain();
            GameConfig.CreateDefaultSetup();

            //Console.WriteLine($"Base path: {_basePath}");

            var menu = new Menu("Main menu", EMenuLevel.Root);

            menu.AddMenuItems(new List<MenuItem>()
            {
                new MenuItem("S", "Game", Game)
            });

            SetCurrentMenu(menu);

            menu.Run();
        }

        private static string Game()
        {
            if (_mainMenu == null)
            {
                var gameMenu = new Menu("Game settings", EMenuLevel.GameSettings);

                SetCurrentMenu(gameMenu);

                if (_gameBrain == null)
                {
                    gameMenu.AddMenuItems(new List<MenuItem>()
                    {
                        new MenuItem("L", "Load game", LoadGameMenu),
                        new MenuItem("D", "Start game", StartGame),
                        new MenuItem("G", "Game configuration", GameConfiguration)
                    });
                }

                _mainMenu = gameMenu;
            }

            var res = _mainMenu.Run();

            return res;
        }

        private static string LoadGameMenu()
        {
            var gameMenu = new Menu("Saving options", EMenuLevel.SaveOptions);

            SetCurrentMenu(gameMenu);

            gameMenu.AddMenuItems(new List<MenuItem>()
            {
                new MenuItem("1", "Load from json", LoadGameJson),
                new MenuItem("2", "Load from db", LoadGameDb),
            });

            var res = gameMenu.Run();
            return res;
        }

        private static string PlayerChoose()
        {
            var gameMenu = new Menu("Game", EMenuLevel.Players);

            SetCurrentMenu(gameMenu);

            gameMenu.AddMenuItems(new List<MenuItem>()
            {
                new MenuItem("1", "Player 1", FirstPlayer),
                new MenuItem("2", "Player 2", SecondPlayer),
                new MenuItem("3", "Save game to json", SaveGameJson),
                new MenuItem("4", "Save game to database", SaveGameDb)
            });

            var res = gameMenu.Run();

            return res;
        }

        private static string SaveGameJson()
        {
            JsonOptions.SaveGameJson(_basePath, _gameBrain, Score0, Score1, ShipCount, true);
            return "";
        }

        private static string SaveGameDb()
        {
            DbOptions.SaveGameDb(_gameBrain, _basePath, Score0, Score1, ShipCount);
            return "";
        }

        private static string LoadGameJson()
        {
            var data = JsonOptions.LoadGameJson(_basePath);
            
            _gameBrain = new BsBrain(data.GameConfigDTO);
            BindValues(data);
            /*
            _gameBrain.WebRestoreBrainFromJson(data.GameConfigDTO, data.GameBoardDTO);
            */

            if (_shortCut.Equals("P")) return "";
            _mainMenu.AddMenuItem(new MenuItem("P", "Game", PlayerChoose));
            _shortCut = "P";

            return "";
        }

        private static string LoadGameDb()
        {
            var data = DbOptions.LoadGameDb();

            RestoreDataFromDb(data);

            /*
            GameConfig.ShipConfigs = data.Item2;
            _gameBrain = new BsBrain(GameConfig);
            _gameBrain.RestoreBrainFromJson(data.Item1, GameConfig);
            */

            if (!_shortCut.Equals("P"))
            {
                _mainMenu.AddMenuItem(new MenuItem("P", "Game", PlayerChoose));
                _shortCut = "P";
            }
            
            return "";
        }
        
        private static void RestoreDataFromDb(GameConfig gameConfig)
        {
            var gameBoard = gameConfig.GameBoards!.FirstOrDefault();
            var gameHistory = gameConfig.GameHistories!.FirstOrDefault();
            var shipList = gameBoard!.Ships;
            var shipConfig = gameConfig.ShipConfigs;
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
                var shipBrain = new ShipBrain(ship.ShipName, coordinatesDto!, ship.Length, ship.Height, ship.Direction);
                listOfShipsBrain.Add(shipBrain);
            }

            var gameBoardDto = new SaveGameDTO.GameBoardDTO()
            {
                BoardA = gameBoardBrain!,
                Ships = listOfShipsBrain,
                Score0 = gameBoard.Score0,
                Score1 = gameBoard.Score1,
                PlayerCount = gameBoard.PlayerCount
            };

            Console.Write("Choose player");
            var userInput = Console.ReadLine()?.Trim();
            var userInputAsInt = int.Parse(userInput!);
            var listOfPlayersShipConfigs = new List<List<ShipConfigBrain>>();
            var listOfPlayer0 = new List<ShipConfigBrain>();
            var listOfPlayer1 = new List<ShipConfigBrain>();
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
                PlayerIdDTO = userInputAsInt,
                ShipConfigBrainDTO = listOfPlayersShipConfigs
                
            };
            
            var deserializedHistory = JsonSerializer.Deserialize<SaveGameDTO.GameHistory>(gameHistory!.Data);
            var gameHistoryDto = new SaveGameDTO.GameHistory()
            {
                GamePlayDto = deserializedHistory!.GamePlayDto
            };
        
            var pathHistory = _basePath + "WebApp" + Path.DirectorySeparatorChar + "GameHistoryTemp"
                              + Path.DirectorySeparatorChar + gameConfig.GameName + ".json";
            //Console.WriteLine(path);
            var jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            var serialized = JsonSerializer.Serialize(gameHistoryDto, jsonOptions);

            System.IO.File.WriteAllTextAsync(pathHistory, serialized);

            _gameBrain = new BsBrain(gameConfigBrain);
            BindValues(gamePlayDto);
        
            //Console.WriteLine(shipList);
        }

        private static void BindValues(SaveGameDTO.GamePlayDTO gamePlayDto)
        {
            _gameBrain.WebRestoreBrainFromJson(gamePlayDto.GameConfigDTO, gamePlayDto.GameBoardDTO);
            Score0 = gamePlayDto.GameBoardDTO.Score0;
            Score1 = gamePlayDto.GameBoardDTO.Score1;
            ShipCount = gamePlayDto.GameBoardDTO.PlayerCount;
            GameConfig = gamePlayDto.GameConfigDTO;
            _gameBrain.SetPlayerNo(gamePlayDto.PlayerIdDTO);
            if (gamePlayDto.ShipConfigBrainDTO != null)
            {
                _gameBrain.SetShipConf(0, gamePlayDto.ShipConfigBrainDTO[0]);
                _gameBrain.SetShipConf(1, gamePlayDto.ShipConfigBrainDTO[1]);
            }
        }

        private static string ReplayGame()
        {
            List<string> option = new List<string>() { "Undo", "Redo", "Continue"};
            Console.WriteLine("Please choose option:");
            var i = 1;
            foreach (var data in option)
            {
                Console.WriteLine($"{i})" + data);
                i++;
            }

            var userChoice = Console.ReadLine()!.Trim();
            var userChoiceInt = int.Parse(userChoice) - 1;
            Replay = option[userChoiceInt];
            var dataHistory = RestoreHistoryDataJsonDTO(_gameBrain.GetConfigBrain().GameName, "GameHistoryTemp");
            if (ReplayCount == -1)
            {
                ReplayCount = dataHistory.GamePlayDto.Count - 1;
            }
            switch (Replay)
            {
                case "Undo":
                    Replay = "Undo";
                    ReplayCount--;
                    break;
                case "Redo":
                    Replay = "Redo";
                    ReplayCount++;
                    break;
                case "Continue":
                    List<SaveGameDTO.GamePlayDTO> tempData = new List<SaveGameDTO.GamePlayDTO>();
                    var count = -1;
                    foreach (var gamePlayDto in dataHistory.GamePlayDto)
                    {
                        if (count == ReplayCount)
                        {
                            break;
                        }
                        tempData.Add(gamePlayDto);
                        count++;
                    }

                    dataHistory.GamePlayDto = tempData;
                    break;
            }

            if (ReplayCount >= 0)
            {
                //Console.WriteLine(ReplayCount);
                if (Replay.Equals("Continue"))
                {
                    BindValues(dataHistory.GamePlayDto[dataHistory.GamePlayDto.Count - 1]);
                    SaveHistory(false);
                }
                else
                {
                    BindValues(dataHistory.GamePlayDto[ReplayCount]);
                    SaveHistory(false);
                }
            }
            
            BsConsoleUi.DrawBoard(_gameBrain.GetBoard(_gameBrain.GetCurrentPlayer(), "A", false));
            
            return "";
        }

        private static string FirstPlayer()
        {
            var gameMenu = new Menu("Game menu", EMenuLevel.Game);
            PlayerId = 0;

            Console.WriteLine("First player");
            gameMenu.AddMenuItems(new List<MenuItem>()
                {
                    new MenuItem("1", "Place bomb", SetBombs),
                    new MenuItem("2", "Set ships", SetShips),
                    new MenuItem("3", "Set ships automatically", SetShipsAutomatically),
                    new MenuItem("4", "ReplayGame", ReplayGame)
                });
            
            Console.WriteLine("Player0 score - " + Score0);

            gameMenu.PlayerId = "Player";

            SetCurrentMenu(gameMenu);

            _gameBrain.SetPlayerNo(PlayerId);

            SetCurrentMenus();

            var res = gameMenu.Run();
            
            SaveHistory(true);

            return res;
        }

        private static void SetCurrentMenus()
        {
            if (_gameBrain.GetCurrentPlayer() == 0)
            {
                _currentMenu.Board = _gameBrain.GetBoard(0, "A", false);

                _currentMenu.Board2 = _gameBrain.GetBoard(1, "A", true);
                
            }
            else
            {
                _currentMenu.Board = _gameBrain.GetBoard(1, "A", false);

                _currentMenu.Board2 = _gameBrain.GetBoard(0, "A", true);
            }
        }

        private static string SecondPlayer()
        {
            var gameMenu = new Menu("Game menu", EMenuLevel.Game);
            PlayerId = 1;
            gameMenu.AddMenuItems(new List<MenuItem>()
            {
                new MenuItem("1", "Place bomb", SetBombs),
                new MenuItem("2", "Set ships", SetShips),
                new MenuItem("3", "Set ships automatically", SetShipsAutomatically),
                new MenuItem("4", "ReplayGame", ReplayGame)
            });
            gameMenu.PlayerId = "Enemy";
            
            Console.WriteLine("Player1 score - " + Score1);
            
            SetCurrentMenu(gameMenu);
            
            _gameBrain.SetPlayerNo(PlayerId);
            
            SetCurrentMenus();
            
            var res = gameMenu.Run();
            SaveHistory(true);

            return res;
        }

        private static string GameConfiguration()
        {
            var gameMenu = new Menu("Settings", EMenuLevel.GameSettings);
            
            gameMenu.AddMenuItems(new List<MenuItem>()
            {
                new MenuItem("1", "Board size", BoardSettings),
                new MenuItem("2", "Ships config", ShipSettings),
                new MenuItem("3", "Touch Rules", SetTouchRule)
            });
            
            var res = gameMenu.Run();
            
            /*
            if (!_shortCut.Equals("P"))
            {
                _mainMenu.AddMenuItem(new MenuItem("P", "Game", PlayerChoose));
                _shortCut = "P";
            }
            */
            
            return res;
        }
        
        private static string StartGame()
        {
            
            Console.WriteLine("Please give name to game");
            var userChoose = Console.ReadLine()!.Trim();
            var gameConfig = GameConfig != null ? GameConfig : new GameConfigBrain();
            gameConfig.GameName = userChoose;
            _gameBrain = new BsBrain(gameConfig);
            if (i == 0)
            {
                SaveHistory(true);
                i++;
            }

            if (!_shortCut.Equals("P"))
            {
                _mainMenu.AddMenuItem(new MenuItem("P", "Game", PlayerChoose));
                _shortCut = "P";
            }
            
            return "";
        }

        private static void SaveHistory(bool notSave)
        {
            
            var jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            
            var gBoard = new SaveGameDTO.GameBoardDTO()
            {
                BoardA = _gameBrain.GetBrainBoardAsArrays(),
                Ships = _gameBrain.GetListOfShips(),
                Score0 = Score0,
                Score1 = Score1,
                PlayerCount = ShipCount
            };
        
            var gamePlay = new SaveGameDTO.GamePlayDTO()
            {
                GameBoardDTO = gBoard,
                GameConfigDTO = GameConfig,
                IsStartedDTO = true,
                PlayerIdDTO = PlayerId,
                ShipConfigBrainDTO = _gameBrain.GetAllShipConfigs()
            };

            var lst = new List<SaveGameDTO.GamePlayDTO>();
            lst.Add(gamePlay);
            var gameHistory = new SaveGameDTO.GameHistory()
            {
                GamePlayDto = lst
            };
            
                    //var gameConfigBrainJsonData = JsonSerializer.Serialize(gamePlay, jsonOptions);
        var gameConfigBrainJsonData2 = JsonSerializer.Serialize(gameHistory, jsonOptions);
        
        var pathHistoryTemp = _basePath + "WebApp" + Path.DirectorySeparatorChar + "GameHistoryTemp"
                   + Path.DirectorySeparatorChar + GameConfig.GameName + ".json";    
        var pathHistory = _basePath + "WebApp" + Path.DirectorySeparatorChar + "GameHistoryTemp"
                          + Path.DirectorySeparatorChar + GameConfig.GameName + "History" + ".json";        

        //Console.WriteLine(path);
        //System.IO.File.WriteAllTextAsync(pathGamePlay, gameConfigBrainJsonData);
        if (notSave)
        {
            if (System.IO.File.Exists(pathHistoryTemp))
            {
                var data = RestoreHistoryDataJsonDTO(GameConfig.GameName,  "GameHistoryTemp");
                data.GamePlayDto.Add(gamePlay);
                gameConfigBrainJsonData2 = JsonSerializer.Serialize(data, jsonOptions);
                ReplayCount = data.GamePlayDto.Count - 1;
                //SessionHelper.SetObjectAsJson(HttpContext.Session, "ReplayCount", ReplayCount);
                //Console.WriteLine("ReplayCount value is " + ReplayCount);
                System.IO.File.WriteAllTextAsync(pathHistoryTemp, gameConfigBrainJsonData2);
                //System.IO.File.WriteAllTextAsync(pathHistory, gameConfigBrainJsonData2);
            }
            else
            {
                System.IO.File.WriteAllTextAsync(pathHistoryTemp, gameConfigBrainJsonData2);
                //System.IO.File.WriteAllTextAsync(pathHistory, gameConfigBrainJsonData2);
            }
        }

        if (!notSave && Replay.Equals("Continue"))
        {
            System.IO.File.Delete(pathHistoryTemp);
            System.IO.File.WriteAllTextAsync(pathHistoryTemp, gameConfigBrainJsonData2);
        }
        }
        
        
        private static SaveGameDTO.GameHistory RestoreHistoryDataJsonDTO(string name, string path)
        {
            var gameName = GetDataFromJson(name, path);
            //Console.WriteLine(gameName);
            var i = JsonSerializer.Deserialize<SaveGameDTO.GameHistory>(gameName);
            return i!;
        }

        private static string GetDataFromJson(string name, string path)
        {
            var gameFiles = Directory
                .EnumerateFiles(
                    _basePath + "WebApp" +
                    Path.DirectorySeparatorChar + "GameHistoryTemp" +
                    Path.DirectorySeparatorChar, "*.json").ToList();
            var gameName = "";
            foreach (var gameFile in gameFiles)
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

        private static string SetTouchRule()
        {
            List<string> touchRules = new List<string>() { "NoTouch", "CornerTouch", "SideTouch"};
            var i = 1;
            foreach (var touchRule in touchRules)
            {
                Console.WriteLine($"{i}) {touchRule}");
            }

            var userInput = Console.ReadLine()!.Trim();
            EShipTouchRule touchRuleEnum;
            switch (userInput)
            {
                case "1":
                    touchRuleEnum = EShipTouchRule.NoTouch;
                    break;
                case "2":
                    touchRuleEnum = EShipTouchRule.CornerTouch;
                    break;
                case "3":
                    touchRuleEnum = EShipTouchRule.SideTouch;
                    break;
                default:
                    touchRuleEnum = EShipTouchRule.NoTouch;
                    break;
            }

            _gameBrain.GetConfigBrain().EShipTouchRule = touchRuleEnum;
            return "";
        }

        private static string BoardSettings()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            
            Console.WriteLine("If you won't put value to X or Y it will be set by default value - 10");
            
            Console.WriteLine("Values must be bigger than 10");
            
            Console.ResetColor();
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            
            var xSize = "";
            
            var ySize = "";
            
            var i = false;
            
            do
            {
                if (i)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    
                    Console.WriteLine("One of the values is less than 10");
                    
                    Console.ResetColor();
                    
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                
                Console.Write("Mark board size by X: ");
                
                xSize = Console.ReadLine()?.Trim();
                
                if (string.IsNullOrWhiteSpace(xSize))
                {
                    xSize = "10";
                }
                
                Console.Write("Mark board size by Y: ");
                
                ySize = Console.ReadLine()?.Trim();
                
                if (string.IsNullOrWhiteSpace(ySize))
                {
                    ySize = "10";
                }

                if (int.Parse(xSize) < 10 || int.Parse(ySize) < 10)
                {
                    i = true;
                }
                
            } while (int.Parse(xSize) < 10 || int.Parse(ySize) < 10);

            var gameConfig = new GameConfigBrain
            {
                BoardSizeX = int.Parse(xSize),
                BoardSizeY = int.Parse(ySize)
            };

            GameConfig = gameConfig;
            if (gameConfig.ShipConfigs == null)
            {
                gameConfig.CreateDefaultSetup();
            }
            _gameBrain = new BsBrain(gameConfig);
            
            if (!_shortCut.Equals("P"))
            {
                _mainMenu.AddMenuItem(new MenuItem("P", "Game", PlayerChoose));
                _shortCut = "P";
            }
            return "";
        }

        private static string ShipSettings()
        {
            var gameMenu = new Menu("Ship configure", EMenuLevel.GameSettings);

            gameMenu.AddMenuItems(new List<MenuItem>()
            {
                new MenuItem("1", "Quantity", ShipQuantity),
                new MenuItem("2", "Size", ShipSize),
                new MenuItem("3", "Remove ship", RemoveShipConfig),
                new MenuItem("4", "Add ship", AddShipConfig)
            });

            var res = gameMenu.Run();
            return res;
        }

        private static string ShipQuantity()
        {
            Console.WriteLine("Choose which ship type quantity you want to increase/decrease:");
            
            var i = 1;
            
            foreach (var shipConfig in GameConfig.ShipConfigs)
            {
                Console.WriteLine($"{i}) {shipConfig.Name} {shipConfig.ShipSizeX} x {shipConfig.ShipSizeY}, quantity {shipConfig.Quantity}");
                i++;
            }
            
            Console.Write("User choice is: ");
            
            var userChoice = Console.ReadLine()?.Trim();
            
            Console.Write("Do you want to increase or decrease?\n" +
                          "If decrease write 0\n" +
                          "If increase write 1\n" +
                          "User choice is: ");
            
            var userChoice2 = Console.ReadLine()?.Trim();
            
            switch (int.Parse(userChoice2!))
            {
                case 0:
                    GameConfig.ShipConfigs[int.Parse(userChoice!) - 1].Quantity--;
                    break;
                case 1:
                    GameConfig.ShipConfigs[int.Parse(userChoice!) - 1].Quantity++;
                    break;
            }

            return "";
        }
        private static string ShipSize()
        {
            Console.WriteLine("Choose which ship type size you want to change:");
            
            var i = 1;
            
            foreach (var shipConfig in GameConfig.ShipConfigs)
            {
                Console.WriteLine($"{i}) {shipConfig.Name} {shipConfig.ShipSizeX} x {shipConfig.ShipSizeY}, quantity {shipConfig.Quantity}");
                i++;
            }
            
            Console.Write("User choice is: ");
            
            var userChoice = Console.ReadLine()?.Trim();
            
            Console.WriteLine("Please mark new ship size");
            
            Console.Write("By X: ");
            
            var sizeByX = Console.ReadLine()?.Trim();
            
            Console.Write("By Y: ");
            
            var sizeByY = Console.ReadLine()?.Trim();
            
            GameConfig.ShipConfigs[int.Parse(userChoice!) - 1].ShipSizeX = int.Parse(sizeByX!);
            
            GameConfig.ShipConfigs[int.Parse(userChoice!) - 1].ShipSizeY = int.Parse(sizeByY!);
            
            return "";
        }

        private static string RemoveShipConfig()
        {
            Console.WriteLine("Choose which ship type you want to remove:");
            
            var i = 1;
            
            foreach (var shipConfig in GameConfig.ShipConfigs)
            {
                Console.WriteLine($"{i}) {shipConfig.Name} {shipConfig.ShipSizeX} x {shipConfig.ShipSizeY}");
                i++;
            }
            
            Console.Write("User choice is: ");
            
            var userChoice = Console.ReadLine()?.Trim();
            
            var gameConfigShipConfig = GameConfig.ShipConfigs[int.Parse(userChoice!) - 1];
            
            GameConfig.ShipConfigs.Remove(gameConfigShipConfig);
            
            return "";
        }
        
        private static string AddShipConfig()
        {
            Console.Write("New ship name: ");
            
            var shipName = Console.ReadLine()?.Trim();
            
            Console.Write("Quantity: ");
            
            var shipQuantity = Console.ReadLine()?.Trim();
            
            Console.Write("Size - X: ");
            
            var shipSizeX = Console.ReadLine()?.Trim();
            
            Console.Write("Size - Y: ");
            
            var shipSizeY = Console.ReadLine()?.Trim();
            
            var shipConfig = new ShipConfigBrain
            {
                Name = shipName!,
                Quantity = int.Parse(shipQuantity!),
                ShipSizeX = int.Parse(shipSizeX!),
                ShipSizeY = int.Parse(shipSizeY!)
            };
            
            GameConfig.ShipConfigs.Add(shipConfig);
            
            return "";
        }

        private static string SetShips()
        {
            List<ShipBrain> listOfShips = new List<ShipBrain>();

            var count = 0;
            
            Console.WriteLine("Choose battleship type:");

            var i = 1;

            foreach (var shipConfig in _gameBrain.GetShipConfig(_gameBrain.GetCurrentPlayer()))
            {
                Console.WriteLine(
                    $"{i}) Name: {shipConfig.Name}, Quantity: {shipConfig.Quantity}, Size: {shipConfig.ShipSizeX} x {shipConfig.ShipSizeY}");
                i++;
            }

            var userShipInput = "";
            
            do
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                
                Console.WriteLine("Choose ship type:");
                
                userShipInput = Console.ReadLine()?.Trim()!;
                
                Console.ResetColor();
                
            } while (string.IsNullOrEmpty(userShipInput));

            Console.ForegroundColor = ConsoleColor.Cyan;
            
            Console.WriteLine("Choose battleship direction:");
            
            Console.WriteLine("1) Horizontal");
            
            Console.WriteLine("2) Vertical");
            
            string userDirectionInput = "";
            
            do
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                
                Console.WriteLine("Choose ship direction:");
                
                userDirectionInput = Console.ReadLine()?.Trim()!;
                
                Console.ResetColor();
                
            } while (string.IsNullOrEmpty(userDirectionInput));

            userDirectionInput = userDirectionInput == "1" ? "Horizontal" : "Vertical";
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            
            Console.WriteLine("Write coordinate(x) to set a ship");
            
            var x = Int32.Parse(Console.ReadLine()?.Trim()!);
            
            
            if (x < 0 ||
                x > _gameBrain.GetConfigBrain().BoardSizeX - 1 ||
                x + _gameBrain.GetConfigBrain().ShipConfigs[int.Parse(userShipInput) - 1].ShipSizeY
                > _gameBrain.GetBoard(PlayerId, "A", null).GetLength(0))
            {
                do
                {
                    Console.WriteLine("Incorrect value. please try it again: ");
                    
                    Console.WriteLine("Write coordinate(x) to set a ship");
                    
                    x = Int32.Parse(Console.ReadLine()?.Trim()!);
                    
                } while (x > _gameBrain.GetConfigBrain().BoardSizeX - 1 || x < 0 || x + _gameBrain.GetConfigBrain().ShipConfigs[int.Parse(userShipInput) - 1].ShipSizeY
                         > _gameBrain.GetBoard(PlayerId, "A", null).GetLength(0));
            }

            Console.WriteLine("Write coordinate(y) to set a ship");
            
            var y = Int32.Parse(Console.ReadLine()?.Trim()!);
            
            if (y < 0 ||
                y > _gameBrain.GetConfigBrain().BoardSizeY - 1 ||
                y + _gameBrain.GetConfigBrain().ShipConfigs[int.Parse(userShipInput) - 1].ShipSizeY 
                > _gameBrain.GetBoard(PlayerId, "A", null).GetLength(1) && userDirectionInput == "Vertical")
            {
                do
                {
                    Console.WriteLine("Incorrect value. please try it again: ");
                    
                    Console.WriteLine("Write coordinate(y) to set a ship");
                    
                    y = Int32.Parse(Console.ReadLine()?.Trim()!);
                    
                } while (y > _gameBrain.GetConfigBrain().BoardSizeY - 1 || y < 0 || y + _gameBrain.GetConfigBrain().ShipConfigs[int.Parse(userShipInput) - 1].ShipSizeY 
                         > _gameBrain.GetBoard(PlayerId, "A", null).GetLength(1) && userDirectionInput == "Vertical");
            }


            var shipName = _gameBrain.GetConfigBrain().ShipConfigs[int.Parse(userShipInput) - 1].Name;
            foreach (var shipConfigBrain in _gameBrain.GetShipConfig(PlayerId))
            {
                if (shipConfigBrain.Name.Equals(shipName))
                {
                    var lenght = shipConfigBrain.ShipSizeX;
                    var height = shipConfigBrain.ShipSizeY;
                    
                    List<Coordinate> coordinates = new();
                    switch (userDirectionInput)
                    {
                        case "Horizontal":
                            for (var num = x; num < x + height; num++)
                            {
                                //Console.WriteLine(new Coordinate(){X = num, Y = y});
                                coordinates.Add(new Coordinate(){X = num, Y = y});
                            }
                            break;
                        case "Vertical":
                            for (var num = y; num < y + height; num++)
                            {
                                coordinates.Add(new Coordinate(){X = x, Y = num});
                            }
                            break;
                    }

                    if (shipConfigBrain.Quantity > 0)
                    {
                        ShipBrain ship = new(
                            shipName,
                            coordinates,
                            lenght,
                            height,
                            userDirectionInput);
            
                        listOfShips.Add(ship);
            
                        count++;
                        _gameBrain.SetPlayerNo(PlayerId);
                        _gameBrain.SetShip(ship);
                        if (_gameBrain.GetPlayerNo() == 0)
                        {
                            //currentShipType!.Quantity--;
                            
                            BsConsoleUi.DrawBoard(_gameBrain.GetBoard(0, "A", false));
                        }

                        if (_gameBrain.GetPlayerNo() == 1)
                        {
                            //currentShipType!.Quantity--;
                            BsConsoleUi.DrawBoard(_gameBrain.GetBoard(1, "A", false));
                        }
                    }
                }
            }
                
            
            SetCurrentMenus();

            ShipCount += listOfShips.Count;
            //Console.WriteLine(ShipCount);
            return "";
        }
        
        public static string SetShipsAutomatically()
        {
            var rnd = new Random();
            if (ShipCount == 5 && _gameBrain.GetCurrentPlayer() == 0)
            {
                Console.WriteLine("You already put max number of ships");
                return "";
            }

            if (ShipCount > 5 && _gameBrain.GetCurrentPlayer() == 1)
            {
                Console.WriteLine("You already put max number of ships");
                return "";
            }
            var minimumNum = 0;
            List<string> directions = new List<string>() {"Vertical", "Horizontal"};
            do
            {
                var valid = false;
                //SetShipConfigs("true");
                do
                {
                    int shipX;
                    int shipY;
                    var randomShipType = _gameBrain.GetShipConfig(PlayerId)[rnd.Next(0, _gameBrain.GetShipConfig(PlayerId).Count - 1)];
                    do
                    {
                        shipY = GetRandomNumber();
                    } while (shipY + randomShipType.ShipSizeY > _gameBrain.GetBoard(PlayerId, "A", null).GetLength(1) && directions[0].Equals("Vertical"));                
                    do
                    {
                        shipX = GetRandomNumber();
                    } while (shipX + randomShipType.ShipSizeY > _gameBrain.GetBoard(PlayerId, "A", null).GetLength(0));
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
                    
                        _gameBrain.SetPlayerNo(PlayerId);
                        valid = _gameBrain.SetShip(ship);
                    }
                } while (valid == false);
                var i = _gameBrain.GetShipConfig(PlayerId);
                //SessionHelper.SetObjectAsJson(HttpContext.Session, "Player" + Request.Query["PlayerId"].ToString(), i);
                minimumNum++;
            } while (minimumNum != 5);
            
            
            if (_gameBrain.GetPlayerNo() == 0)
            {
                //currentShipType!.Quantity--;
                BsConsoleUi.DrawBoard(_gameBrain.GetBoard(0, "A", false));
            }

            if (_gameBrain.GetPlayerNo() == 1)
            {
                //currentShipType!.Quantity--;
                BsConsoleUi.DrawBoard(_gameBrain.GetBoard(1, "A", false));
            }
            
            
            ShipCount += minimumNum;


            SetCurrentMenus();
            SaveHistory(true);

            return "";
        }

        public static int GetRandomNumber()
        {
            var rnd = new Random();
            return rnd.Next(0, GameConfig.BoardSizeX);
        }
        
        private static string SetBombs()
        {
            var x = 0;
            do
            {
                Console.WriteLine("Write coordinates(x) to put a bomb:");
            
                x = int.Parse(Console.ReadLine()!.Trim());
            } while (x < 0 || x > _gameBrain.GetConfigBrain().BoardSizeX - 1);
            
            var y = 0;
            do
            {
                Console.WriteLine("Write coordinates(y) to put a bomb:");
            
                y = int.Parse(Console.ReadLine()!.Trim());
            } while (y < 0 || y > _gameBrain.GetConfigBrain().BoardSizeX - 1);

            //var playerNum = _currentMenu.PlayerId;
            
            Console.WriteLine("Current player is: Player" + _gameBrain.GetCurrentPlayer());

            var ship = _gameBrain.SetBomb(x, y, _gameBrain.GetCurrentPlayer());
            var attackResult = ship == "" ? "miss" : "hit";
            var currentScore = _gameBrain.ShipChange(_gameBrain.GetCurrentPlayer(), ship);
            switch (_gameBrain.GetCurrentPlayer())
            {
                case 0:
                    Score0 += currentScore;
                    Console.WriteLine("Your board");
                    BsConsoleUi.DrawBoard(_gameBrain.GetBoard(0, "A", false));
                    Console.WriteLine("Enemy board");
                    BsConsoleUi.DrawBoard(_gameBrain.GetBoard(1, "A", true));
                    Console.WriteLine("Your score: " + Score0);
                    if (Score0 == _gameBrain.GetConfigBrain().BoardSizeX / 2)
                    {
                        IsFinished = true;
                        Console.WriteLine("Player0 is win!");
                        return "";
                    }
                    break;
                case 1:
                    Score1 += currentScore;
                    Console.WriteLine("Your board");
                    BsConsoleUi.DrawBoard(_gameBrain.GetBoard(1, "A", false));
                    Console.WriteLine("Enemy board");
                    BsConsoleUi.DrawBoard(_gameBrain.GetBoard(0, "A", true));
                    Console.WriteLine("Your score: " + Score1);
                    if (Score1 == _gameBrain.GetConfigBrain().BoardSizeX / 2)
                    {
                        IsFinished = true;
                        Console.WriteLine("Player1 is win!");
                        return "";
                    }
                    break;
            }

            if (attackResult.Equals("hit"))
            {
                return SetBombs();
            }
            
            
            /*
            if (_gameBrain.GetPlayerNo() == 0)
            {
                _gameBrain.SetBomb(int.Parse(x), int.Parse(y), 0);
                
                //BsConsoleUi.DrawBoard(_gameBrain.GetBoard(0, "B"));
                
                SetCurrentMenus();
                
            }

            if (_gameBrain.GetPlayerNo() == 1)
            {
                _gameBrain.SetBomb(int.Parse(x), int.Parse(y), 1);
                
                //BsConsoleUi.DrawBoard(_gameBrain.GetBoard(1, "B"));
                
                SetCurrentMenus();
            }
            */

            SaveHistory(true);

            return "x";
        }
        
        private static void SetCurrentMenu(Menu menu)
        {
            _currentMenu = menu;
        }
    }
}