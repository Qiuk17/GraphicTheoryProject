using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GraphicTheoryProject
{

    class MovieType
    {
        private string movieType;
        private HashSet<Movie> movieOfThisType;
    }

    class Movie
    {
        
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (StreamReader sr = new StreamReader(@"C:\Users\35031\source\repos\GraphicTheoryProject\GraphicTheoryProject\movie.csv", Encoding.UTF8, true))
            {
                Console.Write(sr.ReadToEnd());

            }

            Console.ReadKey();
        }
    }
}
