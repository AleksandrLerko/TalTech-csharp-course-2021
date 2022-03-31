namespace BattleShipBrain
{
    public struct BoardSquareState
    {
        public bool IsShip { get; set; }
        public bool IsBomb { get; set; }
        
        public bool IsTaken { get; set; }
        public bool Hidde { get; set; }

        public override string ToString()
        {
            switch (IsShip, IsBomb)
            {
                case (false, false):
                    return " "; // empty
                case (false, true):
                    return "x"; // miss
                case (true, false):
                    
                    if (Hidde)
                    {
                        return " "; // display ship on enemy board
                    }
                    else
                    {
                        return "8"; // display ship on your board
                    }
                case (true, true):
                    return "*"; // sinked ship
            }
        }
    }
}