using System;
using System.Collections.Generic;
using MenuSystem;

namespace ConsoleApp
{
    public class Program
    {
        public static double _currentValue = 0.0;

        static void Main(string[] args)
        {
            Console.Clear();

            var mainMenu = new Menu("Calculator Main", EMenuLevel.Root);
            mainMenu.AddMenuItems(new List<MenuItem>()
            {
                new MenuItem("B", "Binary operations", SubmenuBinary),
                new MenuItem("U", "Unary operations", SubmenuUnary)
                
            });

            mainMenu.Run();
        }
        
        public static string SubmenuBinary()
        {
            var menu = new Menu("Binary", EMenuLevel.Game);
            menu.AddMenuItems(new List<MenuItem>()
            {
                new MenuItem("+", "+", Add),
                new MenuItem("-", "-", Subtraction),
                new MenuItem("/", "/", Division),
                new MenuItem("*", "*", Multiplication),
                new MenuItem("Pow", "pow", Power)
            });
            var res = menu.Run();
            return res;
        }

        
        public static string SubmenuUnary()
        {
            var menu = new Menu("Unary", EMenuLevel.Game);
            menu.AddMenuItems(new List<MenuItem>()
            {
                new MenuItem("Negate", "Negate", Negate),
                new MenuItem("Sqrt", "Sqrt", Sqrt),
                new MenuItem("Square", "Square", Square),
                new MenuItem("Abs", "Abs value", Abs)
            });
            var res = menu.Run();
            return res;
        }
        
        
        public static string Add()
        {
            Console.WriteLine("Current value: " + _currentValue);
            Console.WriteLine("plus");
            Console.Write("number: ");
            var n = Console.ReadLine()?.Trim();
            double.TryParse(n, out var converted);
            Console.WriteLine("Do you want to add to current value?");
            var answer = Console.ReadLine()?.Trim().ToUpper();
            
            switch (answer)
            {
                case "Y":
                    _currentValue += converted;
                    break;
                case "N":
                    Console.Write("Second value: ");
                    var y = Console.ReadLine()?.Trim();
                    _currentValue = converted + double.Parse(y);
                    break;
            }
            return _currentValue.ToString();
        }
        
        public static string Subtraction()
        {
            // CalculatorCurrentDisplay
            Console.WriteLine("Current value: " + _currentValue);
            Console.WriteLine("minus");
            Console.Write("number: ");
            var n = Console.ReadLine()?.Trim();
            double.TryParse(n, out var converted);
            Console.WriteLine("Do you want to substract to current value?");
            var answer = Console.ReadLine()?.Trim().ToUpper();
            
            switch (answer)
            {
                case "Y":
                    _currentValue -= converted;
                    break;
                case "N":
                    Console.Write("Second value: ");
                    var y = Console.ReadLine()?.Trim();
                    _currentValue = converted - double.Parse(y);
                    break;
            }
            
            return _currentValue.ToString();
        }
        
        public static string Multiplication()
        {
            // CalculatorCurrentDisplay
            Console.WriteLine("Current value: " + _currentValue);
            Console.WriteLine("multiply");
            Console.Write("number: ");
            var n = Console.ReadLine()?.Trim();
            double.TryParse(n, out var converted);
            Console.WriteLine("Do you want to multiply to current value?");
            var answer = Console.ReadLine()?.Trim().ToUpper();
            
            switch (answer)
            {
                case "Y":
                    _currentValue *= converted;
                    break;
                case "N":
                    Console.Write("Second value: ");
                    var y = Console.ReadLine()?.Trim();
                    _currentValue = converted * double.Parse(y);
                    break;
            }
            
            return _currentValue.ToString();
        }
        
        public static string Division()
        {
            // CalculatorCurrentDisplay
            Console.WriteLine("Current value: " + _currentValue);
            Console.WriteLine("divide");
            Console.Write("number: ");
            var n = Console.ReadLine()?.Trim();
            double.TryParse(n, out var converted);

            Console.WriteLine("Do you want to divide to current value?");
            var answer = Console.ReadLine()?.Trim().ToUpper();
            
            switch (answer)
            {
                case "Y":
                    _currentValue /= converted;
                    break;
                case "N":
                    Console.Write("Second value: ");
                    var y = Console.ReadLine()?.Trim();
                    _currentValue = converted / double.Parse(y);
                    break;
            }
            
            return _currentValue.ToString();
        }
        
        public static string Power()
        {
            // CalculatorCurrentDisplay
            Console.WriteLine("Current value: " + _currentValue);
            Console.WriteLine("power");
            Console.Write("number: ");
            var n = Console.ReadLine()?.Trim();
            double.TryParse(n, out var converted);
            
            Console.WriteLine("Do you want to substract to current value?");
            var answer = Console.ReadLine()?.Trim().ToUpper();
            
            switch (answer)
            {
                case "Y":
                    _currentValue = Math.Pow(_currentValue, converted);
                    break;
                case "N":
                    Console.Write("Second value: ");
                    var y = Console.ReadLine()?.Trim();
                    _currentValue = Math.Pow(converted, double.Parse(y));
                    break;
            }

            return _currentValue.ToString();
        }
        
        public static string Negate()
        {
            // CalculatorCurrentDisplay
            Console.WriteLine("Current value: " + _currentValue);
            Console.WriteLine("negate");
            Console.WriteLine("Do you want to negate current value?");

            var answer = Console.ReadLine()?.Trim().ToUpper();
            
            switch (answer)
            {
                case "Y":
                    _currentValue = -_currentValue;
                    break;
                case "N":
                    Console.Write("number: ");
                    var n = Console.ReadLine()?.Trim();
                    double.TryParse(n, out var converted);
                    _currentValue = -converted;
                    break;
            }

            return _currentValue.ToString();
        }
        
        public static string Sqrt()
        {
            // CalculatorCurrentDisplay
            Console.WriteLine("Current value: " + _currentValue);
            Console.WriteLine("sqrt");
            Console.WriteLine("Do you want to get sqrt of current value?");

            var answer = Console.ReadLine()?.Trim().ToUpper();
            
            switch (answer)
            {
                case "Y":
                    _currentValue = Math.Sqrt(_currentValue);
                    break;
                case "N":
                    Console.Write("number: ");
                    var n = Console.ReadLine()?.Trim();
                    double.TryParse(n, out var converted);
                    _currentValue = Math.Sqrt(converted);
                    break;
            }
            
            return _currentValue.ToString();
        }
        
        public static string Square()
        {
            // CalculatorCurrentDisplay
            Console.WriteLine("Current value: " + _currentValue);
            Console.WriteLine("Square");
            Console.WriteLine("Do you want to get sqrt of current value?");

            var answer = Console.ReadLine()?.Trim().ToUpper();
            
            switch (answer)
            {
                case "Y":
                    _currentValue = _currentValue * _currentValue;
                    break;
                case "N":
                    Console.Write("number: ");
                    var n = Console.ReadLine()?.Trim();
                    double.TryParse(n, out var converted);
                    _currentValue = converted * converted;
                    break;
            }
            
            return _currentValue.ToString();
        }
        
        public static string Abs()
        {
            // CalculatorCurrentDisplay
            Console.WriteLine("Current value: " + _currentValue);
            Console.WriteLine("Abs");
            Console.WriteLine("Do you want to get sqrt of current value?");

            var answer = Console.ReadLine()?.Trim().ToUpper();
            
            switch (answer)
            {
                case "Y":
                    _currentValue = Math.Abs(_currentValue);
                    break;
                case "N":
                    Console.Write("number: ");
                    var n = Console.ReadLine()?.Trim();
                    double.TryParse(n, out var converted);
                    _currentValue = Math.Abs(converted);
                    break;
            }
            
            return _currentValue.ToString();
        }
        

    }
}