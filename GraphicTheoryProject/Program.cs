using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GraphicTheoryProject
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                FileStream fs = new FileStream(@"C:\Users\35031\source\repos\GraphicTheoryProject\GraphicTheoryProject\user.csv", FileMode.Open);
                byte[] fileContent = new byte[fs.Length];
                char[] fileChar = new char[fs.Length / 2];
                fs.Read(fileContent, 0, (int)fs.Length);
                Decoder d = Encoding.UTF8.GetDecoder();
                d.GetChars(fileContent, 3, fileContent.Length - 3, fileChar, 0);
                Console.Write(fileChar);
                Console.WriteLine(d.GetHashCode());
            }
            catch(IOException e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadKey();
        }
    }
}
