using System.Text.Json;
using ClassAssistantBot.Models;
using ClassAssistantBot.Services;

var text = System.IO.File.ReadAllText("./environment.json");
var configuration = JsonSerializer.Deserialize<Configuration>(text);
DataAccess dataAccess = new DataAccess();
Console.WriteLine("Start!");
Console.WriteLine("Telegram API Key:" + configuration.TelegramApiKey);
Engine.StartPolling(dataAccess, configuration.TelegramApiKey);
Console.Read();