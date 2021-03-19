using System;
using System.IO;

namespace TestCopy
{
    class Program
    {
        static void Main(string[] args)
        {
            
            string source = "c:\\tmp\\1.mkv"; 
            string dest1 = "C:\\tmp\\2.mkv";
            string dest2 = "c:\\tmp\\3.mkv";
            
            FileManager fm = new FileManager();
            fm.Progress = true;
            fm.Test(source, dest1, dest2);
            
            Console.WriteLine("done!");
            Console.ReadKey();
        }
    }
}
