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
        private int ID; //文档ID编号
        private int discretizatedID; //离散化编号
        private HashSet<int> reference; //参考文献的离散化编号存储

        private int cenDegree, cenDegreeB; //中心度，介度中心度

        public Article(int ID_, int disID_)
        {
            reference = new HashSet<int>();
            ID = ID_;
            discretizatedID = disID_;
            cenDegree = cenDegreeB = 0;
        }

        public void IncreaseCenDegree(int increaseBy, bool isBType)
        {
            if (isBType) cenDegreeB += increaseBy;
            else cenDegree += increaseBy;
        }

        public HashSet<int> GetReference()
        {
            return reference;
        }
        public bool AddReference(int refDiscretizatedID)
        {
            return reference.Add(refDiscretizatedID);
        }
        public override string ToString()
        {
            return ID.ToString() + '+' + discretizatedID.ToString();
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

        static void Main(string[] args)
        {
            try
            {
                StreamReader sr = new StreamReader(@"C:\Users\35031\source\repos\GraphicTheoryProject\GraphicTheoryProject\data\paper.csv", Encoding.UTF8, true);
                ReadIntFromFile(sr.ReadToEnd(), out List<int> listArticleID);
                DiscretizateArticleID(listArticleID, out Dictionary<int, int> dictionaryDiscretization);
                InitialArticle(listArticleID, out Article[] articleSet);
                foreach(var article in articleSet)
                {
                    Console.WriteLine(article.ToString());
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
