using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DAL;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace BattleShipBrain
{
    public class DbOptions
    {
        public static string _basePath { get; set; } = default!;
        public static void SaveGameDb(BsBrain gameBrain, string basePath, int score0, int score1, int count)
        {
            _basePath = basePath;
            var jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            var board = gameBrain.GetBrainBoardAsArrays();
            var boardAsString = JsonSerializer.Serialize(board, jsonOptions);
            using var db = new ApplicationDbContext();

            // Console.Write("Save name: ");
            // var userInput = Console.ReadLine()!.Trim();

            var isUpdate = false;

            var touchRule = 0;
            switch (gameBrain.GetConfigBrain().EShipTouchRule)
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
                BoardSizeX = gameBrain.GetConfigBrain().BoardSizeX,
                BoardSizeY = gameBrain.GetConfigBrain().BoardSizeY,
                GameName = gameBrain.GetConfigBrain().GameName,
                EShipTouchRule = touchRule
            };

            var gameBoard = new GameBoard()
            {
                BoardData = boardAsString,
                GameConfig = gameConfig,
                Score0 = score0,
                Score1 = score1,
                PlayerCount = count
            };
            
            foreach (var shipBrain in gameBrain.GetListOfShips())
            {

                //var coordinatesDto = new SaveGameDTO.CoordinatesDTO();
                var coordinates = new List<Coordinate>();
                foreach (var coordinate in shipBrain.Coordinates)
                {
                    coordinates.Add(coordinate);
                }

                //coordinatesDto.Coordinates = coordinates;
                var jsonStr = JsonSerializer.Serialize(coordinates, jsonOptions);

                var ship = new Ship()
                {
                    ShipName = shipBrain.Name,
                    Coordinates = jsonStr,
                    Direction = shipBrain.Direction,
                    Height = shipBrain.Height,
                    Length = shipBrain.Length,
                    GameBoard = gameBoard
                };
                db.Ship.Add(ship);

            }

            foreach (var shipConfig in gameBrain.GetConfigBrain().ShipConfigs)
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
                            

                db.ShipConfigs.Add(shipConf);
                foreach (var allShipConfig in gameBrain.GetAllShipConfigs())
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

                        }
                    }

                    i++;
                }
            }
            var gameHistoryData = RestoreHistoryDataJsonDTO(gameBrain.GetConfigBrain().GameName, "GameHistoryTemp");
            var serializedGameHistory = JsonSerializer.Serialize(gameHistoryData, jsonOptions);
            var gameHistory = new GameHistory()
            {
                Data = serializedGameHistory,
                GameConfig = gameConfig
            };
            
            db.GameConfigs.Add(gameConfig);
            db.GameBoards.Add(gameBoard);
            db.GameHistories.Add(gameHistory);

            var fileNameConf = basePath +
                               "WebApp" +
                               System.IO.Path.DirectorySeparatorChar +
                               "ConfigSaves" + Path.DirectorySeparatorChar + gameBrain.GetConfigBrain().GameName + ".json";
            
            var gameConfigBrainJsonData = JsonSerializer.Serialize(gameBrain.GetConfigBrain(), jsonOptions);
            File.WriteAllText(fileNameConf, gameConfigBrainJsonData);
            
            db.SaveChanges();
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
        
        public static GameConfig LoadGameDb()
        {
            Console.WriteLine("Your saves:");
            
            using var db = new ApplicationDbContext();

            var count = 1;

            var listWithId = db.GameConfigs.Select(x => x.GameConfigId).ToList();

            foreach (var gameConfig in db.GameConfigs)
            {
                Console.WriteLine($"{count}) {gameConfig.GameName}");
                count++;
            }

            Console.Write("Your choice: ");
            var userChoice = Console.ReadLine()!.Trim();

            var selectedSaveId = int.Parse(userChoice) - 1;

            var gameName = "";
            foreach (var config in db.GameConfigs)
            {
                if (config.GameConfigId == listWithId[selectedSaveId])
                {
                    gameName = config.GameName;
                }
            }

            var x = db.GameConfigs
                .Include(g => g.GameBoards)
                .ThenInclude(x => x.Ships)
                .Include(g => g.ShipConfigs)
                .ThenInclude(x => x.ShipQuantities)
                .Include(x => x.GameHistories);
            var gameConf = new GameConfig();
            foreach (var data in x)
            {
                if (data.GameName.Equals(gameName))
                {
                    gameConf = data;
                }
            }


            return gameConf;
        }
    }
}