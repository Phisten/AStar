
using System;
using System.Collections.Generic;
using System.Text;

namespace PathFinding
{
    /// <summary>
    /// 在一個地圖中,我們要搜尋從A點到B點的最低成本路徑,
    /// 所得的最短路徑為一個節點陣列,
    /// 為路徑通過的結點在連續的空間環境中,
    /// 可能較難建立節點,因為鄰近結點之間要能相通,
    /// 在方塊世界比較容易
    /// </summary>
    public class AStar
    {
        //地圖大小
        private int MapSize = 13;
        //起始位置
        private int StartLocation = 53;//計算的為index位置，MapSize * 4 + 2 - 1
        //目的位置
        private int GoalLocation = 73;//計算的為index位置，MapSize * 5 + 9 - 1

        private int CurrentLocation;//目前位置

        //Current location 邊緣矩陣
        private double[,] OpenMatrix;
        //邊緣矩陣大小
        private int OpenSize = 0;
        //OPEN表保存了所有已生成而未考察的節點，CLOSED表中記錄已訪問過的節點。
        private double[,] ClosedMatrix;
        //OPEN表保存了所有已生成而未考察的節點，CLOSED表中記錄已訪問過的節點。
        private int ClosedSize = 0;
        //資料矩陣大小
        private int[] Map = new int[] { 
1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 
1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 1, 
1, 0, 1, 1, 1, 0, 1, 1, 0, 0, 0, 0, 1, 
1, 0, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 
1, 0, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 
1, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 1, 
1, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 1, 
1, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 1, 
1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 
1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };


        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="MapSize">矩陣大小</param>
        /// <param name="StartLocation">起始位置</param>
        /// <param name="GoalLocation">目的位置</param>
        /// <param name="Map">地圖資料</param>
        public AStar(int MapSize, int StartLocation, int GoalLocation, int[] Map)
        {
            this.MapSize = MapSize;
            this.StartLocation = StartLocation;
            this.GoalLocation = GoalLocation;
            this.OpenMatrix = new double[(int)Math.Pow(MapSize, 2) + 1, 3];
            this.ClosedMatrix = new double[(int)Math.Pow(MapSize, 2) + 1, 3];
            this.Map = Map;
        }

        /// <summary>
        /// 目的位置
        /// </summary>
        public int GoalLocationPoint
        {
            set { this.GoalLocation = value; }
            get { return this.GoalLocation; }
        }

        /// <summary>
        /// 起始位置
        /// </summary>
        public int StartLocationPoint
        {
            set { this.StartLocation = value; }
            get { return this.StartLocation; }
        }

        /// <summary>
        /// 找出路徑
        /// </summary>
        /// <returns></returns>
        public bool FindPathFunction()
        {
            try
            {
                Initialize();

                //int i = 0;
                //目前位置，還不是目的端位置
                while (!(CurrentLocation == GoalLocation))
                {
                    //開始找路徑
                    if (FindRoute().Equals(0))
                    {
                        //break;//當找不到就離開避免進入無窮迴圈
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 初始化矩陣空間
        /// </summary>
        private void Initialize()
        {
            CurrentLocation = StartLocation;//把起始位置存放入目前位置
            OpenMatrix[0, 0] = CurrentLocation;//存放目前位置
            OpenMatrix[0, 1] = 0;//目前父結點位置
            OpenMatrix[0, 2] = GetL2(CurrentLocation, GoalLocation);//與目的位置相隔多少距離

            //把Fringe陣列全部清空
            for (int i = 1; i <= (Math.Pow(MapSize, 2)); i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    OpenMatrix[i, j] = 0;
                }
            }

            //把CLOSED表中記錄已訪問過的節點，紀錄為-1表示未曾拜訪過。
            for (int i = 0; i <= (Math.Pow(MapSize, 2)); i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    ClosedMatrix[i, j] = -1;
                }
            }
        }

        /// <summary>
        /// 找出路徑
        /// </summary>
        /// <returns></returns>
        private int FindRoute()
        {
            //Returns GoalLocation if a route has been completely found,
            // or the next node of the route,
            // or returns zero if not.
            int ReturnValue = 0;

            /*
            * 如果已經完成搜尋則回傳目的位置GoalLocation
            * 反之，則傳回下一個位置
            * 或者，如果找不到就回傳0
            */
            if (OpenMatrix[0, 0] == 0)
            {
                //在起始位置與結束位置間沒有任何的路徑
                //No route exists between start and goal.
                ReturnValue = 0;
            }
            else
            {
                //已經找到最後的位置GoalLocation
                if (OpenMatrix[0, 0] == GoalLocation)
                {
                    //Add node to ClosedList.
                    //增加目前的節點到拜訪過的矩陣中ClosedList
                    for (int i = 0; i < 3; i++)
                    {
                        ClosedMatrix[ClosedSize, i] = OpenMatrix[0, i];
                    }
                    //return RouteNode.
                    CurrentLocation = GoalLocation;
                    ReturnValue = CurrentLocation;
                }
                else//持續找尋
                {
                    //表示OpenMatrix起始位置未被蒐尋過
                    if (CheckCloseListForNode((int)OpenMatrix[0, 0]) == false)//起始位置
                    {
                        //Add node to ClosedList.
                        //把起始位置放入已經搜尋過的矩陣內
                        for (int i = 0; i < 3; i++)
                        {
                            ClosedMatrix[ClosedSize, i] = OpenMatrix[0, i];
                        }

                        //取出目前的位置
                        CurrentLocation = (int)ClosedMatrix[ClosedSize, 0];

                        //以搜尋過的路徑增加
                        ClosedSize += 1;

                        //Remove node from Fringe().把目前的節點由Open矩陣內移除
                        RemoveNodeFromOpen(CurrentLocation);

                        //Add children to Open().計算Open矩陣內的3x3矩陣距離目標的長短，並排序之
                        AddChildrenToOpen(CurrentLocation);

                        //Return new CurrentLocation.
                        ReturnValue = CurrentLocation;
                    }
                }

            }
            return ReturnValue;
        }

        /// <summary>
        /// 增加3x3可能路徑
        /// </summary>
        /// <param name="ParentNode"></param>
        private void AddChildrenToOpen(int ParentNode)
        {
            //Check the ParentNode's surrounding nodes.
            int index = 0;
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    index = ParentNode + MapSize * i + j;
                    if (Map[index] != 1
                    && Map[index] == 0)//表示非邊緣，且有路徑可以走
                    {
                        //Check to see if it's already in the ClosedList.檢查此點是否已經在Cose矩陣內，不是的話，加入Open矩陣內
                        if (CheckCloseListForNode(index) == false)
                        {
                            OpenSize += 1;
                            //Add child node to Open矩陣
                            OpenMatrix[OpenSize, 0] = index;//可以嘗試的路徑(目前結點)
                            OpenMatrix[OpenSize, 1] = ParentNode;//目前父結點位置
                            OpenMatrix[OpenSize, 2] = GetL2(index, GoalLocation);//計算目前結點與目標結點的歐幾里得距離 
                        }
                    }
                }
                //Start the next row down.
            }
            SortList();//排序矩陣
        }

        /// <summary>
        /// 歐幾里得距離
        /// </summary>
        /// <param name="CurrentLoc"></param>
        /// <param name="GoalLoc"></param>
        /// <returns></returns>
        private double GetL2(int CurrentLoc, int GoalLoc)
        {
            int X1 = 0;
            int X2 = 0;
            int Y1 = 0;
            int Y2 = 0;
            double DX = 0;
            double DY = 0;

            X1 = CurrentLoc % MapSize;//取得餘數 Column's Value
            X2 = GoalLoc % MapSize;//取得餘數 Column's Value

            Y1 = (int)Math.Floor(CurrentLoc / (double)MapSize);//取得商數 Row's Value
            Y2 = (int)Math.Floor(GoalLoc / (double)MapSize);//取得商數 Row's Value

            DX = Math.Abs(X2 - X1);//X軸
            DY = Math.Abs(Y2 - Y1);//Y軸

            return Math.Sqrt(Math.Pow(DX, 2) + Math.Pow(DY, 2));//歐幾里得距離

        }

        /// <summary>
        /// 檢查這個節點是否已經被蒐尋過，事的話回傳true，反之false
        /// </summary>
        /// <param name="NodeLocation"></param>
        /// <returns></returns>
        private bool CheckCloseListForNode(int NodeLocation)
        {
            bool ItemFound = false;

            for (int i = 0; i < ClosedSize; i++)
            {
                if (ClosedMatrix[i, 0] == NodeLocation)
                {
                    ItemFound = true;
                    break;
                }
            }

            return ItemFound;
        }

        /// <summary>
        /// 排序目前的Open矩陣，依據歐幾里得距離
        /// </summary>
        private void SortList()
        {
            //排序Open矩陣依據歐幾里得距離進行升序排序
            //Sort Open Matrix in ascending order of Distance + L2(smallest at start, largest at end).
            double[] TempArray = new double[3];

            for (int i = 0; i < OpenSize; i++)
            {
                for (int j = i + 1; j <= OpenSize; j++)
                {
                    if (OpenMatrix[i, 2] > OpenMatrix[j, 2])//如果i比較j大就交換
                    {
                        //Copy succeeding element from Fringe() to TempArray().
                        for (int k = 0; k < TempArray.Length; k++)
                        {
                            TempArray[k] = OpenMatrix[j, k];
                        }
                        //Copy Fringe() element to succeeding Fringe() element.
                        for (int k = 0; k < TempArray.Length; k++)
                        {
                            OpenMatrix[j, k] = OpenMatrix[i, k];
                        }
                        //Copy TempArray() to preceeding Fringe() element. 
                        for (int k = 0; k < TempArray.Length; k++)
                        {
                            OpenMatrix[i, k] = TempArray[k];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 從open矩陣移除關連到CurrentLocation的結點
        /// </summary>
        /// <param name="Node"></param>
        private void RemoveNodeFromOpen(int Node)
        {
            //從open矩陣移除關連到CurrentLocation的結點
            //Removes all references to CurrentLocation from Fringe().
            double[,] TempArray = new double[OpenSize, 3];
            int k = 0;
            for (int i = 0; i <= OpenSize; i++)
            {
                if (OpenMatrix[i, 0] != Node)//open矩陣不包括CurrentLocation的結點
                {
                    for (int j = 0; j < 3; j++)//將open矩陣記錄到暫存矩陣TempArray
                    {
                        TempArray[k, j] = OpenMatrix[i, j];
                    }
                    k++;
                }
            }

            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    OpenMatrix[i, j] = TempArray[i, j];//將TempArray暫存矩陣轉存到矩陣open
                }
            }

            OpenSize = k - 1;
        }

        /// <summary>
        /// 列印出Closed矩陣資料
        /// </summary>
        public string PrintClosedMatrix()
        {
            int i = 0;
            int j = 0;
            string TempString = null;

            for (i = 0; i <= ClosedSize; i++)
            {
                for (j = 0; j < 2; j++)
                {
                    TempString += Convert.ToString(ClosedMatrix[i, j]) + "<-";
                }
                TempString += "\r\n";
            }

            return TempString;
        }

        /// <summary>
        /// 找出最短路徑
        /// </summary>
        public string TraceBack(ref List<int> pathIndex)
        {
            int index = ClosedSize;//Trace parent nodes back to start.
            double[,] TempArray = new double[ClosedSize + 1, 2];
            string TempString = string.Empty;
            int RouteStart = 0;
            bool Done = false;
            int k = ClosedSize;

            //Start i and k at the end of the list and count backward.
            for (int i = 0; i < 2; i++)
            {
                TempArray[ClosedSize, i] = ClosedMatrix[ClosedSize, i];
            }

            //This loop follows the trail of 
            do
            {
                //from the Goal node.
                //All other nodes in ClosedList()
                for (int j = index; j >= 0; j += -1)
                {
                    //are dead-end nodes that the
                    if (ClosedMatrix[j, 0] == ClosedMatrix[index, 1])
                    {
                        //algorithm decided against.
                        index = j;

                        k--;
                        for (int i = 0; i < 2; i++)
                        {
                            TempArray[k, i] = ClosedMatrix[index, i];
                        }

                        if (ClosedMatrix[index, 0] == StartLocation)
                        {
                            RouteStart = k;
                            //StartLocation is at the last decrement of k.
                            Done = true;
                        }

                        break;
                    }
                    if (j == 0) Done = true;//避免陷入無窮迴圈
                }

            } while (!(Done));


            pathIndex = new List<int>(ClosedSize);
            //Print the rendered route.
            for (int i = RouteStart; i <= ClosedSize; i++)
            {
                TempString += Convert.ToString(TempArray[i, 0]) + ", ";
                pathIndex.Add((int)TempArray[i, 0]);
            }

            return TempString;
        }
    }
}