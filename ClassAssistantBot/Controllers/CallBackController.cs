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
            if (callbackQuery.Data.Contains("NextPending//"))
            {
                var data = callbackQuery.Data.Split("//");
                int page = int.Parse(data[1]);
                var interactionType = (Models.InteractionType)int.Parse(data[2]);
                var pendings = pendingDataHandler.GetPendings(user, false, interactionType, page);
                var inline = new List<InlineKeyboardButton>();
                if (page - 1 >= 1)
                    inline.Add(new InlineKeyboardButton
                    {
                        CallbackData = $"BackPending//{page - 1}//{(int)interactionType}",
                        Text = "<<"
                    });
                if (page + 1 <= pendings.Item2)
                    inline.Add(new InlineKeyboardButton
                    {
                        CallbackData = $"NextPending//{page + 1}//{(int)interactionType}",
                        Text = ">>"
                    });
                var keyboard = new InlineKeyboardMarkup()
                {
                    InlineKeyboard = new InlineKeyboardButton[][]{
                        inline.ToArray(),
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
                Menu.CancelMenu(bot, message, "Menú:");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: pendings.Item1,
                                replyMarkup: keyboard);

            }
            else if (callbackQuery.Data.Contains("BackPending//"))
            {
                var data = callbackQuery.Data.Split("//");
                int page = int.Parse(data[1]);
                var interactionType = (Models.InteractionType)int.Parse(data[2]);
                var pendings = pendingDataHandler.GetPendings(user, false, interactionType, page);
                var inline = new List<InlineKeyboardButton>();
                if (page - 1 >= 1)
                    inline.Add(new InlineKeyboardButton
                    {
                        CallbackData = $"BackPending//{page - 1}//{(int)interactionType}",
                        Text = "<<"
                    });
                if (page + 1 <= pendings.Item2)
                    inline.Add(new InlineKeyboardButton
                    {
                        CallbackData = $"NextPending//{page + 1}//{(int)interactionType}",
                        Text = ">>"
                    });
                var keyboard = new InlineKeyboardMarkup()
                {
                    InlineKeyboard = new InlineKeyboardButton[][]{
                        inline.ToArray(),
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
                Menu.CancelMenu(bot, message, "Menú:");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: pendings.Item1,
                                replyMarkup: keyboard);

            }
            else if(callbackQuery.Data == "DenialOfCreditApplications")
            {
                string code = "";
                bool isText = true;
                if (!string.IsNullOrEmpty(callbackQuery.Message.Text))
                {
                    code = callbackQuery.Message.Text.Split(" /")[1];
                }
                else if (!string.IsNullOrEmpty(callbackQuery.Message.Caption))
                {
                    code = callbackQuery.Message.Caption.Split(" /")[1];
                    isText = false;
                }
                else
                    //Error
                    return;
                var pending = pendingDataHandler.GetPending(code);
                var @object = "";
                var imageID = "";
                @object = pendingDataHandler.GetPendingByCode(code, out imageID);
                pendingDataHandler.RemovePending(pending);
                Menu.TeacherMenu(bot, message);
                if (isText)
                    bot.SendMessage(chatId: pending.Student.User.ChatId,
                                    text: $"El profesor @{user.Username} ha denegado su solicitud de créditos \n\n{@object}\n si tienes algún problema pregúntele a él, no la cojas conmigo.");
                else
                    bot.SendPhoto(chatId: pending.Student.User.ChatId,
                                  photo: imageID,
                                  caption: $"El profesor @{user.Username} ha denegado su solicitud de créditos si tienes algún problema pregúntele a él, no la cojas conmigo.");
            }
            else
            {
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
                                CallbackData = $"NextPending//1//{(int)interactionType}",
                                Text = ">>"
                            }
                        },
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
                Menu.CancelMenu(bot, message, "Menú:");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: pendings.Item1,
                                replyMarkup: keyboard);
            }
        }
    }
}

