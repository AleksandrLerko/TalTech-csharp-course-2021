using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Domain;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BattleShipBrain
{
    public class JsonOptions
    {
        public static void SaveGameJson(string basePath, BsBrain brain, int score0, int score1, int playerCount, bool save)
        {
            // Console.Write("Write game save name: ");
            // var userInput = Console.ReadLine()?.Trim();
            
            var jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            
            var gBoard = new SaveGameDTO.GameBoardDTO()
            {
                BoardA = brain.GetBrainBoardAsArrays(),
                Ships = brain.GetListOfShips(),
                Score0 = score0,
                Score1 = score1,
                PlayerCount = playerCount
            };

            var conf = brain.GetConfigBrain();
            conf.GameName = brain.GetConfigBrain().GameName;
            var gamePlay = new SaveGameDTO.GamePlayDTO()
            {
                GameBoardDTO = gBoard,
                GameConfigDTO = conf,
                IsStartedDTO = true,
                PlayerIdDTO = brain.GetPlayerNo(),
                ShipConfigBrainDTO = brain.GetAllShipConfigs()
            };
            
            var lst = new List<SaveGameDTO.GamePlayDTO>();
            lst.Add(gamePlay);
            var gameHistory = new SaveGameDTO.GameHistory()
            {
                GamePlayDto = lst
            };
            
            var gameBrainJsonData = JsonSerializer.Serialize(gamePlay, jsonOptions);
            var gameConfigBrainJsonData2 = JsonSerializer.Serialize(gameHistory, jsonOptions);
            //var gameConfigBrainJsonData = JsonSerializer.Serialize(conf, jsonOptions);


            //var confJsonStrBrain = brain.GetBrainJson();

           //var confJsonStrConfig = brain.GetConfigJson();

            var fileNameBrain = basePath +
                                "WebApp" +
                                System.IO.Path.DirectorySeparatorChar +
                                "GameSaves" + Path.DirectorySeparatorChar + brain.GetConfigBrain().GameName + ".json";            
            
            
            var fileNameHistory = basePath +
                                "WebApp" +
                                System.IO.Path.DirectorySeparatorChar +
                                "GameHistoryTemp" + Path.DirectorySeparatorChar + brain.GetConfigBrain().GameName + ".json";
            /*
            var fileNameConf = basePath +
                                "WebApp" +
                                System.IO.Path.DirectorySeparatorChar +
                                "ConfigSaves" + Path.DirectorySeparatorChar + userInput + ".json";
                                */
            
            Console.WriteLine(fileNameBrain);

            Console.WriteLine("Saving default config!");
            System.IO.File.WriteAllText(fileNameBrain, gameBrainJsonData);
            //System.IO.File.WriteAllText(fileNameConf, gameConfigBrainJsonData);

            if (save)
            {
                var pathHistory = basePath + "WebApp" + Path.DirectorySeparatorChar + "GameHistory"
                                  + Path.DirectorySeparatorChar + brain.GetConfigBrain().GameName + "History" + ".json";
                Console.WriteLine(pathHistory);
                //var dezerialized = GetDataFromJson(GameConfig.GameName, "GameHistoryTemp");
                var gameHistoryData = RestoreHistoryDataJsonDTO(brain.GetConfigBrain().GameName, "GameHistoryTemp", basePath);
                var serializedGameHistory = JsonSerializer.Serialize(gameHistoryData, jsonOptions);
                System.IO.File.WriteAllTextAsync(pathHistory, serializedGameHistory);
            }
            
            
        }
        
        private static SaveGameDTO.GameHistory RestoreHistoryDataJsonDTO(string name, string path, string basePath)
        {
            var gameName = GetDataFromJson(name, path, basePath);
            var i = JsonSerializer.Deserialize<SaveGameDTO.GameHistory>(gameName);
            return i!;
        }
        
        private static string GetDataFromJson(string name, string path, string basePath)
        {
            var gameFiles = Directory
                .EnumerateFiles(
                    basePath + "WebApp" + Path.DirectorySeparatorChar + "GameHistoryTemp" +
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
        
        public static SaveGameDTO.GamePlayDTO LoadGameJson(string basePath)
        {
            var gameFiles = System.IO.Directory
                .EnumerateFiles(
                    basePath + "WebApp" + Path.DirectorySeparatorChar +
                    "GameSaves", "*.json").ToList();
            foreach (var gameFile in gameFiles)
            {
                Console.WriteLine(gameFile);
            }

            Console.WriteLine("Game saves: ");
            for (int i = 0; i < gameFiles.Count; i++)
            {
                var fileName = gameFiles[i].Split(@"\");
                Console.WriteLine($"{i + 1} - {fileName[2]}");
            }
                
            Console.Write("Choose game save: ");
            var userInput = Console.ReadLine()?.Trim();
                
            var gameNameArray = gameFiles[int.Parse(userInput!) - 1];

            var dataFromJson = System.IO.File.ReadAllText(gameNameArray);

            var gameData = JsonSerializer.Deserialize<SaveGameDTO.GamePlayDTO>(dataFromJson);

            return gameData!;
        }
    }
}