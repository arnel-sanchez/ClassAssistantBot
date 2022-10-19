using System.Text.Json;
using ClassAssistantBot.Models;
using ClassAssistantBot.Services;

var text = System.IO.File.ReadAllText("./environment.json");
var configuration = JsonSerializer.Deserialize<Configuration>(text);
DataAccess dataAccess = new DataAccess();
var excercises = dataAccess.Excercises.ToList();
foreach (var excercise in excercises)
{
    var credits = dataAccess.Credits.Where(x => x.ObjectId == excercise.Id).ToList();
    dataAccess.RemoveRange(credits);
}
dataAccess.SaveChanges();
Console.WriteLine("Start!");
Console.WriteLine("Telegram API Key:" + configuration.TelegramApiKey);
Engine.StartPolling(dataAccess, configuration.TelegramApiKey);
Console.Read();