using System;
using System.Collections.Generic;

namespace BattleShipBrain
{
    public class ShipBrain
    {
        public string Name { get; set; }
        
        public List<Coordinate> Coordinates { get; set; }
        public int Length { get; set; }
        public int Height { get; set; }

        public string Direction { get; set; }

        public ShipBrain(string name, List<Coordinate> coordinates, int length, int height, string direction)
        {
            Name = name;
            Direction = direction;
            Length = length;
            Height = height;
            Coordinates = coordinates;
        }


        private string GetDirection()
        {
            return Direction;
        }

        public List<Coordinate> GetCoordinates()
        {
            return Coordinates;
        }

        public int GetShipSize()
        {
            return Coordinates.Count;
        }

        public int GetShipDamageCount(BoardSquareState[,] board)
        {
            var count = 0;
            foreach (var coordinate in Coordinates)
            {
                if (board[coordinate.X, coordinate.Y].IsBomb)
                {
                    count++;
                }
            }
            return count;
        }

        public bool IsAlive(BoardSquareState[,] board)
        {
            foreach (var coordinate in Coordinates)
            {
                if (board[coordinate.X, coordinate.Y].IsBomb)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        

        /*
        public override string ToString()
        {
            return $"Ship coordinates: ({XCoordinate},{YCoordinate}), Ship direction: {_dir}, Ship type is {_shipType}, Is alive: {_isAlive}";
        }
        */
    }
}