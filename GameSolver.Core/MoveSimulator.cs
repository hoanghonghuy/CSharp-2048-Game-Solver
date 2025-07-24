using System;

namespace GameSolver.Core
{
    /// <summary>
    /// Chịu trách nhiệm mô phỏng các nước đi trên bàn cờ.
    /// Lớp này không thay đổi trạng thái của bàn cờ gốc mà trả về một bản sao mới.
    /// </summary>
    public class MoveSimulator
    {
        /// <summary>
        /// Mô phỏng một nước đi và trả về trạng thái bàn cờ mới.
        /// </summary>
        /// <param name="board">Bàn cờ hiện tại.</param>
        /// <param name="direction">Hướng di chuyển.</param>
        /// <returns>
        /// Một tuple chứa:
        /// - Board: Bàn cờ mới sau nước đi.
        /// - int: Điểm số ghi được từ nước đi này.
        /// - bool: True nếu bàn cờ có thay đổi, False nếu không.
        /// </returns>
        public Tuple<Board, int, bool> SimulateMove(Board board, Direction direction)
        {
            int[,] originalGrid = (int[,])board.Grid.Clone();
            int[,] newGrid = (int[,])board.Grid.Clone();
            int moveScore = 0;

            // Xoay bàn cờ để luôn xử lý theo hướng TRÁI
            switch (direction)
            {
                case Direction.Up:
                    newGrid = RotateGridCounterClockwise(newGrid);
                    break;
                case Direction.Right:
                    newGrid = RotateGrid180(newGrid);
                    break;
                case Direction.Down:
                    newGrid = RotateGridClockwise(newGrid);
                    break;
            }

            // Thực hiện di chuyển và hợp nhất
            for (int i = 0; i < 4; i++)
            {
                int[] row = new int[4];
                for (int j = 0; j < 4; j++)
                {
                    row[j] = newGrid[i, j];
                }

                int[] mergedRow = MoveAndMergeRow(row, out int score);
                moveScore += score;

                for (int j = 0; j < 4; j++)
                {
                    newGrid[i, j] = mergedRow[j];
                }
            }

            // Xoay bàn cờ về vị trí ban đầu
            switch (direction)
            {
                case Direction.Up:
                    newGrid = RotateGridClockwise(newGrid);
                    break;
                case Direction.Right:
                    newGrid = RotateGrid180(newGrid);
                    break;
                case Direction.Down:
                    newGrid = RotateGridCounterClockwise(newGrid);
                    break;
            }

            bool boardChanged = !AreGridsEqual(originalGrid, newGrid);

            // Dùng constructor mới để tạo bàn cờ mới
            var newBoard = new Board(newGrid, board.Score);

            // Trả về bàn cờ mới, điểm từ nước đi, và cờ báo hiệu sự thay đổi.
            return Tuple.Create(newBoard, moveScore, boardChanged);
        }

        // Di chuyển và hợp nhất trên một hàng (luôn theo hướng trái)
        private int[] MoveAndMergeRow(int[] row, out int score)
        {
            score = 0;
            // 1. Dồn các số về bên trái (loại bỏ các số 0)
            int[] newRow = new int[4];
            int currentIndex = 0;
            for (int i = 0; i < 4; i++)
            {
                if (row[i] != 0)
                {
                    newRow[currentIndex++] = row[i];
                }
            }

            // 2. Hợp nhất các ô giống nhau
            for (int i = 0; i < 3; i++)
            {
                if (newRow[i] != 0 && newRow[i] == newRow[i + 1])
                {
                    newRow[i] *= 2;
                    score += newRow[i];
                    newRow[i + 1] = 0;
                }
            }

            // 3. Dồn lại một lần nữa sau khi hợp nhất
            int[] finalRow = new int[4];
            currentIndex = 0;
            for (int i = 0; i < 4; i++)
            {
                if (newRow[i] != 0)
                {
                    finalRow[currentIndex++] = newRow[i];
                }
            }

            return finalRow;
        }

        // Hàm kiểm tra xem hai bàn cờ có giống nhau không
        private bool AreGridsEqual(int[,] grid1, int[,] grid2)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (grid1[i, j] != grid2[i, j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        #region Grid Rotation Helpers

        private int[,] RotateGridClockwise(int[,] grid)
        {
            int[,] newGrid = new int[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    newGrid[i, j] = grid[3 - j, i];
                }
            }
            return newGrid;
        }

        private int[,] RotateGridCounterClockwise(int[,] grid)
        {
            int[,] newGrid = new int[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    newGrid[i, j] = grid[j, 3 - i];
                }
            }
            return newGrid;
        }

        private int[,] RotateGrid180(int[,] grid)
        {
            int[,] newGrid = new int[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    newGrid[i, j] = grid[3 - i, 3 - j];
                }
            }
            return newGrid;
        }

        #endregion
    }
}