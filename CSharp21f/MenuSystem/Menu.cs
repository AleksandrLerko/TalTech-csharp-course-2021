using System;
using System.Collections.Generic;
using System.Linq;
using BattleShipBrain;
using BattleShipConsoleUI;

namespace MenuSystem
{
    public class Menu
    {
        private readonly EMenuLevel _menuLevel;

        private readonly List<MenuItem> _menuItems = new List<MenuItem>();
        private readonly MenuItem _menuItemExit = new MenuItem("E", "Exit", null);
        private readonly MenuItem _menuItemReturn = new MenuItem("R", "Return", null);
        private readonly MenuItem _menuItemMain = new MenuItem("M", "Main", null);

        private readonly HashSet<string> _menuShortCuts = new HashSet<string>();
        private readonly HashSet<string> _menuSpecialShortCuts = new HashSet<string>();

        private readonly string _title;

        private static double _value;

        public string PlayerId = default!;

        public BoardSquareState[,] Board = default!;
        public BoardSquareState[,] Board2 = default!;

        public Menu(string title, EMenuLevel menuLevel)
        {
            _title = title;
            _menuLevel = menuLevel;

            switch (_menuLevel)
            {
                case EMenuLevel.Root:
                    _menuSpecialShortCuts.Add(_menuItemExit.ShortCut.ToUpper());
                    break;
                case EMenuLevel.GameSettings:
                    _menuSpecialShortCuts.Add(_menuItemReturn.ShortCut.ToUpper());
                    _menuSpecialShortCuts.Add(_menuItemMain.ShortCut.ToUpper());
                    _menuSpecialShortCuts.Add(_menuItemExit.ShortCut.ToUpper());
                    break;                       
                case EMenuLevel.SaveOptions:
                    _menuSpecialShortCuts.Add(_menuItemReturn.ShortCut.ToUpper());
                    _menuSpecialShortCuts.Add(_menuItemMain.ShortCut.ToUpper());
                    _menuSpecialShortCuts.Add(_menuItemExit.ShortCut.ToUpper());
                    break;                
                case EMenuLevel.Players:
                    _menuSpecialShortCuts.Add(_menuItemReturn.ShortCut.ToUpper());
                    _menuSpecialShortCuts.Add(_menuItemMain.ShortCut.ToUpper());
                    _menuSpecialShortCuts.Add(_menuItemExit.ShortCut.ToUpper());
                    break;
                case EMenuLevel.Game:
                    _menuSpecialShortCuts.Add(_menuItemReturn.ShortCut.ToUpper());
                    _menuSpecialShortCuts.Add(_menuItemMain.ShortCut.ToUpper());
                    _menuSpecialShortCuts.Add(_menuItemExit.ShortCut.ToUpper());
                    break;
            }
        }

        public void AddMenuItem(MenuItem item, int position = -1)
        {
            if (_menuSpecialShortCuts.Add(item.ShortCut.ToUpper()) == false)
            {
                throw new ApplicationException($"Conflicting menu shortcut {item.ShortCut.ToUpper()}");
            }
            if (_menuShortCuts.Add(item.ShortCut.ToUpper()) == false)
            {
                throw new ApplicationException($"Conflicting menu shortcut {item.ShortCut.ToUpper()}");
            }


            if (position == -1)
            {
                _menuItems.Add(item);
            }
            else
            {
                _menuItems.Insert(position, item);
            }
        }

        public void DeleteMenuItem(int position = 0)
        {
            _menuItems.RemoveAt(position);
        }

        public void AddMenuItems(List<MenuItem> items)
        {
            foreach (var menuItem in items)
            {
                AddMenuItem(menuItem);
            }
        }

        public string Run()
        {
            var runDone = false;
            var input = "";
            do
            {
                OutputMenu();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Your choice:");
                input = Console.ReadLine()?.Trim().ToUpper();
                var isInputValid = _menuShortCuts.Contains(input!);
                if (isInputValid)
                    
                {
                    var item = _menuItems.FirstOrDefault(t => t.ShortCut.ToUpper() == input);
                    input = item?.RunMethod==null ? input : item.RunMethod();
                    if (input != null && !input.All(char.IsLetter))
                    {
                        _value = double.Parse(input);
                        Console.Write("");
                    }
                }
                
                runDone = _menuSpecialShortCuts.Contains(input!);
                
                if (!runDone && !isInputValid)
                {
                    Console.WriteLine($"Unknown shortcut '{input}'!");
                }

            } while (!runDone);

            if (input == _menuItemReturn.ShortCut.ToUpper()) return "";
            
            return input!;
        }

        private void OutputMenu()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("====> " + _title + " <====");
            Console.ForegroundColor = ConsoleColor.Cyan;
            //Console.WriteLine("Current Value: " + value);
            // if (Board != null)
            // {
            //     Console.WriteLine($"You");
            //     BsConsoleUi.DrawBoard(Board);
            //     Console.WriteLine($"Enemy");
            //     BsConsoleUi.DrawBoard(Board2);
            // }
            Console.ForegroundColor = ConsoleColor.Red;
            //Console.WriteLine("-------------------");
            
            if (_title == "Player menu")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Choose a player");
                Console.ResetColor();
            }
            
            foreach (var t in _menuItems)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(t);
            }
            
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("-------------------");

            Console.ForegroundColor = ConsoleColor.Cyan;
            switch (_menuLevel)
            {
                case EMenuLevel.Root:
                    Console.WriteLine(_menuItemExit);
                    break;
                case EMenuLevel.GameSettings:
                    Console.WriteLine(_menuItemReturn);
                    Console.WriteLine(_menuItemExit);
                    break;
                case EMenuLevel.SaveOptions:
                    Console.WriteLine(_menuItemReturn);
                    Console.WriteLine(_menuItemExit);
                    break;  
                case EMenuLevel.Players:
                    Console.WriteLine(_menuItemReturn);
                    Console.WriteLine(_menuItemMain);
                    Console.WriteLine(_menuItemExit);
                    break;
                case EMenuLevel.Game:
                    Console.WriteLine(_menuItemReturn);
                    Console.WriteLine(_menuItemMain);
                    Console.WriteLine(_menuItemExit);
                    break;
            }
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("=====================");
        }
    }
}
