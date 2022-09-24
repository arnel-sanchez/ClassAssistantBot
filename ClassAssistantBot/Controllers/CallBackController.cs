using System;
using ClassAssistantBot.Services;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;

namespace ClassAssistantBot.Controllers
{
    public class CallBackController
    {
        private BotClient bot;
        private Message message;
        private CallbackQuery callbackQuery;
        private Telegram.BotAPI.AvailableTypes.User appUser;
        private PendingDataHandler pendingDataHandler;
        private UserDataHandler userDataHandler;

        public CallBackController(BotClient bot, DataAccess dataAccess)
        {
            this.bot = bot;
            this.message = new Message();
            this.callbackQuery = new CallbackQuery();
            this.appUser = new Telegram.BotAPI.AvailableTypes.User();
            this.pendingDataHandler = new PendingDataHandler(dataAccess);
            this.userDataHandler = new UserDataHandler(dataAccess);
        }

        public void ProcessCallBackQuery(CallbackQuery callbackQuery)
        {
            this.callbackQuery = callbackQuery;
            this.message = callbackQuery.Message;
            if (callbackQuery.Message.From == null)
            {
                Logger.Error($"Error: Mensaje con usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            // Ignore user 777000 (Telegram)s
            if (callbackQuery.From.Id == 777000)
            {
                return;
            }
            this.appUser = callbackQuery.From;

            var user = userDataHandler.GetUser(appUser.Id);
            Models.InteractionType interactionType = Models.InteractionType.None;
            if (callbackQuery.Data == "Meme")
                interactionType = Models.InteractionType.Meme;
            else if (callbackQuery.Data == "ClassTitle")
                interactionType = Models.InteractionType.ClassTitle;
            else if (callbackQuery.Data == "ClassIntervention")
                interactionType = Models.InteractionType.ClassIntervention;
            else if (callbackQuery.Data == "StatusPhrase")
                interactionType = Models.InteractionType.StatusPhrase;
            else if (callbackQuery.Data == "Joke")
                interactionType = Models.InteractionType.Joke;
            else if (callbackQuery.Data == "RectificationToTheTeacher")
                interactionType = Models.InteractionType.RectificationToTheTeacher;
            var pendings = pendingDataHandler.GetPendings(user, false, interactionType);
            var keyboard = new InlineKeyboardMarkup()
            {
                InlineKeyboard = new InlineKeyboardButton[][]{
                        new InlineKeyboardButton[]{
                            new InlineKeyboardButton
                            {
                                CallbackData = "ClassIntervention",
                                Text = "Intervención en Clase"
                            },
                            new InlineKeyboardButton
                            {
                                CallbackData = "ClassTitle",
                                Text = "Cambiar Título de la Clase"
                            }
                        },
                        new InlineKeyboardButton[]{
                            new InlineKeyboardButton
                            {
                                CallbackData = "Meme",
                                Text = "Meme"
                            },
                            new InlineKeyboardButton
                            {
                                CallbackData = "Joke",
                                Text = "Chiste"
                            }
                        },
                        new InlineKeyboardButton[]{
                            new InlineKeyboardButton
                            {
                                CallbackData = "RectificationToTheTeacher",
                                Text = "Rectificación al Profesor"
                            },
                            new InlineKeyboardButton
                            {
                                CallbackData = "StatusPhrase",
                                Text = "Frase de Estado"
                            }
                        },
                    }
            };
            bot.SendMessage(chatId: message.Chat.Id,
                            text: pendings,
                            replyMarkup: keyboard);
            Menu.CancelMenu(bot, message);
        }
    }
}

