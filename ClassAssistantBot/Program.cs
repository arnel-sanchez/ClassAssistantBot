using System.Text.Json;
using ClassAssistantBot.Models;
using ClassAssistantBot.Services;

var text = System.IO.File.ReadAllText("./environment.json");
var configuration = JsonSerializer.Deserialize<Configuration>(text);
DataAccess dataAccess = new DataAccess();
Logger.ClearLog();
Console.WriteLine("Start!");
Console.WriteLine("Telegram API Key:" + configuration.TelegramApiKey);
try
{
    Engine.StartPolling(dataAccess, configuration.TelegramApiKey);
}
catch (Exception ex)
{
    await Logger.Error(ex.Message);
}
Console.Read();