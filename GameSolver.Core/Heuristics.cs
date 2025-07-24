using System;
using System.Linq;

namespace GameSolver.Core
{
    /// <summary>
    /// Chứa các hàm để đánh giá và chấm điểm một trạng thái bàn cờ.
    /// </summary>
    public static class Heuristics
    {
        // Trọng số cho các tiêu chí đánh giá. Đại ca có thể tinh chỉnh các con số này.
        private const double MonotonicityWeight = 1.0;
        private const double SmoothnessWeight = 0.1;
        private const double EmptyTilesWeight = 2.7;
        private const double MaxValueWeight = 1.0;

        public static double Evaluate(Board board)
        {
            int[,] grid = board.Grid;

            double monotonicity = EvaluateMonotonicity(grid);
            double smoothness = EvaluateSmoothness(grid);
            int emptyTiles = board.GetEmptyTiles().Count;
            int maxValue = GetMaxValue(grid);

            return (monotonicity * MonotonicityWeight)
                   + (smoothness * SmoothnessWeight)
                   + (Math.Log(emptyTiles) * EmptyTilesWeight) // Dùng Log để điểm thưởng giảm dần
                   + (maxValue * MaxValueWeight);
        }

        private static double EvaluateMonotonicity(int[,] grid)
        {
            double[] totals = { 0, 0, 0, 0 };

            // Đánh giá theo hàng (trái->phải và phải->trái)
            for (int i = 0; i < 4; i++)
            {
                int current = 0;
                int next = current + 1;
                while (next < 4)
                {
                    while (next < 4 && grid[i, next] == 0) next++;
                    if (next >= 4) next--;

                    double currentValue = grid[i, current] == 0 ? 0 : Math.Log(grid[i, current]) / Math.Log(2);
                    double nextValue = grid[i, next] == 0 ? 0 : Math.Log(grid[i, next]) / Math.Log(2);

                    if (currentValue > nextValue)
                    {
                        totals[0] += nextValue - currentValue;
                    }
                    else if (nextValue > currentValue)
                    {
                        totals[1] += currentValue - nextValue;
                    }
                    current = next;
                    next++;
                }
            }

            // Đánh giá theo cột (trên->dưới và dưới->trên)
            for (int j = 0; j < 4; j++)
            {
                int current = 0;
                int next = current + 1;
                while (next < 4)
                {
                    while (next < 4 && grid[next, j] == 0) next++;
                    if (next >= 4) next--;

                    double currentValue = grid[current, j] == 0 ? 0 : Math.Log(grid[current, j]) / Math.Log(2);
                    double nextValue = grid[next, j] == 0 ? 0 : Math.Log(grid[next, j]) / Math.Log(2);

                    if (currentValue > nextValue)
                    {
                        totals[2] += nextValue - currentValue;
                    }
                    else if (nextValue > currentValue)
                    {
                        totals[3] += currentValue - nextValue;
                    }
                    current = next;
                    next++;
                }
            }

            return Math.Max(totals[0], totals[1]) + Math.Max(totals[2], totals[3]);
        }

        private static double EvaluateSmoothness(int[,] grid)
        {
            double smoothness = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (grid[i, j] != 0)
                    {
                        double value = Math.Log(grid[i, j]) / Math.Log(2);
                        // Kiểm tra ô bên phải
                        if (j < 3 && grid[i, j + 1] != 0)
                        {
                            double targetValue = Math.Log(grid[i, j + 1]) / Math.Log(2);
                            smoothness -= Math.Abs(value - targetValue);
                        }
                        // Kiểm tra ô bên dưới
                        if (i < 3 && grid[i + 1, j] != 0)
                        {
                            double targetValue = Math.Log(grid[i + 1, j]) / Math.Log(2);
                            smoothness -= Math.Abs(value - targetValue);
                        }
                    }
                }
            }
            return smoothness;
        }

        private static int GetMaxValue(int[,] grid)
        {
            return grid.Cast<int>().Max();
        }
    }
}