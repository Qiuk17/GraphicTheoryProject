using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GraphicTheoryProject
{
    class Movie
    {
        private int nameHashCode;
        private int typeHashCode;
        private string movieName;
        private string movieType;
        private double movieMark;
        public Movie(string name, string type, double mark) 
        {
            movieName = name;
            movieType = type;
            nameHashCode = name.GetHashCode();
            typeHashCode = type.GetHashCode();
            movieMark = mark;
        }
        public string GetMovieName()
        {
            return movieName;
        }
        public string GetMovieTyoe()
        {
            return movieType;
        }
        public double GetMovieMark()
        {
            return movieMark;
        }
        public override int GetHashCode()
        {
            
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            using (StreamReader sr = new StreamReader(@"C:\Users\35031\source\repos\GraphicTheoryProject\GraphicTheoryProject\movie.csv", Encoding.UTF8, true))
            {
                Console.Write(sr.ReadToEnd());
                Movie movie1 = new Movie("resr", "resr", 9.922);
                Movie movie2 = new Movie("resr", "resr", 9.922);
                Console.Write(movie1.GetHashCode());
                Console.Write(movie2.GetHashCode());
            }

            Console.ReadKey();
        }
    }
}
