
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace PathFinding
{
    public class AStar
    {
        public static int FindPath(int startNodeIndex, int endNodeIndex, ref AStarMap map, out List<AStarNode> bestPath)
        {
            //input check
            if (startNodeIndex < 0 || startNodeIndex >= map.Size)
            {
                throw new ApplicationException();
            }
            if (endNodeIndex < 0 || endNodeIndex >= map.Size)
            {
                throw new ApplicationException();
            }
            if (map == null)
            {
                throw new ApplicationException();
            }
            
            //init
            int Start = startNodeIndex;
            int end = endNodeIndex;
            int curNodeIndex = Start;

            List<AStarNode> openList = new List<AStarNode>();
            List<AStarNode> closedList = new List<AStarNode>();


            map.PathReset();

            openList.Add(map.GetNodeFromIndex(startNodeIndex));
            openList[0].Cost = 0;
            openList[0].Risk = map.GetGridDistance(startNodeIndex, endNodeIndex);
            openList[0].State = AStarNode.NodeState.Open;
            map.SetAllNodeRisk(endNodeIndex);

            int PathSearchResult = PathTestLoop(endNodeIndex, map, openList, closedList);

            bestPath = new List<AStarNode>();
            //search bestPath
            if (PathSearchResult == 0)
            {
                TrackBack(endNodeIndex, map, bestPath);
            }

            return PathSearchResult;
        }

        private static void TrackBack(int endNodeIndex, AStarMap map, List<AStarNode> bestPath)
        {
            bool trackBackContinue = true;
            AStarNode curNode = map.GetNodeFromIndex(endNodeIndex);
            do
            {
                bestPath.Add(curNode);
                if (curNode.ParentNode != null)
                {
                    curNode = curNode.ParentNode;
                }
                else
                {
                    trackBackContinue = false;
                }
            } while (trackBackContinue);
        }

        private static int PathTestLoop(int endNodeIndex, AStarMap map, List<AStarNode> openList, List<AStarNode> closedList)
        {
            int searchResult = 0;
            //path finding loop
            bool SearchEnd = false;
            do
            {
                //檢查OpenList內是否還有待探索節點, 無則表示已無活路
                if (openList.Count == 0)
                {
                    searchResult = 1;
                    SearchEnd = true;

                    break;
                }

                //get the Node of lowest total cost
                AStarNode curNode = openList[0];
                for (int i = 0, length = openList.Count; i < length; i++)
                    if (curNode.TotalCost > openList[i].TotalCost)
                        curNode = openList[i];


                //move curNode to closeList
                curNode.State = AStarNode.NodeState.Close;
                closedList.Add(curNode);
                openList.Remove(curNode);

                //Expansion Neighboring Node , label their Open
                for (int i = -1; i < 2; i++)
                {
                    int X = curNode.X + i;
                    for (int j = -1; j < 2; j++)
                    {
                        int Y = curNode.Y + j;
                        if (X >= 0 && X < map.Width && Y >= 0 && Y < map.Height) //未超過地圖邊界
                        {
                            AStarNode newNodeTmp = map[X][Y];
                            if (newNodeTmp.Value == 0)  //Value 0 代表可通過 無障礙物
                                //&& newNodeTmp != curNode) //排除Comparison by self
                            {
                                // if is endPoint
                                if (newNodeTmp.Index == endNodeIndex)
                                {
                                    SearchEnd = true;
                                }

                                //if newNode is open or close, select better
                                switch (newNodeTmp.State)
                                {
                                    case AStarNode.NodeState.Open:
                                    case AStarNode.NodeState.Close:
                                        // if newPath is not better
                                        // *this step will Exclude curNode self and curNode.Parent
                                        if (newNodeTmp.Cost <= curNode.Cost + 1)
                                        {
                                            continue;
                                        }
                                        openList.Remove(newNodeTmp);
                                        closedList.Remove(newNodeTmp);
                                        break;
                                    case AStarNode.NodeState.Unvisited:
                                        break;
                                    default:
                                        throw new ApplicationException();
                                        break;
                                }

                                curNode.SetChildNode(ref newNodeTmp);
                                //move curNode to openList
                                openList.Add(newNodeTmp);

                                //if (!openList.Exists(x=>x==curNode))
                                //{
                                //    openList.Add(curNode);
                                //}

                            }
                        }
                    }
                }


            } while (!SearchEnd);
            return searchResult;
        }


        public class AStarMap
        {
            private List<List<AStarNode>> Data; // Data[W or X][H or Y]

            public int Width;
            public int Height;
            public int Size;

            public AStarMap(int width, int height)
            {
                this.Width = width;
                this.Height = height;
                Size = Width * Height;
                Data = new List<List<AStarNode>>(Width);
                for (int i = 0; i < Width; i++)
                {
                    Data.Add(new List<AStarNode>(Height));
                    for (int j = 0; j < Height; j++)
                    {
                        Data[i].Add(new AStarNode() { Index = i + Height * j, X = i, Y = j });
                    }
                }

            }

            public void PathReset() 
            {
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        Data[i][j].Cost = int.MaxValue;
                        Data[i][j].Risk = int.MaxValue;
                        Data[i][j].TotalCost = int.MaxValue;

                        Data[i][j].ParentNode = null;
                        Data[i][j].State = AStarNode.NodeState.Unvisited;
                    }
                }
            }

            public int SetAllNodeRisk(int targetIndex)
            {
                return SetAllNodeRisk(targetIndex % Width , targetIndex / Height);
            }
            public int SetAllNodeRisk(int targetX,int targetY)
            {
                AStarNode tarNode = Data[targetX][targetY];

                for (int i = 0; i < Width; i++)
                    for (int j = 0; j < Height; j++)
                        Data[i][j].Risk = Data[i][j].GetGridDistance(tarNode);

                return (int)AStarMapErrorCode.Success;
            }

            #region 透過索引或座標取得地圖資訊
            /// <summary>
            /// 透過座標取得地圖資訊傳入值為[X][Y]
            /// </summary>
            /// <param name="X">水平座標</param>
            /// <returns></returns>
            public List<AStarNode> this[int X]
            {
                get { return Data[X]; }
                set { Data[X] = value; }
            }
            /// <summary>
            /// 透過索引取得地圖資訊
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public AStarNode GetNodeFromIndex(int index)
            {
                return Data[index % Width][index / Width];
            }
            //public AStarNode GetNodeFromCoordinate(int X, int Y)
            //{
            //    return Data[X][Y];
            //}
            #endregion

            #region 座標轉換與距離計算 
            /// <summary>
            /// 取得兩座標在地圖中的最短網格距離(不考慮障礙)
            /// </summary>
            /// <param name="X1"></param>
            /// <param name="Y1"></param>
            /// <param name="X2"></param>
            /// <param name="Y2"></param>
            /// <returns></returns>
            public int GetGridDistance(int X1, int Y1, int X2, int Y2)
            {
                return Math.Abs((X1 - X2) - (Y1 - Y2));
            }
            /// <summary>
            /// 取得兩索引在地圖中的最短網格距離(不考慮障礙)
            /// </summary>
            /// <param name="index1"></param>
            /// <param name="index2"></param>
            /// <returns></returns>
            public int GetGridDistance(int index1, int index2)
            {
                return Math.Abs((index1 % Size - index2 % Size) - (index1 / Size - index2 / Size));
            }
            #endregion



            public enum AStarMapErrorCode { Success }
        }


        public class AStarNode
        {
            //base
            public int Value;
            public int Index;
            public int X;
            public int Y;

            //path data
            public int TotalCost; //func F 值越低越好
            public int Cost = int.MaxValue; //func G
            public int Risk = int.MaxValue; //func H
            public AStarNode ParentNode = null;
            public NodeState State = NodeState.Unvisited;

            public enum NodeState { Unvisited, Open, Close };

            /// <summary>
            /// 將傳入節點設定為子節點,並將狀態設為open
            /// </summary>
            /// <param name="childNode"></param>
            /// <returns></returns>
            public int SetChildNode(ref AStarNode childNode)
            {
                if (childNode == null)
                    return (int)AStarNodeErrorCode.childNode_Is_Null;

                childNode.ParentNode = this;
                childNode.Cost = this.Cost + 1;
                childNode.State = NodeState.Open;

                return (int)AStarNodeErrorCode.Success;
            }

            public int GetGridDistance(AStarNode targetNode)
            {
                return Math.Abs((this.X - targetNode.X) - (this.Y - targetNode.Y));
            }

            public enum AStarNodeErrorCode { Success, childNode_Is_Null }
        }
    }





    public class AStar_fake
    {
        //地圖大小
        private int MapSize = 13;
        //起始位置
        private int StartLocation = 53;//計算的為index位置，MapSize * 4 + 2 - 1
        //目的位置
        private int GoalLocation = 73;//計算的為index位置，MapSize * 5 + 9 - 1

        private int CurrentLocation;//目前位置

        //Current location 邊緣矩陣
        private double[,] OpenNode;
        //邊緣矩陣大小
        private int OpenSize = 0;
        //OPEN表保存了所有已生成而未考察的節點，CLOSED表中記錄已訪問過的節點。
        private double[,] ClosedNode;
        private int ClosedSize = 0;
        private int[] Map;

        /// <summary>
        /// ASTAT初始化
        /// </summary>
        /// <param name="MapSize">矩陣大小</param>
        /// <param name="StartLocation">起始位置</param>
        /// <param name="GoalLocation">目的位置</param>
        /// <param name="Map">地圖資料</param>
        public AStar_fake(int MapSize, int StartLocation, int GoalLocation, int[] Map)
        {
            this.MapSize = MapSize;
            this.StartLocation = StartLocation;
            this.GoalLocation = GoalLocation;
            this.OpenNode = new double[(int)Math.Pow(MapSize, 2) + 1, 4];
            this.ClosedNode = new double[(int)Math.Pow(MapSize, 2) + 1, 4];
            this.Map = Map;
        }

        public bool FindPath()
        {
            Initialize();

            while (!(CurrentLocation == GoalLocation))
            {
                //開始找路徑
                if (FindNewNode() == 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 初始化矩陣空間
        /// </summary>
        private void Initialize()
        {
            CurrentLocation = StartLocation;//把起始位置存放入目前位置
            OpenNode[0, 0] = CurrentLocation;//存放目前位置
            OpenNode[0, 1] = 0;//目前父結點位置
            OpenNode[0, 2] = GetDirectDistance(CurrentLocation, GoalLocation);//與目的位置的直線距離
            OpenNode[0, 3] = 0; //目前測出的 移動至此節點所需的最短步數

            //設為-1 代表未循覽過
            for (int i = 0; i <= (MapSize*MapSize); i++)
                for (int j = 0; j < 4; j++)
                    ClosedNode[i, j] = -1;

        }

        /// <summary>
        /// 找出路徑
        /// </summary>
        /// <returns></returns>
        private int FindNewNode()
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
            if (OpenNode[0, 0] == 0)
            {
                //在起始位置與結束位置間沒有任何的路徑
                //No route exists between start and goal.
                ReturnValue = 0;
            }
            else
            {
                //已經找到最後的位置GoalLocation
                if (OpenNode[0, 0] == GoalLocation)
                {
                    //Add node to ClosedList.
                    //增加目前的節點到拜訪過的矩陣中ClosedList
                    for (int i = 0; i < 4; i++)
                    {
                        ClosedNode[ClosedSize, i] = OpenNode[0, i];
                    }
                    //return RouteNode.
                    CurrentLocation = GoalLocation;
                    ReturnValue = CurrentLocation;
                }
                else//持續找尋
                {
                    ////表示OpenNode未被蒐尋過
                    //if (IsCloseNode((int)OpenNode[0, 0]) == false)//起始位置 
                    //{
                        //Add node to ClosedList.
                        //把起始位置放入已經搜尋過的矩陣內
                        for (int i = 0; i < 4; i++)
                        {
                            ClosedNode[ClosedSize, i] = OpenNode[0, i];
                        }

                        //取出目前的位置
                        CurrentLocation = (int)ClosedNode[ClosedSize, 0];

                        //以搜尋過的路徑增加
                        ClosedSize += 1;

                        //Remove node from Fringe().把目前的節點由Open矩陣內移除
                        RemoveNodeFromOpen(CurrentLocation);

                        //Add children to Open().計算Open矩陣內的3x3矩陣距離目標的長短，並排序之
                        TrySurroundingOpenNode(CurrentLocation);

                        //Return new CurrentLocation.
                        ReturnValue = CurrentLocation;
                    //}
                }

            }
            return ReturnValue;
        }

        /// <summary>
        /// 測試周邊節點是否可走
        /// </summary>
        /// <param name="ParentNode"></param>
        private void TrySurroundingOpenNode(int ParentNode)
        {
            //Check the ParentNode's surrounding nodes.
            int index = 0;
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    index = ParentNode + MapSize * i + j;
                    if (index >= 0 && index < MapSize * MapSize && Map[index] != 1 && Map[index] == 0)//表示非邊緣，且有路徑可以走
                    {
                        //Check to see if it's already in the ClosedList.
                        if (IsCloseNode(index) == false)
                        {
                            OpenSize += 1;
                            //Add child node to OpenList
                            OpenNode[OpenSize, 0] = index;//可以嘗試的路徑(目前結點)
                            OpenNode[OpenSize, 1] = ParentNode;//目前父結點位置
                            OpenNode[OpenSize, 2] = GetDirectDistance(index, GoalLocation);//計算目前結點與目標結點的歐幾里得距離 
                            OpenNode[OpenSize, 3] = OpenNode[ParentNode, 3] + 1; //目前消耗步數
                        }
                    }
                }
                //Start the next row down.
            }
            SortOpenNode();//排序矩陣
        }

        private double GetDirectDistance(int CurrentLoc, int GoalLoc)
        {
            int X1 = 0;
            int X2 = 0;
            int Y1 = 0;
            int Y2 = 0;
            double DX = 0;
            double DY = 0;

            X1 = CurrentLoc % MapSize;
            X2 = GoalLoc % MapSize;

            Y1 = (int)Math.Floor(CurrentLoc / (double)MapSize);
            Y2 = (int)Math.Floor(GoalLoc / (double)MapSize);

            DX = Math.Abs(X2 - X1);//X軸
            DY = Math.Abs(Y2 - Y1);//Y軸

            return Math.Sqrt(Math.Pow(DX, 2) + Math.Pow(DY, 2));

        }

        /// <summary>
        /// 檢查這個節點是否已被蒐尋過
        /// </summary>
        /// <param name="NodeIndex"></param>
        /// <returns></returns>
        private bool IsCloseNode(int NodeIndex)
        {
            for (int i = 0; i < ClosedSize; i++)
                if (ClosedNode[i, 0] == NodeIndex)
                    return true;
            return false;
        }

        /// <summary>
        /// 排序目前的Open矩陣，依據歐幾里得距離
        /// </summary>
        private void SortOpenNode()
        {
            //氣泡 升序
            //排序Open矩陣依據歐幾里得距離進行升序排序
            //Sort Open Matrix in ascending order of Distance + L2(smallest at start, largest at end).
            double[] TempArray = new double[4];

            for (int i = 0; i < OpenSize; i++)
            {
                for (int j = i + 1; j <= OpenSize; j++)
                {
                    if (OpenNode[i, 2] > OpenNode[j, 2])//如果i比較j大就交換
                    {
                        //Copy succeeding element from Fringe() to TempArray().
                        for (int k = 0; k < TempArray.Length; k++)
                        {
                            TempArray[k] = OpenNode[j, k];
                        }
                        //Copy Fringe() element to succeeding Fringe() element.
                        for (int k = 0; k < TempArray.Length; k++)
                        {
                            OpenNode[j, k] = OpenNode[i, k];
                        }
                        //Copy TempArray() to preceeding Fringe() element. 
                        for (int k = 0; k < TempArray.Length; k++)
                        {
                            OpenNode[i, k] = TempArray[k];
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
            double[,] TempArray = new double[OpenSize, 4];
            int k = 0;
            for (int i = 0; i <= OpenSize; i++)
            {
                if (OpenNode[i, 0] != Node)//open矩陣不包括CurrentLocation的結點
                {
                    for (int j = 0; j < 4; j++)//將open矩陣記錄到暫存矩陣TempArray
                    {
                        TempArray[k, j] = OpenNode[i, j];
                    }
                    k++;
                }
            }

            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    OpenNode[i, j] = TempArray[i, j];//將TempArray暫存矩陣轉存到矩陣open
                }
            }

            OpenSize = k - 1;
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
                TempArray[ClosedSize, i] = ClosedNode[ClosedSize, i];
            }

            //This loop follows the trail of 
            do
            {
                //from the Goal node.
                //All other nodes in ClosedList()
                for (int j = index; j >= 0; j += -1)
                {
                    //are dead-end nodes that the
                    if (ClosedNode[j, 0] == ClosedNode[index, 1])
                    {
                        //algorithm decided against.
                        index = j;

                        k--;
                        for (int i = 0; i < 2; i++)
                        {
                            TempArray[k, i] = ClosedNode[index, i];
                        }

                        if (ClosedNode[index, 0] == StartLocation)
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