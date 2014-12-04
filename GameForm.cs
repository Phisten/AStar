using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace aStar
{
    public partial class GameForm : Form
    {
        public GameForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RandomMapInit_Click(null, null);
        }

        /// <summary>路徑是否可走斜線</summary>
        bool AllowDiagonal = false;


        /// <summary>地圖的邏輯長與寬</summary>
        int MapSize = -1;
        /// <summary>每格座標的像素長寬</summary>
        int MapCellSize = 30;
        /// <summary>起點座標索引</summary>
        int Enemy = 0;
        /// <summary>終點座標索引</summary>
        int Player = 0;
        /// <summary>最佳路徑</summary>
        //List<int> PathIndex = null;
        List<PathFinding.AStar.AStarNode> bestPath;

        /// <summary>地圖資訊</summary>
        //int[] MapData = null;

        PathFinding.AStar.AStarMap aStarMap;

        /// <summary>
        /// 建構空白地圖
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapInit_Click(object sender, EventArgs e)
        {
            MapSize = int.Parse(textBox3.Text);
            MapCellSize = int.Parse(textBox4.Text);
            //MapData = new int[MapSize * MapSize];


            aStarMap = new PathFinding.AStar.AStarMap(MapSize, MapSize);
            //初始化地圖邊界
            for (int i = 0, length = MapSize; i < length; i++)
            {
                aStarMap[0][i].Value = 1;
                aStarMap[MapSize - 1][i].Value = 1;
                aStarMap[i][MapSize - 1].Value = 1;
                aStarMap[i][0].Value = 1;


            }

            //初始化起終點
            Enemy = MapSize + 1;
            Player = MapSize * MapSize - MapSize - 2;
            ScorePoint = Enemy;
            //PathIndex = null;
            bestPath = null;
            DrawMap();

        }

        /// <summary>
        /// 建構亂數地圖
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RandomMapInit_Click(object sender, EventArgs e)
        {
            MapSize = int.Parse(textBox3.Text);
            MapCellSize = int.Parse(textBox4.Text);
            //MapData = new int[MapSize * MapSize];


            aStarMap = new PathFinding.AStar.AStarMap(MapSize, MapSize);
            //初始化地圖邊界
            for (int i = 0, length = MapSize; i < length; i++)
            {
                aStarMap[0][i].Value = 1;
                aStarMap[MapSize - 1][i].Value = 1;
                aStarMap[i][MapSize - 1].Value = 1;
                aStarMap[i][0].Value = 1;
            }

            Random rnd = new Random();

            for (int i = 1; i < MapSize - 1; i++)
            {
                for (int j = 1; j < MapSize - 1; j++)
                {
                    int val = rnd.Next() % 8;
                    aStarMap[i][j].Value = val > 1 ? 0 : 1;
                }
            }
            //初始化起終點
            Enemy = MapSize + 1;
            Player = MapSize * MapSize - MapSize - 2;
            ScorePoint = Enemy;
            //PathIndex = null;
            bestPath = null;

            aStarMap[Enemy / MapSize][Enemy % MapSize].Value = 0;
            aStarMap[Player / MapSize][Player % MapSize].Value = 0;

            DrawMap();

            bool findPath = PathFinding.AStar.FindPath(Enemy, Player, AllowDiagonal, ref aStarMap, out bestPath) == 0;
            if (findPath == true)
            {
                button4.Text = "開始遊戲";
                button4.Enabled = true;
            }
            else
            {
                button4.Text = "地圖沒有活路";
                button4.Enabled = false;
            }
        }


        private void DrawMap()
        {
            DrawMap(MapSize, MapCellSize, aStarMap, Enemy, Player, ScorePoint, bestPath);
        }
        private void DrawMap(int mapSize, int mapCellSize, PathFinding.AStar.AStarMap mapData, int startIndex, int endIndex, int scorePoint, List<PathFinding.AStar.AStarNode> bestPath = null)
        {

            int mapCellCenter = mapCellSize / 2;

            Bitmap b = new Bitmap(mapSize * mapCellSize, mapSize * mapCellSize);
            Graphics g = Graphics.FromImage(b);


            Pen zeroPen = new Pen(Color.White, mapCellSize); //用於填滿可通過的區域
            Pen onePen = new Pen(Color.Black, mapCellSize); //用於填滿無法通過的區域
            Pen linePen = new Pen(Color.Gray, 1); //用於填滿無法通過的區域

            Pen[] mapInf = new Pen[] { zeroPen, onePen };
            //根據地圖資料(mapData)填補地形
            if (mapData == null)
            {
                throw new ApplicationException();
            }
            for (int i = 0; i < mapData.Width; i++)
            {
                for (int j = 0; j < mapData.Height; j++)
                {
                    int x = i * mapCellSize;
                    int y = j * mapCellSize + +mapCellCenter;
                    g.DrawLine(mapInf[mapData[i][j].Value], x, y, x + mapCellSize, y);
                }
            }

            //畫上得分點
            Pen ScorePointPen = new Pen(Color.Yellow, mapCellSize);
            int ScorePointX = (scorePoint % mapSize) * mapCellSize;
            int ScorePointY = (scorePoint / mapSize) * mapCellSize + mapCellCenter;
            g.DrawLine(ScorePointPen, ScorePointX, ScorePointY, ScorePointX + mapCellSize, ScorePointY);

            //畫上玩家
            Pen endPen = new Pen(Color.Blue, mapCellSize);
            int endX = (endIndex % mapSize) * mapCellSize;
            int endY = (endIndex / mapSize) * mapCellSize + mapCellCenter;
            g.DrawLine(endPen, endX, endY, endX + mapCellSize, endY);

            //畫上敵人
            Pen startPen = new Pen(Color.Red, mapCellSize);
            int startX = (startIndex % mapSize) * mapCellSize;
            int startY = (startIndex / mapSize) * mapCellSize + mapCellCenter;
            g.DrawLine(startPen, startX, startY, startX + mapCellSize, startY);


            //畫上外框
            //g.DrawRectangle(onePen, mapCellCenter, mapCellCenter, mapCellSize * (mapSize - 1) , mapCellSize * (mapSize - 1) );
            //畫上網格
            for (int i = 1; i < mapSize; i++)
            {
                g.DrawLine(linePen, 0, i * mapCellSize, mapSize * mapCellSize, i * mapCellSize);
                g.DrawLine(linePen, i * mapCellSize, 0, i * mapCellSize, mapSize * mapCellSize);
            }


            pic1.Image = b;
            pic1.ClientSize = b.Size;

        }


        PlayerKeyState playerMoveDirect = 0;
        //KeyEventArgs playerKeyCode = new KeyEventArgs(new Keys());
        private void timer1_Tick(object sender, EventArgs e)
        {
            int playerX = Player % MapSize;
            int playerY = Player / MapSize;

            int X_Move = 0;
            int Y_Move = 0;
            if (playerMoveDirect == PlayerKeyState.Left)
            {
                X_Move--;
            }
            if (playerMoveDirect == PlayerKeyState.Up)
            {
                Y_Move--;
            }
            if (playerMoveDirect == PlayerKeyState.Right)
            {
                X_Move++;
            }
            if (playerMoveDirect == PlayerKeyState.Down)
            {
                Y_Move++;
            }
            int newX = playerX + X_Move;
            int newY = playerY + Y_Move;
            if (0 < newX && newX < MapSize - 1 && 0 < newY && newY < MapSize - 1 &&
                aStarMap[newX][newY].Value == 0)
            {
                Player = MapSize * newY + newX;
            }

            aiStep++;
            if (aiStep >= 2)
            {
                bool findPath = PathFinding.AStar.FindPath(Enemy, Player, AllowDiagonal, ref aStarMap, out bestPath) == 0;
                if (findPath == true && bestPath.Count > 2)
                {
                    Enemy = bestPath[bestPath.Count - 2].Index;
                }
                aiStep = 0;
            }

            if (Player == ScorePoint)
            {
                Score++;
                label3.Text = "目前得分：" + Score;

                Random rand = new Random();
                do
                {
                    ScorePoint = rand.Next() % (MapSize * MapSize - 1);
                } while (aStarMap.GetNodeFromIndex(ScorePoint).Value != 0);
            }


            DrawMap();

            if (Enemy == Player)
            {
                button4_Click_GameStart(null,null);
                MessageBox.Show("Game Over! 得分:" + Score);
            }
        }

        int aiStep = 0;
        int Score = 0;
        int ScorePoint = 0;

        private void button4_Click_GameStart(object sender, EventArgs e)
        {
            bool curEnableState = !timer1.Enabled;
            bool curInvEnableState = timer1.Enabled;

            if (curEnableState)
            {
                //初始化起終點
                Enemy = MapSize + 1;
                Player = MapSize * MapSize - MapSize - 2;
                ScorePoint = Enemy;
                Score = 0;
                label3.Text = "目前得分：0";

                timer1.Interval = 125;
                button4.Text = "停止遊戲";
            }
            else
            {
                button4.Text = "開始遊戲";
            }

            //button4.Enabled = curInvEnableState;
            button1.Enabled = curInvEnableState;
            button3.Enabled = curInvEnableState;
            textBox3.Enabled = curInvEnableState;
            textBox4.Enabled = curInvEnableState;
            timer1.Enabled = curEnableState;
        }


        private void GameForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left && playerMoveDirect == PlayerKeyState.Left)
            {
                playerMoveDirect = PlayerKeyState.Null;
            }
            if (e.KeyCode == Keys.Up && playerMoveDirect == PlayerKeyState.Up)
            {
                playerMoveDirect = PlayerKeyState.Null;
            }
            if (e.KeyCode == Keys.Right && playerMoveDirect == PlayerKeyState.Right)
            {
                playerMoveDirect = PlayerKeyState.Null;
            }
            if (e.KeyCode == Keys.Down && playerMoveDirect == PlayerKeyState.Down)
            {
                playerMoveDirect = PlayerKeyState.Null;
            }
        }

        [Flags]
        public enum PlayerKeyState
        {
            Null = 0, Left = 1, Up = 2, Right = 4, Down = 8
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Left)
            {
                playerMoveDirect = PlayerKeyState.Left;
            }
            if (keyData == Keys.Up)
            {
                playerMoveDirect = PlayerKeyState.Up;
            }
            if (keyData == Keys.Right)
            {
                playerMoveDirect = PlayerKeyState.Right;
            }
            if (keyData == Keys.Down)
            {
                playerMoveDirect = PlayerKeyState.Down;
            }
            return base.ProcessDialogKey(keyData);
        }
    }
}
