using GameSolver.Core;
using System;
using System.Linq;
using System.Threading;

namespace GameSolver.ConsoleApp
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("--- Welcome to 2048 Game ---");
                Console.WriteLine("Please choose a game mode:");
                Console.WriteLine("1. Manual Play (You play with W, A, S, D)");
                Console.WriteLine("2. AI Player (Watch the AI play)");
                Console.WriteLine("3. AI Assistant (Get best move for your game)"); // Lựa chọn mới
                Console.WriteLine("-----------------------------");
                Console.Write("Enter your choice (1, 2, or 3), or Q to quit: ");

                string? choice = Console.ReadLine();

                switch (choice?.ToUpper())
                {
                    case "1":
                        StartHumanPlayerMode();
                        break;
                    case "2":
                        StartAiPlayerMode();
                        break;
                    case "3":
                        StartAssistantMode();
                        break;
                    case "Q":
                        return; // Thoát chương trình
                    default:
                        Console.WriteLine("Invalid choice. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        /// <summary>
        /// Chế độ trợ lý AI
        /// </summary>
        public static void StartAssistantMode()
        {
            var solver = new Solver();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("--- AI Assistant Mode ---");
                Console.WriteLine("Enter the numbers on your 4x4 grid. Use 0 for empty tiles.");

                var grid = new int[4, 4];
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        Console.Write($"Enter tile at [Row {i + 1}, Col {j + 1}]: ");
                        while (!int.TryParse(Console.ReadLine(), out grid[i, j]))
                        {
                            Console.WriteLine("Invalid input. Please enter a number.");
                            Console.Write($"Enter tile at [Row {i + 1}, Col {j + 1}]: ");
                        }
                    }
                }

                // Tạo một bàn cờ tạm thời từ input của người dùng
                var currentBoard = new Board(grid, 0); // Score không quan trọng ở đây

                Console.WriteLine("\nAnalyzing your board...");
                PrintBoard(currentBoard, "Your Board");

                Direction? bestMove = solver.FindBestMove(currentBoard);

                Console.WriteLine("\n--- AI SUGGESTION ---");
                if (bestMove.HasValue)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"      BEST MOVE: {bestMove.Value.ToString().ToUpper()}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("      NO VALID MOVES FOUND. GAME OVER?");
                    Console.ResetColor();
                }
                Console.WriteLine("---------------------\n");

                Console.Write("Do you want to get another suggestion? (Y/N): ");
                if (Console.ReadLine()?.ToUpper() != "Y")
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Chế độ cho người chơi tự điều khiển.
        /// </summary>
        public static void StartHumanPlayerMode()
        {
            var board = new Board();
            var simulator = new MoveSimulator();

            while (true)
            {
                Console.Clear();
                PrintBoard(board, "Manual Player");

                if (board.IsGameOver)
                {
                    // Vòng lặp sẽ thoát ở lần kiểm tra tiếp theo nhưng ta nên cập nhật
                    // trạng thái IsGameOver một cách tường minh hơn.
                    // Trong trường hợp này, việc kiểm tra trước khi đọc phím là đủ.
                }

                Console.WriteLine("Use W, A, S, D to move. Press Q to quit.");

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Q) break;

                Direction? moveDirection = null;
                switch (keyInfo.Key)
                {
                    case ConsoleKey.W: moveDirection = Direction.Up; break;
                    case ConsoleKey.S: moveDirection = Direction.Down; break;
                    case ConsoleKey.A: moveDirection = Direction.Left; break;
                    case ConsoleKey.D: moveDirection = Direction.Right; break;
                }

                if (moveDirection.HasValue)
                {
                    var result = simulator.SimulateMove(board, moveDirection.Value);
                    if (result.Item3) // boardChanged
                    {
                        board = result.Item1;
                        board.AddScore(result.Item2);
                        board.AddNewTile();
                    }
                }

                // Kiểm tra lại trạng thái thua sau mỗi nước đi
                if (IsGameEffectivelyOver(board)) break;
            }

            Console.Clear();
            PrintBoard(board, "Manual Player");
            Console.WriteLine("--- GAME OVER ---");
            Console.WriteLine($"Final Score: {board.Score}");
            Console.WriteLine($"Highest Tile: {GetHighestTile(board)}");
        }

        /// <summary>
        /// Chế độ cho AI tự động chơi.
        /// </summary>
        public static void StartAiPlayerMode()
        {
            var board = new Board();
            var solver = new Solver();
            var simulator = new MoveSimulator();
            int moveCount = 0;

            while (true)
            {
                Console.Clear();
                PrintBoard(board, "AI Player");
                moveCount++;
                Console.WriteLine($"Move: {moveCount}");

                Direction? bestMove = solver.FindBestMove(board);
                if (bestMove == null)
                {
                    break; // AI không tìm thấy nước đi -> Game Over
                }

                Console.WriteLine($"AI decided to move: {bestMove.Value}");

                var result = simulator.SimulateMove(board, bestMove.Value);
                board = result.Item1;
                board.AddScore(result.Item2);
                board.AddNewTile();

                Thread.Sleep(100);
            }

            Console.Clear();
            PrintBoard(board, "AI Player");
            Console.WriteLine("--- GAME OVER ---");
            Console.WriteLine($"Final Score: {board.Score}");
            Console.WriteLine($"Highest Tile: {GetHighestTile(board)}");
            Console.WriteLine($"Total Moves: {moveCount}");
        }

        private static bool IsGameEffectivelyOver(Board board)
        {
            var simulator = new MoveSimulator();
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (simulator.SimulateMove(board, direction).Item3) // Kiểm tra xem có nước đi nào thay đổi bàn cờ không
                {
                    return false; // Vẫn còn nước đi
                }
            }
            return true; // Hết nước đi
        }

        private static void PrintBoard(Board board, string mode)
        {
            Console.WriteLine($"2048 Game - {mode}");
            Console.WriteLine($"Score: {board.Score}");
            Console.WriteLine("-----------------------------");
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    int value = board.Grid[i, j];
                    string formattedValue = value == 0 ? "." : value.ToString();
                    Console.Write($"| {formattedValue,-4} ");
                }
                Console.WriteLine("|");
                Console.WriteLine("-----------------------------");
            }
        }

        private static int GetHighestTile(Board board)
        {
            return board.Grid.Cast<int>().Max();
        }
    }
}