using System;
namespace ClassAssistantBot.Services
{
    public static class Logger
    {
        public static void Success(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[" + DateTime.Now.Date + ":" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + "]: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
        }

        public static void Error(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("[" + DateTime.Now.Date + ":" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + "]: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
        }

        public static void Warning(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("[" + DateTime.Now.Date + ":" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + "]: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
        }
    }
}

