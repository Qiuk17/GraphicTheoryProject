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

    class Graph //用于计算的图
    {
        private class Node //图的节点
        {
            public HashSet<int> Succs { get; set; }
            public int CenDegreeA { get; set; } //该节点出现在其他节点对上最短路径上的次数（不包括两个端点）
            public int CenDegreeB { get; set; } //该节点到其他所有联通的节点的最短路径之和
            public int NumReachTo { get; set; } //该节点可以联通的节点数量（不包括自身）
            public Node() { Succs = new HashSet<int>(); CenDegreeA = CenDegreeB = NumReachTo = 0; }
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
                Nodes[i] = new Node();
            foreach(var article in articles)
            {
                foreach (var refDiscretizatedID in article.Reference)
                {
                    Nodes[article.DiscretizatedID].Succs.Add(refDiscretizatedID);
                    Nodes[refDiscretizatedID].Succs.Add(article.DiscretizatedID);
                }
            }
        }
        public void GetSingleSourceShortestPath(int source, out int[] Path) 
        {
            List<int>[] SourcedPath = new List<int>[Nodes.Length];
            Path = new int[Nodes.Length];
            int[] d = new int[Nodes.Length];
            for(int i = 0; i < Nodes.Length; i++)
            {
                d[i] = 500000; Path[i] = -1;
            }
            Queue<int> Q = new Queue<int>();
            Q.Enqueue(source);
            d[source] = 0;
            while(Q.Count != 0)
            {
                int u = Q.Dequeue();
                foreach(var nextNode in Nodes[u].Succs)
                {
                    if(1 + d[u] < d[nextNode])
                    {
                        d[nextNode] = 1 + d[u];
                        Path[nextNode] = u;
                        Q.Enqueue(nextNode);
                    }
                }
            }
        }
        
        public void GraphAnalysis()
        {
            for(int i = 0; i < Nodes.Length; i++)
            {
                GetSingleSourceShortestPath(i, out int[] Path);
                for(int j = 0; j < Nodes.Length; j++)
                {
                    if (Path[j] == -1) continue;
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
        private int Width, Height;//图片宽度、高度
        private List<DrawCircle> Circles;
        private List<DrawLine> Lines;

        public DrawGraph()
        {
            Width = 1920; Height = 1080;
            Circles = new List<DrawCircle>();
            Lines = new List<DrawLine>();
        }

        private class DrawPoint
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

        private class Color
        {
            public Color(int r, int g, int b)
            {
                R = r;
                G = g;
                B = b;
            }
            public Color (int RGB) //用于生成渐变色
            {
                if(RGB <= 255)
                {
                    R = 255; G = RGB; B = 0;
                }
                else if(RGB <= 510)
                {
                    R = 510 - RGB;
                    G = 255; B = 0;
                }
                else if(RGB <= 765)
                {
                    R = 0;
                    G = 255; B = RGB - 510;
                }
                else if(RGB <= 1020)
                {
                    R = 0;
                    G = 1020 - RGB;
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

        private class DrawLine
        {
            private DrawPoint StartPoint, EndPoint;
            private Color StrokeColor;
            public DrawLine(int x1, int y1, int x2, int y2, int R, int G, int B)
            {
                StrokeColor = new Color(R, G, B);
                StartPoint = new DrawPoint(x1, y1);
                EndPoint = new DrawPoint(x2, y2);
            }
            public override string ToString()
            {
                return StartPoint.ToString() + ' ' + EndPoint.ToString() + ' ' + StrokeColor.ToString();
            }
        }

        private class DrawCircle
        {
            private DrawPoint CircleCenter;
            private Color StrokeColor;
            public DrawCircle(int x, int y, int RGB)
            {
                CircleCenter = new DrawPoint(x, y);
                StrokeColor = new Color(RGB);
            }
            public override string ToString()
            {
                return CircleCenter.ToString() + ' ' + StrokeColor.ToString();
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
            static public Vector operator + (Vector lhs, Vector rhs)
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
                x = Math.Max(0.0, x);
                x = Math.Min(maxX, x);
                y = Math.Max(0.0, y);
                y = Math.Min(maxY, y);
            }
            public static Vector operator - (Point lhs, Point rhs)
            {
                return new Vector(rhs.x - lhs.x, rhs.y - lhs.y);
            }
            
            public static Point operator + (Point lhs, Vector rhs)
            {
                return new Point(lhs.x + rhs.DiractionX, lhs.y + rhs.DiractionY);
            }
        }

        public void GraphGen(Graph graph)
        {
            Point[] points = new Point[graph.NodeNum];
            Random r = new Random();
            for(int i = 0; i < graph.NodeNum; i++) points[i] = new Point(ref r);
            bool[][] g = new bool[points.Length][];
            for (int i = 0; i < points.Length; i++) g[i] = new bool[points.Length];
            for (int i = 0; i < points.Length; i++)
                for (int j = i + 1; j < points.Length; j++)
                    g[i][j] = g[j][i] = graph.IsLinked(i, j);
            const double MinT = 1E-4, Delta = 1 - 1E-1;
            double k = Math.Sqrt(1920.0*1080.0/ points.Length);
            for(double T = k/2; T > MinT; T *= Delta)
            {
                Console.WriteLine(T);
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
                       PointsDelta[i] = PointsDelta[i] + new Vector(-delX, -delY, k*k/Math.Sqrt(SqrVecLen));
                       if (g[i][j]) PointsDelta[i] = PointsDelta[i] + new Vector(delX, delY, SqrVecLen/k);
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
            StreamWriter sw = new StreamWriter(@"res.txt", true, Encoding.UTF8);
            DrawPoint[] drawPoints = new DrawPoint[points.Length];
            for (int i = 0; i < points.Length; i++)
                drawPoints[i] = new DrawPoint((int)points[i].x, (int)points[i].y);
            for (int i = 0; i < points.Length; i++)
                for (int j = i + 1; j < points.Length; j++)
                    if (g[i][j]) { sw.WriteLine(drawPoints[i].ToString() + ' ' + drawPoints[j].ToString());}
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
                dictionaryDiscretization.Add(ID, cnt++);
        }

        static void InitialArticle(List<int> orderedIDList, out Article[] articleSet)
        {
            articleSet = new Article[orderedIDList.Count];
            int cnt = 0;
            foreach(int ID in orderedIDList)
                articleSet[cnt] = new Article(ID, cnt++);
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

        static void Main(string[] args)
        {
            try
            {
                StreamReader sr = new StreamReader(@"paper.csv", Encoding.UTF8, true);
                string FileContent = sr.ReadToEnd();
                ReadIntFromFile(FileContent, out List<int> listArticleID);
                DiscretizateArticleID(listArticleID, out Dictionary<int, int> dictionaryDiscretization);
                InitialArticle(listArticleID, out Article[] articleSet);
                AddReferenceToArticle(ref articleSet, FileContent, dictionaryDiscretization);
                Graph test = new Graph();
                test.InitGraph(articleSet);
                test.GraphAnalysis();
                for(int i = 0; i < articleSet.Length; i++)
                {
                    Console.WriteLine(test.GetNodeCenDegreeA(i).ToString() + ' ' + test.GetNodeCenDegreeB(i).ToString() + ' ' + test.GetReachedNum(i).ToString());
                }
                DrawGraph test1 = new DrawGraph();
                test1.GraphGen(test);

                Console.ReadKey();
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
