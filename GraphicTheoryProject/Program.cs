using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GraphicTheoryProject
{

    class article
    {
        private int ID; //文档ID编号
        private int discretizatedID; //离散化编号
        private HashSet<int> reference; //参考文献的离散化编号存储

        private int cenDegree, cenDegreeB; //中心度，介度中心度
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

    }

    class Program
    {
        static void Main(string[] args)
        {
            using (StreamReader sr = new StreamReader(@"C:\Users\35031\source\repos\GraphicTheoryProject\GraphicTheoryProject\data\paper.csv", Encoding.UTF8, true))
            {
                while (!sr.EndOfStream)
                    System.Console.Write(sr.ReadLine());
                

            }

            Console.ReadKey();
        }
    }
}
