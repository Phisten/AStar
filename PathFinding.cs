
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace PathFinding
{
    public class AStar
    {
        public static int FindPath(int startNodeIndex, int endNodeIndex, bool AllowDiagonal, ref AStarMap map, out List<AStarNode> bestPath)
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
            map.SetAllNodeRisk(endNodeIndex);

            openList.Add(map.GetNodeFromIndex(startNodeIndex));
            openList[0].Cost = 0;
            //openList[0].Risk = map.GetGridDistance(startNodeIndex, endNodeIndex);
            openList[0].TotalCost = openList[0].Risk;
            openList[0].State = AStarNode.NodeState.Open;

            int PathSearchResult = PathTestLoop(endNodeIndex, map, openList, closedList, AllowDiagonal);

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

        private static int PathTestLoop(int endNodeIndex, AStarMap map, List<AStarNode> openList, List<AStarNode> closedList, bool AllowDiagonal)
        {

            List<int> X_PathList;
            List<int> Y_PathList;
            if (AllowDiagonal)
            {
                X_PathList = new List<int> { -1, 0, 1, -1, 0, 1, -1, 0, 1 };
                Y_PathList = new List<int> { -1, -1, -1, 0, 0, 0, 1, 1, 1 };
            }
            else
            {
                X_PathList = new List<int> { 0, -1, 0, 1, 0 };
                Y_PathList = new List<int> { -1, 0, 0, 0, 1 };
            }
            int DirectCount = X_PathList.Count;


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

                for (int i = 0; i < DirectCount; i++)
                {
                    int X = curNode.X + X_PathList[i];
                    int Y = curNode.Y + Y_PathList[i];
                    if (X >= 0 && X < map.Width && Y >= 0 && Y < map.Height) //未超過地圖邊界
                    {
                        AStarNode newNodeTmp = map[X][Y];
                        if (newNodeTmp.Value == 0)  //Value 0 代表可通過 無障礙物
                        //&& newNodeTmp != curNode  //排除Comparison by self
                        //&& curNode.ParentNode != newNodeTmp)
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

                                    if (newNodeTmp.TotalCost <= curNode.Cost + 1 + curNode.Risk)
                                    {
                                        continue;
                                    }
                                    //if (newNodeTmp.Cost <= curNode.Cost + 1)
                                    //{

                                    //}
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
                return SetAllNodeRisk(targetIndex % Width, targetIndex / Height);
            }
            public int SetAllNodeRisk(int targetX, int targetY)
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
                childNode.TotalCost = childNode.Cost + childNode.Risk;
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

}