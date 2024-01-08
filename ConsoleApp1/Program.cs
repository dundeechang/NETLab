namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string text = "國泰證券是一間好公司。";
            Console.ForegroundColor = ConsoleColor.Yellow;
            while (true)
            {
                Console.Clear();
                Console.Write(text);
                text = text[1..] + text[0];
                Thread.Sleep(200);
            }
        }
    }
}
