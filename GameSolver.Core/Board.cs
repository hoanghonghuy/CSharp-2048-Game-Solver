using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSolver.Core
{
    /// <summary>
    /// Đại diện cho bàn cờ game 2048.
    /// Chịu trách nhiệm lưu trữ trạng thái các ô, điểm số và kiểm tra điều kiện kết thúc game.
    /// </summary>
    public class Board
    {
        #region Properties

        /// <summary>
        /// Mảng 2 chiều 4x4 lưu giá trị các ô trên bàn cờ.
        /// Giá trị 0 đại diện cho ô trống.
        /// </summary>
        public int[,] Grid { get; private set; }

        /// <summary>
        /// Điểm số hiện tại của người chơi.
        /// </summary>
        public int Score { get; private set; }

        /// <summary>
        /// Cờ đánh dấu trạng thái kết thúc của game.
        /// True nếu không còn nước đi hợp lệ.
        /// </summary>
        public bool IsGameOver { get; private set; }

        private readonly Random _random = new Random();

        #endregion

        #region Constructor

        /// <summary>
        /// Khởi tạo một bàn cờ mới cho một game mới.
        /// </summary>
        public Board()
        {
            Grid = new int[4, 4];
            Score = 0;
            IsGameOver = false;

            // Bắt đầu game với 2 ô số ngẫu nhiên
            AddNewTile();
            AddNewTile();
        }

        /// <summary>
        /// Khởi tạo một bàn cờ từ một grid có sẵn.
        /// Dùng để tạo các bản sao bàn cờ khi AI "suy nghĩ".
        /// </summary>
        public Board(int[,] initialGrid, int currentScore)
        {
            Grid = initialGrid;
            Score = currentScore;
            
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Cộng điểm vào điểm số tổng.
        /// </summary>
        public void AddScore(int points)
        {
            if (points > 0)
            {
                Score += points;
            }
        }

        /// <summary>
        /// Thêm một ô số mới (2 hoặc 4) vào một vị trí trống ngẫu nhiên.
        /// </summary>
        public void AddNewTile()
        {
            List<Tuple<int, int>> emptyTiles = GetEmptyTiles();
            if (emptyTiles.Count > 0)
            {
                // Chọn một ô trống ngẫu nhiên
                Tuple<int, int> tile = emptyTiles[_random.Next(emptyTiles.Count)];

                // 90% cơ hội ra số 2, 10% ra số 4
                Grid[tile.Item1, tile.Item2] = _random.Next(10) < 9 ? 2 : 4;
            }

            // Sau khi thêm ô mới, kiểm tra xem game đã kết thúc chưa
            CheckGameOver();
        }

        /// <summary>
        /// Lấy danh sách tất cả các ô đang trống trên bàn cờ.
        /// </summary>
        /// <returns>Danh sách các tuple (hàng, cột) của các ô trống.</returns>
        public List<Tuple<int, int>> GetEmptyTiles()
        {
            var emptyTiles = new List<Tuple<int, int>>();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (Grid[i, j] == 0)
                    {
                        emptyTiles.Add(Tuple.Create(i, j));
                    }
                }
            }
            return emptyTiles;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Kiểm tra xem trò chơi đã kết thúc hay chưa.
        /// Game kết thúc khi không còn ô trống VÀ không còn nước đi hợp lệ nào.
        /// </summary>
        private void CheckGameOver()
        {
            if (GetEmptyTiles().Any())
            {
                IsGameOver = false;
                return;
            }

            // Kiểm tra các nước đi hợp lệ
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    int currentTile = Grid[i, j];
                    // Kiểm tra ô bên phải
                    if (j < 3 && currentTile == Grid[i, j + 1])
                    {
                        IsGameOver = false;
                        return;
                    }
                    // Kiểm tra ô bên dưới
                    if (i < 3 && currentTile == Grid[i + 1, j])
                    {
                        IsGameOver = false;
                        return;
                    }
                }
            }

            // Nếu không còn ô trống và không có nước đi nào, game over
            IsGameOver = true;
        }

        #endregion
    }
}