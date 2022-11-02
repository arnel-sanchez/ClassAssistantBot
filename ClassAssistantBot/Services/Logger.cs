using System.ComponentModel.DataAnnotations;
using System.Net;
using System.IO;
using System;
namespace ClassAssistantBot.Services
{
    public static class Logger
    {
        public static async Task Success(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[" + DateTime.Now.Date + ":" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + "]: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
            await RegisterLog(text);
        }

        public static async Task Command(string text)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("[" + DateTime.Now.Date + ":" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + "]: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
            await RegisterLog(text);
        }

        public static async Task Error(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("[" + DateTime.Now.Date + ":" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + "]: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
            await RegisterLog(text);
        }

        public static async Task Warning(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("[" + DateTime.Now.Date + ":" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + "]: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
            await RegisterLog(text);
        }

        private static async Task RegisterLog(string text)
        {
            var fullPath = "./test.log";
            if (!File.Exists(fullPath)) File.Create(fullPath);
            await File.AppendAllTextAsync(fullPath, "[" + DateTime.Now.Date + ":" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + "]: " + text);
        }
    }
}

