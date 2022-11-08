using System.ComponentModel.DataAnnotations;
using System.Net;
using System.IO;
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
             RegisterLog(text);
        }

        public static void Command(string text)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("[" + DateTime.Now.Date + ":" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + "]: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
             RegisterLog(text);
        }

        public static void Error(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("[" + DateTime.Now.Date + ":" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + "]: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
             RegisterLog(text);
        }

        public static void Warning(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("[" + DateTime.Now.Date + ":" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + "]: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
             RegisterLog(text);
        }

        private static void RegisterLog(string text)
        {
            var fullPath = "./test.log";
            if (!File.Exists(fullPath))
            {
                var file = File.Create(fullPath);
                file.Close();

            }
             File.AppendAllText(fullPath, "[" + DateTime.Now.Date + ":" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + "]: " + text + "\n");
        }

        public static void ClearLog()
        {
            var fullPath = "./test.log";
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}

