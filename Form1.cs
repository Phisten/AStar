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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MapInit_Click(null, null);
        }

        /// <summary>
        /// 探索路徑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PathFinding_Click(object sender, EventArgs e)
        {

            bestPath = null;
            bool findPath = PathFinding.AStar.FindPath(MapStartIndex, MapEndIndex,ref aStarMap, out bestPath) == 0;
            if (findPath == false)
            {
                MessageBox.Show("沒有從起點移動至終點的路徑,請修改地圖");
            }
            else
            {
                DrawMap();
            }

            
        }


        /// <summary>地圖的邏輯長與寬</summary>
        int MapSize = -1;
        /// <summary>每格座標的像素長寬</summary>
        int MapCellSize = 30;
        /// <summary>起點座標索引</summary>
        int MapStartIndex = 0;
        /// <summary>終點座標索引</summary>
        int MapEndIndex = 0;
        /// <summary>最佳路徑</summary>
        //List<int> PathIndex = null;
        List<PathFinding.AStar.AStarNode> bestPath;

        /// <summary>地圖資訊</summary>
        //int[] MapData = null;

        PathFinding.AStar.AStarMap aStarMap;

        /// <summary>
        /// 建構地圖
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
            for (int i = 0,length = MapSize; i < length; i++)
            {
                aStarMap[0][i].Value = 1;
                aStarMap[MapSize-1][i].Value = 1;
                aStarMap[i][MapSize - 1].Value = 1;
                aStarMap[i][0].Value = 1;


            }

            //初始化起終點
            MapStartIndex = MapSize + 1;
            MapEndIndex = MapSize * MapSize - MapSize - 2;
            //PathIndex = null;
            bestPath = null;
            DrawMap();

        }


        private void DrawMap()
        {
            DrawMap(MapSize, MapCellSize, aStarMap, MapStartIndex, MapEndIndex, bestPath);
        }
        private void DrawMap(int mapSize, int mapCellSize, PathFinding.AStar.AStarMap mapData, int startIndex, int endIndex, List<PathFinding.AStar.AStarNode> bestPath = null)
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

            for (int i = 0, length = mapData.Size; i < length; i++)
            {

            }

            //畫上路徑
            Pen pathPen = new Pen(Color.LightBlue, mapCellSize); //用於填滿最短的區域
            if (bestPath != null)
            {
                for (int i = 0, length = bestPath.Count; i < length; i++)
                {
                    int x = (bestPath[i].X) * mapCellSize;
                    int y = (bestPath[i].Y) * mapCellSize + +mapCellCenter;
                    g.DrawLine(pathPen, x, y, x + mapCellSize, y);
                }
            }

            //畫上起點
            Pen startPen = new Pen(Color.Blue, mapCellSize);
            int startX = (startIndex % mapSize) * mapCellSize;
            int startY = (startIndex / mapSize) * mapCellSize + +mapCellCenter;
            g.DrawLine(startPen, startX, startY, startX + mapCellSize, startY);

            //畫上終點
            Pen endPen = new Pen(Color.Red, mapCellSize);
            int endX = (endIndex % mapSize) * mapCellSize;
            int endY = (endIndex / mapSize) * mapCellSize + +mapCellCenter;
            g.DrawLine(endPen, endX, endY, endX + mapCellSize, endY);

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


        /// <summary>地圖編輯模式(0:修改地圖資訊,1:設定起點位置,2:設定終點位置)</summary>
        int MapEditMode = 0;
        /// <summary>
        /// 修改地圖編輯模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton_MapEditMode_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton[] mapEditModeRbox = new RadioButton[] { radioButton1, radioButton2, radioButton3 };
            for (int i = 0; i < mapEditModeRbox.Length; i++)
                if (mapEditModeRbox[i] == sender as RadioButton)
                    MapEditMode = i;
        }

        private void pic1_Click(object sender, EventArgs e)
        {
            var s = sender as PictureBox;
            var mouse = e as MouseEventArgs;
            int index = CoordinateTransform(mouse.X, mouse.Y);
            MapEdit(index);
            DrawMap();
        }



        /// <summary>記錄前次index避免重複修改</summary>
        int LastEditIndex = -1;
        /// <summary>
        /// 編輯地形或設定起終點
        /// </summary>
        /// <param name="index"></param>
        private void MapEdit(int index)
        {
            int X = index % MapSize;
            int Y = index / MapSize;
            if (LastEditIndex == index)
            {
                return;
            }
            //一旦編輯地圖, 路徑就失效
            //PathIndex = null;
            bestPath = null;

            switch (MapEditMode)
            {
                case 0: //地形編輯
                    //MapData[index] = MapData[index] == 0 ? 1 : 0;
                    aStarMap[X][Y].Value = aStarMap[X][Y].Value == 0 ? 1 : 0;
                    break;
                case 1: //起點設定
                    MapStartIndex = index;
                    break;
                case 2: //終點設定
                    MapEndIndex = index;
                    break;
                default:
                    break;
            }
            LastEditIndex = index;
        }


        private void pic1_MouseMove(object sender, MouseEventArgs e)
        {
            var s = sender as PictureBox;
            var mouse = e as MouseEventArgs;
            int index = CoordinateTransform(mouse.X, mouse.Y);
            if (MousePress == true && index < MapSize * MapSize)
            {
                MapEdit(index);
                DrawMap();
            }
        }

        /// <summary>
        /// 像素座標轉二維邏輯座標後輸出至label,並回傳一維邏輯座標(索引)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private int CoordinateTransform(int pixelX, int pixelY)
        {
            int mapCellCenter = MapCellSize / 2;

            int logicX = (pixelX ) / MapCellSize;
            int logicY = (pixelY ) / MapCellSize;

            int index = logicX + logicY * MapSize;

            //更新邏輯座標資訊
            label3.Text = "座標：" + logicX.ToString() + "," + logicY.ToString();

            return index;
        }

        bool MousePress;
        private void pic1_MouseDown(object sender, MouseEventArgs e)
        {
            MousePress = true;
        }

        private void pic1_MouseUp(object sender, MouseEventArgs e)
        {
            MousePress = false;
            LastEditIndex = -1;
        }

        private void pic1_MouseLeave(object sender, EventArgs e)
        {
            MousePress = false;
            LastEditIndex = -1;
        }


    }
}
