using System;

namespace ConsoleAppCore31
{
    class Program
    {
        static void Main(string[] args)
        {
            ClassLibrary1.Class1 class1 = new ClassLibrary1.Class1();

            string str1 = "一二三四五六七";

            byte[] rawData = class1.GetBytes(str1, 950);
            Console.WriteLine(string.Join(",", rawData));

            string data = class1.GetString(rawData, 950);
            Console.WriteLine(data);

            Console.Read();
        }
    }
}
