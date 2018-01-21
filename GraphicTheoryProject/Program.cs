using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GraphicTheoryProject
{

    class Article
    {
        public int ID { get; } //文档ID编号
        public int DiscretizatedID { get; } //离散化编号
        public HashSet<int> Reference { get; } //参考文献的离散化编号存储

        public Article(int ID_, int disID_)
        {
            Reference = new HashSet<int>();
            ID = ID_;
            DiscretizatedID = disID_;
        }

        public bool AddReference(int refDiscretizatedID)
        {
            return Reference.Add(refDiscretizatedID);
        }
        public override string ToString()
        {
            string ret = ID.ToString() + '\n';
            foreach(var seg in Reference)
            {
                ret += seg.ToString() + ' ';
            }
            return ret;
        }
    }

    class GraphGenerator
    {
        class Graph //用于计算的图
        {
            private class Node //图的节点
            {
                public int ID { get; }
                public HashSet<int> Succs { get; set; }
                public int CenDegreeB { get; set; } //该节点出现在其他节点对上最短路径上的次数（不包括两个端点，大-重要）
                public int CenDegreeA { get; set; } //该节点到其他所有联通的节点的最短路径之和（小-重要）
                public int NumReachTo { get; set; } //该节点可以联通的节点数量（不包括自身）
                public Node(int id) { ID = id; Succs = new HashSet<int>(); CenDegreeA = CenDegreeB = NumReachTo = 0; }
                public override string ToString()
                {
                    return CenDegreeA.ToString() + ' ' + CenDegreeB.ToString() + ' ' + NumReachTo.ToString();
                }
            }
            private Node[] Nodes;
            public int GetNodeCenDegreeA(int NodeID)
            {
                return Nodes[NodeID].CenDegreeA;
            }
            public int GetNodeCenDegreeB(int NodeID)
            {
                return Nodes[NodeID].CenDegreeB;
            }
            public int GetReachedNum(int NodeID)
            {
                return Nodes[NodeID].NumReachTo;
            }
            public bool IsLinked(int NodeIDA, int NodeIDB)
            {
                return Nodes[NodeIDA].Succs.Contains(NodeIDB);
            }
            public int NodeNum { get => Nodes.Length; }
            public void InitGraph(Article[] articles)
            {
                Nodes = new Node[articles.Length];
                for (int i = 0; i < articles.Length; i++)
                {
                    Nodes[i] = new Node(i);
                }

                foreach (var article in articles)
                {
                    foreach (var refDiscretizatedID in article.Reference)
                    {
                        Nodes[article.DiscretizatedID].Succs.Add(refDiscretizatedID);
                        Nodes[refDiscretizatedID].Succs.Add(article.DiscretizatedID);
                    }
                }
                GraphAnalysis();
                for(int i = 0; i < Nodes.Length; i++)
                {
                    if (Nodes[i].CenDegreeA == 0) Nodes[i].CenDegreeA = 100000000;
                    if (Nodes[i].CenDegreeB == 0) Nodes[i].CenDegreeB = 1;
                }
            }


            public void GetSingleSourceShortestPath(int source, out int[] Path)
            {
                Path = new int[Nodes.Length];
                int[] d = new int[Nodes.Length];
                for (int i = 0; i < Nodes.Length; i++)
                {
                    d[i] = 500000; Path[i] = -1;
                }
                Queue<int> Q = new Queue<int>();
                Q.Enqueue(source);
                d[source] = 0;
                while (Q.Count != 0)
                {
                    int u = Q.Dequeue();
                    foreach (var nextNode in Nodes[u].Succs)
                    {
                        if (1 + d[u] < d[nextNode])
                        {
                            d[nextNode] = 1 + d[u];
                            Path[nextNode] = u;
                            Q.Enqueue(nextNode);
                        }
                    }
                }
            }

            public void GetMiniSpanTree(out int[] Fa)
            {
                Fa = new int[Nodes.Length];
                bool[] vis = new bool[Nodes.Length];
                for(int i = 0; i < Nodes.Length; i++)
                {
                    Fa[i] = -1; vis[i] = false;
                }
                for(int i = 0; i < Nodes.Length; i++)
                {
                    if(!vis[i])
                    {
                        DFS(i, ref vis, ref Fa, -1);
                    }
                }
            }
            private void DFS(int u, ref bool[] vis, ref int[] Fa, int fa)
            {
                vis[u] = true;
                Fa[u] = fa;
                foreach(var v in Nodes[u].Succs)
                {
                    if(!vis[v])
                    {
                        DFS(v, ref vis, ref Fa, u);
                    }
                }
            }

            public void GetImportanceSortedNodes(out int[] OrderedNodes)
            {
                OrderedNodes = new int[Nodes.Length];
                List<Node> list = new List<Node>(Nodes);
                Node[] orderedNodes = list.OrderBy(i => i.CenDegreeA / (double)i.CenDegreeB).ToArray();
                for(int i = 0; i < Nodes.Length; i++)
                {
                    OrderedNodes[i] = orderedNodes[i].ID;
                }
            }

            public void GraphAnalysis()
            {
                for (int i = 0; i < Nodes.Length; i++)
                {
                    GetSingleSourceShortestPath(i, out int[] Path);
                    for (int j = 0; j < Nodes.Length; j++)
                    {
                        if (Path[j] == -1)
                        {
                            continue;
                        }
                        else
                        {
                            Nodes[i].NumReachTo++;
                            for (int u = Path[j]; Path[u] != -1; u = Path[u])
                            {
                                Nodes[i].CenDegreeA++;
                                Nodes[u].CenDegreeB++;
                            }
                        }
                    }
                }
            }
            
        }

        class DrawGraph //输出用于绘图的信息交给Processing 3绘图
        {
            private DrawPoint[] drawPoints;
            public DrawPoint[] DrawPoints => drawPoints;
            private StreamWriter streamWriter;
            public DrawGraph()
            {
                try
                {
                    streamWriter = new StreamWriter(@"res.txt", false, Encoding.UTF8);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.GetType());
                    Console.ReadKey();
                    Environment.Exit(1);
                }

            }

            public class DrawPoint
            {
                public DrawPoint(int posX, int posY)
                {
                    PosX = posX;
                    PosY = posY;
                }

                public int PosX { get; set; } //横坐标
                public int PosY { get; set; } //纵坐标
                public override string ToString()
                {
                    return PosX.ToString() + ' ' + PosY.ToString();
                }
            }

            public class Color
            {
                public Color(int r, int g, int b)
                {
                    R = r;
                    G = g;
                    B = b;
                }
                public Color(int RGB) //用于生成渐变色
                {
                    if (RGB <= 255)
                    {
                        R = 255; G = RGB; B = 0;
                    }
                    else if (RGB <= 510)
                    {
                        R = 510 - RGB;
                        G = 255; B = 0;
                    }
                    else if (RGB <= 765)
                    {
                        R = 0;
                        G = 255; B = RGB - 510;
                    }
                    else if (RGB <= 1020)
                    {
                        R = 0;
                        G = 1020 - RGB;
                        B = 255;
                    }
                    else if (RGB > 1020)
                    {
                        R = 0;
                        G = 255;
                        B = 255;
                    }
                }
                public int R { get; set; }
                public int G { get; set; }
                public int B { get; set; }
                public override string ToString()
                {
                    return R.ToString() + ' ' + G.ToString() + ' ' + B.ToString();
                }
            }

            private class Vector
            {
                public double DiractionX, DiractionY;
                public double GetLength()
                {
                    return Math.Sqrt(DiractionX * DiractionX + DiractionY * DiractionY);
                }
                public Vector(double diractionX, double diractionY)
                {
                    if (double.IsNaN(diractionX)) diractionX = 1e9;
                    if (double.IsNaN(diractionY)) diractionY = 1e9;
                    DiractionX = diractionX;
                    DiractionY = diractionY;
                }
                public Vector(double diractionX, double diractionY, double length)
                {
                    //Console.WriteLine("using {0},{1} L={2}", diractionX, diractionY, length);
                    if (double.IsNaN(length)) length = 1e9;
                    DiractionX = diractionX; DiractionY = diractionY;
                    double LL = length / GetLength();
                    DiractionX *= LL;
                    DiractionY *= LL;
                    //Console.WriteLine("We get ({0},{1})", DiractionX, DiractionY);
                }
                static public Vector operator +(Vector lhs, Vector rhs)
                {
                    return new Vector(lhs.DiractionX + rhs.DiractionX, lhs.DiractionY + rhs.DiractionY);
                }
            }

            private class Point
            {
                public double x, y;
                private static double maxX = 1920.0, maxY = 1080.0;
                public static double MaxX => maxX;
                public static double MaxY => maxY;

                public Point(double _x, double _y)
                {
                    x = _x; y = _y;
                }
                public Point(ref Random r)
                {
                    x = r.NextDouble() * maxX;
                    y = r.NextDouble() * maxY;
                }
                public void SetIntoRange()
                {
                    x = Math.Max(10.0, x);
                    x = Math.Min(maxX, x);
                    y = Math.Max(10.0, y);
                    y = Math.Min(maxY, y);
                }
                public static Vector operator -(Point lhs, Point rhs)
                {
                    return new Vector(rhs.x - lhs.x, rhs.y - lhs.y);
                }

                public static Point operator +(Point lhs, Vector rhs)
                {
                    return new Point(lhs.x + rhs.DiractionX, lhs.y + rhs.DiractionY);
                }
            }

            public void GraphGen(Graph graph) //将代数意义上的结点映射到二维平面上的坐标
            {
                Point[] points = new Point[graph.NodeNum];
                Random r = new Random();
                for (int i = 0; i < graph.NodeNum; i++)
                {
                    points[i] = new Point(ref r);
                }

                bool[][] g = new bool[points.Length][];
                for (int i = 0; i < points.Length; i++)
                {
                    g[i] = new bool[points.Length];
                }

                for (int i = 0; i < points.Length; i++)
                {
                    for (int j = i + 1; j < points.Length; j++)
                    {
                        g[i][j] = g[j][i] = graph.IsLinked(i, j);
                    }
                }

                const double MinT = 1E-4, Delta = 1 - 1E-1;
                double k = Math.Sqrt(1920.0 * 1080.0 / points.Length);
                Console.Write("正在计算结点的坐标");
                for (double T = k / 2; T > MinT; T *= Delta)
                {
                    Console.Write('.');
                    //Console.ReadKey();

                    Vector[] PointsDelta = new Vector[points.Length];
                    Parallel.For(0, points.Length, i =>
                    {
                        PointsDelta[i] = new Vector(0.0, 0.0);
                        for (int j = 0; j < points.Length; j++)
                        {
                            if (i == j) continue;
                            double delX = points[j].x - points[i].x, delY = points[j].y - points[i].y;
                            double SqrVecLen = delX * delX + delY * delY;
                            PointsDelta[i] = PointsDelta[i] + new Vector(-delX, -delY, k * k / Math.Sqrt(SqrVecLen));
                            if (g[i][j]) PointsDelta[i] = PointsDelta[i] + new Vector(delX, delY, SqrVecLen / k);
                            //Console.WriteLine("({0},{1})", PointsDelta[i].DiractionX, PointsDelta[i].DiractionY);
                            //Console.WriteLine("From ({0},{1}) to ({2},{3}) deltaX={4} deltaY={5} Length={6}", points[i].x, points[i].y, points[j].x, points[j].y, delX, delY, Math.Sqrt(SqrVecLen));
                        }
                    });
                    Parallel.For(0, points.Length, i =>
                    {
                        //Console.Write("({0},{1}) + vec({2},{3}) is ", points[i].x, points[i].y, PointsDelta[i].DiractionX, PointsDelta[i].DiractionY);
                        if (PointsDelta[i].GetLength() > T) PointsDelta[i] = new Vector(PointsDelta[i].DiractionX, PointsDelta[i].DiractionY, T);
                        points[i] = points[i] + PointsDelta[i];
                        //Console.WriteLine("({0},{1})", points[i].x, points[i].y);
                        points[i].SetIntoRange();
                    });
                }

                drawPoints = new DrawPoint[points.Length];
                for (int i = 0; i < points.Length; i++)
                    drawPoints[i] = new DrawPoint((int)points[i].x, (int)points[i].y);
                Console.Clear();
            }

            public void PrintLine(int StartPointID, int EndPointID, Color color)
            {
                streamWriter.WriteLine(drawPoints[StartPointID].ToString() + ' ' + drawPoints[EndPointID].ToString() + ' ' + color.ToString());
            }
            public void PrintCircle(int CircleCenterID, int Radius, Color color)
            {
                streamWriter.WriteLine(drawPoints[CircleCenterID].ToString() + ' ' + Radius.ToString() + ' ' + color.ToString());
            }

            public void EndPrint()
            {
                streamWriter.Close();
            }
        }

        Graph graph;
        DrawGraph drawGraph;

        public GraphGenerator(Article[] articleSet)
        {
            graph = new Graph();
            graph.InitGraph(articleSet);
            drawGraph = new DrawGraph();
            drawGraph.GraphGen(graph);
        }
        private void PrintAllGraphT(int ColorGray)
        {
            for(int i = 0; i < drawGraph.DrawPoints.Length; i++)
            {
                for(int j = i + 1; j < drawGraph.DrawPoints.Length; j++)
                {
                    if (graph.IsLinked(i, j))
                    {
                        drawGraph.PrintLine(i, j, new DrawGraph.Color(ColorGray, ColorGray, ColorGray));
                    }
                }
            }
        }

        public void PrintGraphBase()
        {
            PrintAllGraphT(200);
        }

        private static void ToGraph()
        {
            System.Diagnostics.Process np = System.Diagnostics.Process.Start("sketch.exe");
            np.WaitForExit();
        }

        public void PrintAllGraph()
        {
            PrintAllGraphT(0);
            drawGraph.EndPrint();
            ToGraph();
        }
        public void PrintShortestPath(Dictionary<int, int> dictionary)
        {
            PrintGraphBase();
            Console.Write("请输入起点的编号:");
            bool rtn1 = int.TryParse(Console.ReadLine(), out int StartPos);
            Console.Write("请输入终点的编号:");
            bool rtn2 = int.TryParse(Console.ReadLine(), out int EndPos);
            if(rtn1 && rtn2 && dictionary.ContainsKey(StartPos) && dictionary.ContainsKey(EndPos))
            {
                int StartPointID = dictionary[StartPos], EndPointID = dictionary[EndPos];
                graph.GetSingleSourceShortestPath(EndPointID, out int[] Path);
                if(Path[StartPointID] == -1)
                {
                    Console.WriteLine("路径不存在，两点之间不连通。");
                    Console.ReadKey();
                }
                else
                {
                    for(int p = StartPointID, q = Path[StartPointID]; q != -1; p = Path[p], q = Path[q])
                    {
                        drawGraph.PrintLine(p, q, new DrawGraph.Color(0, 0, 0));
                        drawGraph.PrintCircle(p, 10, new DrawGraph.Color(0, 0, 0));
                        drawGraph.PrintCircle(q, 10, new DrawGraph.Color(0, 0, 0));
                    }
                    drawGraph.EndPrint();
                    ToGraph();
                }
            }
            else
            {
                Console.WriteLine("输入有误，请确认后再输入。");
                Environment.Exit(1);
            }
        }
        public void PrintMiniSpanTree()
        {
            PrintGraphBase();
            graph.GetMiniSpanTree(out int[] Fa);
            for(int i = 0; i < Fa.Length; i++)
            {
                drawGraph.PrintCircle(i, 10, new DrawGraph.Color(0, 0, 0));
                if(Fa[i] != -1)
                {
                    drawGraph.PrintLine(i, Fa[i], new DrawGraph.Color(0, 0, 0));
                }
            }
            drawGraph.EndPrint();
            ToGraph();
        }
        public void PrintImportance()
        {
            PrintGraphBase();
            graph.GetImportanceSortedNodes(out int[] OrderedNodes);
            for (int i = 0; i < OrderedNodes.Length; i++)
            {
                drawGraph.PrintCircle(OrderedNodes[i], 10, new DrawGraph.Color((int)Math.Round(i * (1050 / (double)OrderedNodes.Length))));
            }
            drawGraph.EndPrint();
            ToGraph();
        }
    }

    class Program
    {
        static void ReadIntFromFile(string str, out List<int> array)
        {
            int prec = -1;
            array = new List<int>();
            List<int> listCache = new List<int>();
            char[] splitChars = new char[] { ',', ';', ' ', '\n' };
            string[] intS = str.Split(splitChars);
            for (int i = 0; i < intS.Length; i++) //Console.WriteLine(intS[i]);
            {
                if(int.TryParse(intS[i], out int intCache)) listCache.Add(intCache);
            }
            listCache.Sort();
            foreach(int intCache in listCache)
            {
                if (intCache != prec) array.Add(intCache);
                prec = intCache;
            }
        }

        static void DiscretizateArticleID(List<int> orderedIDList, out Dictionary<int, int> dictionaryDiscretization)
        {
            dictionaryDiscretization = new Dictionary<int, int>();
            int cnt = 0;
            foreach(int ID in orderedIDList)
            {
                dictionaryDiscretization.Add(ID, cnt++);
            }
        }

        static void InitialArticle(List<int> orderedIDList, out Article[] articleSet)
        {
            articleSet = new Article[orderedIDList.Count];
            int cnt = 0;
            foreach(int ID in orderedIDList)
            {
                articleSet[cnt] = new Article(ID, cnt++);
            }
        }
        
        static void AddReferenceToArticle(ref Article[] articles, string FileContent, Dictionary<int, int> dictionary)
        {
            foreach (string Line in FileContent.Split(new char[] { '\n' }))
            {
                string[] segments = Line.Split(new char[] { ',', ';' });
                if(int.TryParse(segments[0], out int articleID))
                {
                    int discretizatedArticleID;
                    if(dictionary.ContainsKey(articleID))
                    {
                        discretizatedArticleID = dictionary[articleID];
                        for(int i = 1; i < segments.Length; i++)
                        {
                            if(int.TryParse(segments[i], out int refID))
                            {
                                if(dictionary.ContainsKey(refID))
                                {
                                    articles[discretizatedArticleID].AddReference(dictionary[refID]);
                                }
                            }
                        }
                    }
                }
            }
        }

        static void UserGuide(out string FilePath, out int DisplayMode)
        {
            Console.WriteLine("本程序采用数据集中paper.csv中的部分数据，如果你想要更好的画图效果，请使用规模更小（结点数目小于100）的数据。\n输入1则使用paper.csv，输入其它则自定义数据。");
            Console.Write("请输入：");
            
            FilePath = "paper.csv";
            if(Console.ReadKey().KeyChar != '1')
            {
                Console.Clear();
                Console.Write("请按照如下方式构造数据：每一行代表一个结点，每一行应该写成如下形式\n{num1},{num2};{num3};{num4}......{numN}\n表示编号为num1的结点与编号为num2、num3……numN的结点相连\n例如“12,1;2;3;4;3242”\n需要注意的是结点编号不需要连续，但是也不能超过16位无符号整型\n构造的文件请使用UTF-8编码。\n请输入文件名称（完整路径、相对路径均可）：");
                FilePath = Console.ReadLine(); 
            }
            Console.Clear();
            DisplayMode = 1;
            Console.Write("请输入输出的类型（1：最短路； 2：最小生成树； 3.结点关键程度； 4：图完整输出（规模较大时不推荐））:");
            DisplayMode = int.Parse(Console.ReadKey().KeyChar.ToString());
            Console.WriteLine("\n你选择的文件是{0}，输出类型为{1}.\n按任意键继续.", FilePath, DisplayMode);
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("请按提示来操作本程序。\nPress any key to continue.");
            Console.ReadKey();
            try
            {
                Console.Clear();
                UserGuide(out string FilePath, out int DisplayMode);

                StreamReader sr = new StreamReader(FilePath, Encoding.UTF8, true);
                string FileContent = sr.ReadToEnd();

                ReadIntFromFile(FileContent, out List<int> listArticleID);
                DiscretizateArticleID(listArticleID, out Dictionary<int, int> dictionaryDiscretization);
                InitialArticle(listArticleID, out Article[] articleSet);
                AddReferenceToArticle(ref articleSet, FileContent, dictionaryDiscretization);

                //Graph graph = new Graph();
                //graph.InitGraph(articleSet);
                //graph.GraphAnalysis();

                //DrawGraph drawGraph = new DrawGraph();
                //drawGraph.GraphGen(graph);
                GraphGenerator graphGenerator = new GraphGenerator(articleSet);
                if(DisplayMode == 1)
                {
                    graphGenerator.PrintShortestPath(dictionaryDiscretization);
                }
                else if(DisplayMode == 2)
                {
                    graphGenerator.PrintMiniSpanTree();
                }
                else if(DisplayMode == 3)
                {
                    graphGenerator.PrintImportance();
                }
                else if(DisplayMode == 4)
                {
                    graphGenerator.PrintAllGraph();
                }
                else
                {
                    Console.Write("输入有误");
                    Console.ReadKey();
                }
                //Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType().ToString());
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }

        }
    }
}
