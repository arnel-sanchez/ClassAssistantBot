using System.Text;
using Telegram.BotAPI;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.InlineMode;
using Telegram.BotAPI.Payments;
using ClassAssistantBot.Controllers;

namespace ClassAssistantBot.Services
{
    public sealed class Engine : TelegramBotBase
    {
        private static BotClient? bot;
        private static CommandController? commandControler;

        public static void StartPolling(DataAccess dataAccess, string apiKey)
        {
            bot = new BotClient(apiKey);
            commandControler = new CommandController(bot, dataAccess);
            Engine.SetMyCommands();
            var updates = bot.GetUpdates<IEnumerable<Update>>();
            while (true)
            {
                if (updates.Any())
                {
                    foreach (var update in updates)
                    {
                        var botInstance = new Engine();
                        botInstance.OnUpdate(update);
                    }
                    var offset = updates.Last().UpdateId + 1;
                    updates = bot.GetUpdates<IEnumerable<Update>>(offset);
                }
                else
                {
                    updates = bot.GetUpdates<IEnumerable<Update>>();
                }
            }
        }

        private static void SetMyCommands()
        {
            bot.SetMyCommands(
                new BotCommand[]
                {
                    new BotCommand("start", "Inicia una nueva instancia del bot"),
                    new BotCommand("student", "Se registra el usuario como estudiante"),
                    new BotCommand("teacher", "Se registra el usuario como profesor")
                }
            );
        }

        protected override void OnMessage(Message message)
        {
            commandControler.ProcessCommand(message);
        }

        protected override void OnBotException(BotRequestException exp)
        {
            Logger.Error(exp.Message);
        }

        protected override void OnException(Exception exp)
        {
            Logger.Error(exp.Message);
        }

        protected override void OnCallbackQuery(CallbackQuery callbackQuery)
        {

        }

        protected override void OnChannelPost(Message message)
        {

        }

        protected override void OnChosenInlineResult(ChosenInlineResult chosenInlineResult)
        {

        }

        protected override void OnEditedChannelPost(Message message)
        {

        }

        protected override void OnEditedMessage(Message message)
        {

        }

        protected override void OnInlineQuery(InlineQuery inlineQuery)
        {

        }

        protected override void OnPoll(Poll poll)
        {

        }

        protected override void OnPollAnswer(PollAnswer pollAnswer)
        {

        }

        protected override void OnPreCheckoutQuery(PreCheckoutQuery preCheckoutQuery)
        {

        }

        protected override void OnShippingQuery(ShippingQuery shippingQuery)
        {

        }
    }
}
