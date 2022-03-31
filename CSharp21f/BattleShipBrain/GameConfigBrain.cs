using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BattleShipBrain
{
    [Serializable]
    public class GameConfigBrain
    {
        public string GameName { get; set; } = default!;
        public int BoardSizeX { get; set; } = 10;
        public int BoardSizeY { get; set; } = 10;

        public List<ShipConfigBrain> ShipConfigs { get; set; } = default!;

        public EShipTouchRule EShipTouchRule { get; set; } = EShipTouchRule.NoTouch;
        
        public GameConfigBrain DeepCopy(GameConfigBrain gcb)
        {
            return (GameConfigBrain) gcb.MemberwiseClone();;
        }

        public void CreateDefaultSetup()
        {
            ShipConfigs = new List<ShipConfigBrain>()
            {
                new()
                {
                    Name = "Patrol",
                    Quantity = 5,
                    ShipSizeX = 1,
                    ShipSizeY = 1,
                },            
                new()
                {
                    Name = "Cruiser",
                    Quantity = 4,
                    ShipSizeX = 1,
                    ShipSizeY = 2,
                },            
                new()
                {
                    Name = "Submarine",
                    Quantity = 3,
                    ShipSizeX = 1,
                    ShipSizeY = 3,
                },            
                new()
                {
                    Name = "Battleship",
                    Quantity = 2,
                    ShipSizeX = 1,
                    ShipSizeY = 4,
                },            
                new()
                {
                    Name = "Carrier",
                    Quantity = 1,
                    ShipSizeX = 1,
                    ShipSizeY = 5,
                },

            };
        }

        public override string ToString()
        {
            var jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            
            return JsonSerializer.Serialize(this, jsonOptions);
        }
    }
}