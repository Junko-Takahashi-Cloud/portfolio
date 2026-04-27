using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;


namespace 数独
{
    public partial class Form1 : Form
    {
        private int[,] grid = new int[9, 9];
        private int[,] initialGrid = new int[9, 9];
        private int cellsToRemove = 40;
        private Dictionary<(int row, int col), List<int>> gridCandidates = new Dictionary<(int row, int col), List<int>>();

        public Form1()
        {
            InitializeComponent();
            InitializeGrid();
            GenerateSudoku(40);
        }

        private void InitializeGrid()
        {
            dataGridView1.RowCount = 9;
            dataGridView1.ColumnCount = 9;
            dataGridView1.ColumnHeadersVisible = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Font = new Font("Arial", 12);

            for (int i = 0; i < 9; i++)
            {
                dataGridView1.Columns[i].Width = 40;
                dataGridView1.Rows[i].Height = 40;
            }

            dataGridView1.CellPainting += DataGridView1_CellPainting;
        }

        private void GenerateSudoku(int cellsToRemove)
        {
            this.cellsToRemove = cellsToRemove;
            FillGrid(grid);
            Array.Copy(grid, initialGrid, grid.Length);
            CreatePuzzle(grid);

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    dataGridView1.Rows[i].Cells[j].Value = grid[i, j] == 0 ? "" : grid[i, j].ToString();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GenerateSudoku(40);
            MessageBox.Show("リセットされました！", "通知");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            GenerateSudoku(50);
            MessageBox.Show("難易度が高に設定されました！", "通知");
        }

        private bool FillGrid(int[,] grid)
        {
            Random random = new Random();

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (grid[row, col] == 0)
                    {
                        List<int> numbers = Enumerable.Range(1, 9).OrderBy(x => random.Next()).ToList();
                        foreach (int num in numbers)
                        {
                            if (IsSafeToPlace(grid, row, col, num))
                            {
                                grid[row, col] = num;
                                if (FillGrid(grid))
                                {
                                    return true;
                                }
                                grid[row, col] = 0;
                            }
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        private bool IsSafeToPlace(int[,] grid, int row, int col, int num)
        {
            for (int i = 0; i < 9; i++)
            {
                if (grid[row, i] == num || grid[i, col] == num)
                {
                    return false;
                }
            }

            int startRow = row - row % 3;
            int startCol = col - col % 3;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (grid[startRow + i, startCol + j] == num)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void CreatePuzzle(int[,] grid)
        {
            Random random = new Random();
            int cellsRemoved = 0;

            while (cellsRemoved < cellsToRemove)
            {
                int row = random.Next(0, 9);
                int col = random.Next(0, 9);

                if (grid[row, col] != 0)
                {
                    grid[row, col] = 0;
                    cellsRemoved++;
                }
            }
        }

        private void HighlightErrors(int[,] userGrid)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (userGrid[i, j] != 0 && userGrid[i, j] != initialGrid[i, j])
                    {
                        dataGridView1.Rows[i].Cells[j].Style.BackColor = Color.Pink;
                    }
                    else
                    {
                        dataGridView1.Rows[i].Cells[j].Style.BackColor = Color.White;
                    }
                }
            }
        }

        private void btnCheckSolution_Click(object sender, EventArgs e)
        {
            int[,] userGrid = new int[9, 9];
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    string cellValue = dataGridView1.Rows[i].Cells[j].Value?.ToString();
                    userGrid[i, j] = string.IsNullOrEmpty(cellValue) ? 0 : int.Parse(cellValue);
                }
            }

            HighlightErrors(userGrid);

            if (CheckSolution(userGrid))
            {
                MessageBox.Show("おめでとうございます！正解です！", "結果");
            }
            else
            {
                MessageBox.Show("間違いがあります。再確認してください。", "結果");
            }
        }

        private bool CheckSolution(int[,] userGrid)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (userGrid[i, j] != initialGrid[i, j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        // 保存と復元を統一
        private void SaveSudokuData(int[,] board, Dictionary<(int row, int col), List<int>> candidates, string filePath)
        {
            var data = new { Board = board, Candidates = candidates };
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        private (int[,] board, Dictionary<(int row, int col), List<int>> candidates) LoadSudokuData(string filePath)
        {
            string json = File.ReadAllText(filePath);
            var data = JsonConvert.DeserializeObject<dynamic>(json);
            return (data.Board, data.Candidates);
        }

        // セルの状態を表すクラス
        public class CellState
        {
        public int Row { get; set; }
        public int Column { get; set; }
        public List<int> Candidates { get; set; } // 複数候補の数字
        }

        // 一時保存するメソッド
        private void SaveCellStates(List<CellState> cellStates)
        {
            string json = JsonConvert.SerializeObject(cellStates, Formatting.Indented);
            File.WriteAllText("cell_states.json", json);
            MessageBox.Show("状態が一時保存されました。", "情報");
        }

        // 一時保存用のボタン処理
        private void button5_Click(object sender, EventArgs e)
        {
            // セルの状態を取得するリストを準備
            List<CellState> cellStates = new List<CellState>();

            // 例：DataGridViewの行と列をループして入力を取得
            for (int row = 0; row < dataGridView1.Rows.Count; row++)
            {
                for (int column = 0; column < dataGridView1.Columns.Count; column++)
                {
                    var cellValue = dataGridView1.Rows[row].Cells[column].Value?.ToString(); // 入力値を取得
                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        // 候補がない場合は1つの値として扱う
                        cellStates.Add(new CellState
                        {
                            Row = row,
                            Column = column,
                            Candidates = new List<int> { int.Parse(cellValue) } // 候補ではなく実値
                        });
                    }
                }
            }

            // 入力された状態を保存
            SaveCellStates(cellStates);
            MessageBox.Show("入力された状態を保存しました。", "情報");
        }
        
        private void DataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.All);

            using (Pen pen = new Pen(Color.Black, 2))
            {
                if (e.RowIndex >= 0 && e.RowIndex % 3 == 0)
                {
                    e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Right, e.CellBounds.Top);
                }

                if (e.ColumnIndex >= 0 && e.ColumnIndex % 3 == 0)
                {
                    e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Left, e.CellBounds.Bottom);
                }
            }

            e.Handled = true;
        }
            

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private List<CellState> LoadCellStates()
        {
            string json = File.ReadAllText("cell_states.json");
            return JsonConvert.DeserializeObject<List<CellState>>(json);
        }

        // 復元ボタン処理
        private void button6_Click(object sender, EventArgs e)
        {
            List<CellState> cellStates = LoadCellStates();
            foreach (var cellState in cellStates)
            {
                dataGridView1.Rows[cellState.Row].Cells[cellState.Column].Value =
                    string.Join(",", cellState.Candidates);
            }
            MessageBox.Show($"復元されたセル数: {cellStates.Count}", "情報");
        }

    }
}
