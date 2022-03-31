using System;
using BattleShipBrain;

namespace BattleShipConsoleUI
{
    public static class BsConsoleUi
    {
        public static void DrawBoard(BoardSquareState[,] board)
        {
            var width = board.GetLength(0);
            var height = board.GetLength(1);

            for (int colIndex = 0; colIndex < width; colIndex++)
            {
                //Console.Write($"  {colIndex}  ");
                Console.Write($"+-{colIndex}-+");
            }
            
            Console.WriteLine();
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Console.Write(x == 0 ? $"{y} {board[x, y]} |" : $"| {board[x, y]} |");
                }
                Console.WriteLine();

                for (int x = 0; x < width; x++)
                {
                    Console.Write("+---+");
                }
                Console.WriteLine();
            }
        }
    }
}