using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DAL;
using Domain;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace BattleShipBrain
{
    public class BsBrain
    {
        private int _currentPlayerNo;

        private List<ShipBrain> _ships = new();

        private readonly GameBoardBrain[] _gameBoards = new GameBoardBrain[2];

        private GameConfigBrain _configBrain;
        public int ShipCount { get; set; } = default!;

        private List<ShipConfigBrain> _shipConfigBrainPlayer0 = default!;
        private List<ShipConfigBrain> _shipConfigBrainPlayer1 = default!;

        public BsBrain(GameConfigBrain config)
        {
            _configBrain = config;
            SetShipListConfs(config);
            _gameBoards[0] = new GameBoardBrain();
            _gameBoards[1] = new GameBoardBrain();

            _gameBoards[0].BoardA = new BoardSquareState[config.BoardSizeX, config.BoardSizeY];
            //_gameBoards[0].BoardB = new BoardSquareState[config.BoardSizeX, config.BoardSizeY];
            
            _gameBoards[1].BoardA = new BoardSquareState[config.BoardSizeX, config.BoardSizeY];
            //_gameBoards[1].BoardB = new BoardSquareState[config.BoardSizeX, config.BoardSizeY];

            for (var x = 0; x < config.BoardSizeX; x++)
            {
                for (var y = 0; y < config.BoardSizeY; y++)
                {

                    _gameBoards[0].BoardA[x, y] = new BoardSquareState
                    {
                        IsShip = false,
                        IsBomb = false
                    };
                    _gameBoards[1].BoardA[x, y] = new BoardSquareState
                    {
                        IsShip = false,
                        IsBomb = false
                    };
                }
            }
        }

        private void InitializeBoard()
        {
            for (var x = 0; x < _configBrain.BoardSizeX; x++)
            {
                for (var y = 0; y < _configBrain.BoardSizeY; y++)
                {

                    _gameBoards[0].BoardA[x, y] = new BoardSquareState
                    {
                        IsShip = false,
                        IsBomb = false
                    };
                    _gameBoards[1].BoardA[x, y] = new BoardSquareState
                    {
                        IsShip = false,
                        IsBomb = false
                    };
                }
            }
        }

        public int ShipChange(int playerId, string ship)
        {
            var i = 0;
            var count = 0;
            var score = 0;
            List<ShipBrain> tempListPlayer0Ships = new List<ShipBrain>();
            List<ShipBrain> tempListPlayer1Ships = new List<ShipBrain>();
            foreach (var shipBrain in GetListOfShips())
            {
                if (count < Math.Ceiling(Convert.ToDouble(GetListOfShips().Count / 2)))
                {
                    tempListPlayer0Ships.Add(shipBrain);
                }
                else
                {
                    tempListPlayer1Ships.Add(shipBrain);
                }

                count++;
            }
            switch (playerId)
            {
                
                case 0:
                    foreach (var listOfShip in tempListPlayer1Ships)
                    {
                        if (listOfShip.Name.Equals(ship))
                        {
                            //Console.WriteLine(listOfShip.GetShipDamageCount(GetBoard(1, "A", null)));
                            if (listOfShip.GetShipDamageCount(GetBoard(1, "A", null)) == listOfShip.Height)
                            {
                                score = 1;
                                tempListPlayer1Ships.Remove(listOfShip);
                                break;
                            }
                        }
                        i++;
                    }
                    break;
                case 1:
                    foreach (var listOfShip in tempListPlayer0Ships)
                    {
                        if (listOfShip.Name.Equals(ship))
                        {
                            //Console.WriteLine(listOfShip.GetShipDamageCount(GetBoard(0, "A", null)));
                            if (listOfShip.GetShipDamageCount(GetBoard(0, "A", null)) == listOfShip.Height)
                            {
                                score = 1;
                                tempListPlayer0Ships.Remove(listOfShip);
                                break;
                            }
                        }
                        i++;
                    }
                    break;
            }

            var tempList = tempListPlayer0Ships.ToList();
            tempList.AddRange(tempListPlayer1Ships);
            _ships = tempList;
            return score;
        }

        private void SetShipListConfs(GameConfigBrain config)
        {
            _shipConfigBrainPlayer0 = DeepCopy.DeepClone(config).ShipConfigs;
            _shipConfigBrainPlayer1 = DeepCopy.DeepClone(config).ShipConfigs;
        }

        public void SetShipConf(int currentPlayerNo, List<ShipConfigBrain> list)
        {
            switch (currentPlayerNo)
            {
                case 0:
                    _shipConfigBrainPlayer0 = list;
                    break;
                case 1:
                    _shipConfigBrainPlayer1 = list;
                    break;
            }
            
        }

        public List<ShipConfigBrain> GetShipConfig(int playerId)
        {
            return playerId == 0 ? _shipConfigBrainPlayer0 : _shipConfigBrainPlayer1;
        }

        public List<List<ShipConfigBrain>> GetAllShipConfigs()
        {
            var list = new List<List<ShipConfigBrain>>
            {
                _shipConfigBrainPlayer0,
                _shipConfigBrainPlayer1
            };
            return list;
        }

        public int GetCurrentPlayer()
        {
            return _currentPlayerNo;
        }

        public GameConfigBrain GetConfigBrain()
        {
            return _configBrain;
        }

        public string SetBomb(int x, int y, int currentPlayerNo)
        {
            string shipName = "";
            if (currentPlayerNo == 0)
            {
                if (_gameBoards[1].BoardA[x, y].IsShip)
                {
                    var i = 0;
                    foreach (var brainShip in _ships)
                    {
                        if (i >= 5)
                        {
                            //var coordinatesOfValidShip = brainShip.Coordinates.Find(g => g.X == x && g.Y == y);
                            foreach (var coord in brainShip.Coordinates)
                            {
                                if (coord.X == x && coord.Y == y)
                                {
                                    shipName = brainShip.Name;
                                }
                            }
                        }

                        i++;
                    }
                    _gameBoards[1].BoardA[x, y].IsBomb = true;
                }
                else
                {
                    _gameBoards[1].BoardA[x, y].IsBomb = true;
                }
            }
            else
            {
                if (_gameBoards[0].BoardA[x, y].IsShip)
                {
                    var i = 0;
                    foreach (var brainShip in _ships)
                    {
                        if (i < 5)
                        {
                            //var coordinatesOfValidShip = brainShip.Coordinates.Find(g => g.X == x && g.Y == y);
                            foreach (var coord in brainShip.Coordinates)
                            {
                                if (coord.X == x && coord.Y == y)
                                {
                                    shipName = brainShip.Name;
                                }
                            }
                        }

                        i++;
                    }
                    _gameBoards[0].BoardA[x, y].IsBomb = true;
                }
                _gameBoards[0].BoardA[x, y].IsBomb = true;
            }

            return shipName;
        }

        public void SetPlayerNo(int playerNo)
        {
            _currentPlayerNo = playerNo;
        }

        public int GetPlayerNo()
        {
            return _currentPlayerNo;
        }

        public void SetGameConf(GameConfigBrain gameConfig)
        {
            _configBrain = gameConfig;
        }
        
           
        /*
        private bool TouchRule(ShipBrain ship)
        {
            var allowedCells = ship.Coordinates;
            var board = _gameBoards[_currentPlayerNo].BoardA;
            return false;
        }
        */
        
        private bool CheckShipCorners(ShipBrain ship, int currentPlayerNo)
        {
            
            return false;
        }        
        
        private bool CheckShipEdges(ShipBrain ship, int currentPlayerNo)
        {
            
            return false;
        }

        private List<Coordinate> GetExtendedShipCoordinates(ShipBrain shipBrain)
        {
            var extendedListOfCoordinates = new List<Coordinate>();
            switch (shipBrain.Direction)
            {
                case "Vertical":
                    var i = 0;
                    for (var x = shipBrain.Coordinates[0].X - 1; x < shipBrain.Coordinates[0].X + 2; x++)
                    {
                        for (var num = shipBrain.Coordinates[0].Y - 1; num < shipBrain.Coordinates[0].Y + shipBrain.Height + 1; num++)
                        {
                            if (num < 0 || x < 0) continue;
                            if (num < GetConfigBrain().BoardSizeY && x < GetConfigBrain().BoardSizeX)
                            {
                                switch (_configBrain.EShipTouchRule)
                                {
                                    case EShipTouchRule.NoTouch:
                                        extendedListOfCoordinates.Add(new Coordinate(){X = x, Y = num});
                                        break;
                                    case EShipTouchRule.CornerTouch:
                                        if (x == shipBrain.Coordinates[0].X - 1 || x == shipBrain.Coordinates[0].X + 1)
                                        {
                                            if (num == shipBrain.Coordinates[0].Y - 1 || num == shipBrain.Coordinates[0].Y + shipBrain.Height)
                                            {
                                                continue;
                                            }
                                        }
                                        extendedListOfCoordinates.Add(new Coordinate(){X = x, Y = num});
                                        break;
                                    case EShipTouchRule.SideTouch:
                                        
                                        if (x == shipBrain.Coordinates[0].X - 1 || x == shipBrain.Coordinates[0].X + 1)
                                        {
                                            if (num == shipBrain.Coordinates[0].Y - 1 || num == shipBrain.Coordinates[0].Y + shipBrain.Height)
                                            {
                                                extendedListOfCoordinates.Add(new Coordinate(){X = x, Y = num});
                                            }
                                        }

                                        if (shipBrain.Coordinates[0].X - 1  < x && x < shipBrain.Coordinates[0].X + 1)
                                        {
                                            if (shipBrain.Coordinates[0].Y - 1 < num && num < shipBrain.Coordinates[0].Y + shipBrain.Height)
                                            {
                                                extendedListOfCoordinates.Add(new Coordinate(){X = x, Y = num});
                                            }
                                        }
                                        
                                        break;
                                }
                            }

                        }   
                    }

                    break;
                case "Horizontal":
                    for (var y = shipBrain.Coordinates[0].Y - 1; y < shipBrain.Coordinates[0].Y + 2; y++)
                    {
                        for (var num = shipBrain.Coordinates[0].X - 1; num < shipBrain.Coordinates[0].X + shipBrain.Height + 1; num++)
                        {
                            if (num < 0 || y < 0) continue;
                            if (num < GetConfigBrain().BoardSizeX && y < GetConfigBrain().BoardSizeY)
                            {
                                switch (_configBrain.EShipTouchRule)
                                {
                                    case EShipTouchRule.NoTouch:
                                        extendedListOfCoordinates.Add(new Coordinate(){X = num, Y = y});
                                        break;
                                    case EShipTouchRule.CornerTouch:
                                        if (y == shipBrain.Coordinates[0].X - 1 || y == shipBrain.Coordinates[0].X + 1)
                                        {
                                            if (num == shipBrain.Coordinates[0].Y - 1 || num == shipBrain.Coordinates[0].Y + shipBrain.Height)
                                            {
                                                continue;
                                            }
                                        }
                                        extendedListOfCoordinates.Add(new Coordinate(){X = num, Y = y});
                                        break;
                                    case EShipTouchRule.SideTouch:
                                        if (y == shipBrain.Coordinates[0].X - 1 || y == shipBrain.Coordinates[0].X + 1)
                                        {
                                            if (num == shipBrain.Coordinates[0].Y - 1 || num == shipBrain.Coordinates[0].Y + shipBrain.Height)
                                            {
                                                extendedListOfCoordinates.Add(new Coordinate(){X = num, Y = y});
                                            }
                                        }

                                        if (shipBrain.Coordinates[0].X - 1  < y && y < shipBrain.Coordinates[0].X + 1)
                                        {
                                            if (shipBrain.Coordinates[0].Y - 1 < num && num < shipBrain.Coordinates[0].Y + shipBrain.Height)
                                            {
                                                extendedListOfCoordinates.Add(new Coordinate(){X = num, Y = y});
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }

                    break;
            }
            
            return extendedListOfCoordinates;
        }


        

        public bool SetShip(ShipBrain ship)
        {
            var shipConf = GetShipConfig(_currentPlayerNo).Find(x => x.Name == ship.Name);
            //var tempBoard = new GameBoardBrain().BoardA;
            var isValid = true;
            if (shipConf!.Quantity > 0)
            {
                var neededBoard = GetBoard(_currentPlayerNo, "A", null);
                foreach (var coordinate in ship.Coordinates)
                {
                    if (neededBoard[coordinate.X, coordinate.Y].IsTaken || neededBoard[coordinate.X, coordinate.Y].IsShip)
                    {
                        isValid = false;
                    }
                }

                if (isValid)
                {
                    foreach (var coordinate in ship.GetCoordinates())
                    {
                        foreach (var coord in GetExtendedShipCoordinates(ship))
                        {
                            if (_currentPlayerNo == 0)
                            {
                                if (coord.X == coordinate.X && coord.Y == coordinate.Y)
                                {
                                    _gameBoards[0].BoardA[coord.X, coord.Y].IsTaken = false;
                                    _gameBoards[0].BoardA[coord.X, coord.Y].IsShip = true;
                                }
                                else
                                {
                                    _gameBoards[0].BoardA[coord.X, coord.Y].IsTaken = _gameBoards[0].BoardA[coord.X, coord.Y].IsShip != true;

                                }
                            }
                            else
                            {
                                if (coord.X == coordinate.X && coord.Y == coordinate.Y)
                                {
                                    _gameBoards[1].BoardA[coordinate.X, coordinate.Y].IsTaken = false;
                                    _gameBoards[1].BoardA[coordinate.X, coordinate.Y].IsShip = true;
                                }
                                else
                                {
                                    _gameBoards[1].BoardA[coord.X, coord.Y].IsTaken = _gameBoards[1].BoardA[coord.X, coord.Y].IsShip != true;
                                }
                            }
                        }
                    }
                    shipConf.Quantity--;
                    _ships.Add(ship);
                }
            }

            return isValid;
        }
        

        public List<ShipBrain> GetListOfShips()
        {
            return _ships;
        }        
        
        public void SetShips(ShipBrain ship)
        {
            _ships.Add(ship);
        }

        public BoardSquareState[,] GetBoard(int playerNo, string boardName, bool? isHidden)
        {
            switch (playerNo)
            {
                case 0 when boardName.Equals("A"):
                    return isHidden != null ? CopyOfBoard(_gameBoards[0].BoardA, isHidden.Value) : CopyOfBoard(_gameBoards[0].BoardA, null);
                case 1 when boardName.Equals("A"):
                    return isHidden != null ? CopyOfBoard(_gameBoards[1].BoardA, isHidden.Value) : CopyOfBoard(_gameBoards[1].BoardA, null);
                default:
                    return null!;
            }
        }

        private BoardSquareState[,] CopyOfBoard(BoardSquareState[,] board, bool? isHidden)
        {
            var res = new BoardSquareState[board.GetLength(0), board.GetLength(1)];
            
            for (var x = 0; x < board.GetLength(0); x++)
            {
                for (var y = 0; y < board.GetLength(1); y++)
                {
                    if (isHidden != null)
                    {
                        if (isHidden!.Value)
                        {
                            res[x, y] = board[x, y];
                            res[x, y].Hidde = true;
                        }
                        else
                        {
                            res[x, y] = board[x, y];
                        }
                    }
                    else
                    {
                        res[x, y] = board[x, y];
                    }
                }
            }

            return res;
        }

        /*
        public string GetConfigJson()
        {
            var dto = new SaveGameDTO.GameConfigDTO();
            dto.BoardSizeX = _configBrain.BoardSizeX;
            dto.BoardSizeY = _configBrain.BoardSizeY;
            var jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            var jsonStr = JsonSerializer.Serialize(dto, jsonOptions);
            return jsonStr;
        }
        */

        /*
        public string GetBrainJson()
        {
            var dto2 = new SaveGameDTO.GameBoardDTO();
            List<List<BoardSquareState>> boardsA = new List<List<BoardSquareState>>();

            List<BoardSquareState> boardSquareStatesA1 = new List<BoardSquareState>();
            List<BoardSquareState> boardSquareStatesA2 = new List<BoardSquareState>();
            
            
            foreach (var board in _gameBoards[0].BoardA)
            {
                boardSquareStatesA1.Add(board);
            }
            
            foreach (var board in _gameBoards[1].BoardA)
            {
                boardSquareStatesA2.Add(board);
            }
            
            
            
            boardsA.Add(boardSquareStatesA1);
            boardsA.Add(boardSquareStatesA2);

            dto2.BoardA = boardsA;

            dto2.Ships = _ships;


            var jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            
            var jsonStr = JsonSerializer.Serialize(dto2, jsonOptions);
            return jsonStr;
        }
        */
        
        public List<List<BoardSquareState>> GetBrainBoardAsArrays()
        {
            List<List<BoardSquareState>> boardsA = new List<List<BoardSquareState>>();

            List<BoardSquareState> boardSquareStatesA1 = new List<BoardSquareState>();
            List<BoardSquareState> boardSquareStatesA2 = new List<BoardSquareState>();

            foreach (var board in _gameBoards[0].BoardA)
            {
                boardSquareStatesA1.Add(board);
            }
            
            foreach (var board in _gameBoards[1].BoardA)
            {
                boardSquareStatesA2.Add(board);
            }
            
            
            boardsA.Add(boardSquareStatesA1);
            boardsA.Add(boardSquareStatesA2);
            return boardsA;
        }
        public void WebRestoreBrainFromJson(GameConfigBrain config, SaveGameDTO.GameBoardDTO conf)
        {
            DataRestore(config, conf);
            
        }

        public void DataRestore(GameConfigBrain config, SaveGameDTO.GameBoardDTO conf)
        {
            _configBrain = config;
            _gameBoards[0] = new GameBoardBrain();
            _gameBoards[1] = new GameBoardBrain();

            _gameBoards[0].BoardA = new BoardSquareState[config.BoardSizeX, config.BoardSizeY];

            _gameBoards[1].BoardA = new BoardSquareState[config.BoardSizeX, config.BoardSizeY];
            

            var i = 0;
            if (conf.Ships != null) _ships = conf.Ships;
            foreach (var board in conf!.BoardA)
            {
                int count = 0;
                for (int x = 0; x < config.BoardSizeX; x++)
                {
                    for (int y = 0; y < config.BoardSizeY; y++)
                    {
                        switch (i)
                        {
                            case 0:
                                _gameBoards[0].BoardA[x, y] = new BoardSquareState
                                {
                                    IsShip = board[count].IsShip,
                                    IsBomb = board[count].IsBomb,
                                    IsTaken = board[count].IsTaken
                                };
                                count++;
                                break;
                            case 1:
                                _gameBoards[1].BoardA[x, y] = new BoardSquareState
                                {
                                    IsShip = board[count].IsShip,
                                    IsBomb = board[count].IsBomb,
                                    IsTaken = board[count].IsTaken
                                };
                                count++;
                                break;
                        }
                    }
                }
                i++;
            }
        }

        
        public override string ToString()
        {
            return "";
        }
    }
}