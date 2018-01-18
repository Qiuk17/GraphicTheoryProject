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
            public HashSet<int> succs;
            public Node() { succs = new HashSet<int>(); }
        }
        private Node[] nodes;
        public void InitGraph(Article[] articles)
        {
            nodes = new Node[articles.Length];
            for (int i = 0; i < articles.Length; i++)
                nodes[i] = new Node();
            foreach(var article in articles)
            {
                foreach (var refDiscretizatedID in article.Reference)
                {
                    nodes[article.DiscretizatedID].succs.Add(refDiscretizatedID);
                    nodes[refDiscretizatedID].succs.Add(article.DiscretizatedID);
                }
            }
        }
        public void GetSingleSourceShortestPath(int source, out int[] Path) 
        {
            List<int>[] SourcedPath = new List<int>[nodes.Length];
            Path = new int[nodes.Length];
            int[] d = new int[nodes.Length];
            for(int i = 0; i < nodes.Length; i++)
            {
                d[i] = 500000; Path[i] = -1;
            }
            Queue<int> Q = new Queue<int>();
            Q.Enqueue(source);
            d[source] = 0;
            while(Q.Count != 0)
            {
                int u = Q.Dequeue();
                foreach(var nextNode in nodes[u].succs)
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
                StreamReader sr = new StreamReader(@"C:\Users\35031\source\repos\GraphicTheoryProject\GraphicTheoryProject\data\paper.csv", Encoding.UTF8, true);
                string FileContent = sr.ReadToEnd();
                ReadIntFromFile(FileContent, out List<int> listArticleID);
                DiscretizateArticleID(listArticleID, out Dictionary<int, int> dictionaryDiscretization);
                InitialArticle(listArticleID, out Article[] articleSet);
                AddReferenceToArticle(ref articleSet, FileContent, dictionaryDiscretization);
                Graph test = new Graph();
                test.InitGraph(articleSet);
                for(int i = 0; i < articleSet.Length; i++)
                {
                    Console.WriteLine(i.ToString() + " Path:");
                    test.GetSingleSourceShortestPath(i, out int[] Path);
                    //foreach (var j in Path) Console.Write(j.ToString() + ' ');
                    //Console.WriteLine();
                }
                Console.ReadKey();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
            
        }
    }
}
