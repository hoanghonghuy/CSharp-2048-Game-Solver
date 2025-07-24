using System;
using System.Linq;

namespace GameSolver.Core
{
    /// <summary>
    /// Chứa thuật toán Expectimax để tìm nước đi tốt nhất.
    /// </summary>
    public class Solver
    {
        private readonly MoveSimulator _simulator;
        private const int SearchDepth = 4; // Độ sâu tìm kiếm, có thể tăng/giảm để cân bằng tốc độ và độ chính xác

        public Solver()
        {
            _simulator = new MoveSimulator();
        }

        /// <summary>
        /// Tìm và trả về hướng đi tốt nhất cho bàn cờ hiện tại.
        /// </summary>
        public Direction? FindBestMove(Board board)
        {
            Direction? bestDirection = null;
            double bestScore = double.MinValue;

            // Thử từng hướng đi và xem hướng nào cho điểm đánh giá cao nhất
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                // Mô phỏng nước đi
                var result = _simulator.SimulateMove(board, direction);
                if (!result.Item3) // Bỏ qua nếu nước đi không làm thay đổi bàn cờ
                {
                    continue;
                }

                // Gọi thuật toán tìm kiếm để đánh giá bàn cờ mới
                double score = Search(result.Item1, SearchDepth - 1, false); // false: lượt của máy (thêm ô ngẫu nhiên)

                if (score > bestScore)
                {
                    bestScore = score;
                    bestDirection = direction;
                }
            }

            return bestDirection;
        }

        /// <summary>
        /// Thuật toán Expectimax đệ quy.
        /// </summary>
        /// <param name="board">Bàn cờ để đánh giá.</param>
        /// <param name="depth">Độ sâu còn lại.</param>
        /// <param name="isPlayerTurn">True nếu là lượt của người chơi, False nếu là lượt của máy.</param>
        /// <returns>Điểm đánh giá cho bàn cờ.</returns>
        private double Search(Board board, int depth, bool isPlayerTurn)
        {
            // Nếu hết độ sâu hoặc game over, trả về điểm heuristic của bàn cờ hiện tại
            if (depth == 0 || board.IsGameOver)
            {
                return Heuristics.Evaluate(board);
            }

            if (isPlayerTurn) // Lượt người chơi: Chọn nước đi có điểm cao nhất (MAX)
            {
                double bestScore = double.MinValue;
                foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                {
                    var result = _simulator.SimulateMove(board, direction);
                    if (!result.Item3) continue;

                    bestScore = Math.Max(bestScore, Search(result.Item1, depth - 1, false));
                }
                return bestScore;
            }
            else // Lượt của máy: Tính điểm trung bình có trọng số của các khả năng (EXPECTI)
            {
                var emptyTiles = board.GetEmptyTiles();
                if (!emptyTiles.Any())
                {
                    return Heuristics.Evaluate(board);
                }

                double totalScore = 0;

                // Giả định có 2 khả năng cho mỗi ô trống: ra số 2 (90%) và ra số 4 (10%)
                foreach (var tile in emptyTiles)
                {
                    // Thử thêm số 2
                    var gridWith2 = (int[,])board.Grid.Clone();
                    gridWith2[tile.Item1, tile.Item2] = 2;
                    var boardWith2 = new Board(gridWith2, board.Score);
                    totalScore += Search(boardWith2, depth - 1, true) * 0.9;

                    // Thử thêm số 4
                    var gridWith4 = (int[,])board.Grid.Clone();
                    gridWith4[tile.Item1, tile.Item2] = 4;
                    var boardWith4 = new Board(gridWith4, board.Score);
                    totalScore += Search(boardWith4, depth - 1, true) * 0.1;
                }

                // Trả về điểm kỳ vọng (trung bình)
                return totalScore / emptyTiles.Count;
            }
        }
    }
}