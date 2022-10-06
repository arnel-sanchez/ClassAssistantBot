using System;
using ClassAssistantBot.Models;
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
        private TeacherDataHandler teacherDataHandler;

        public CallBackController(BotClient bot, DataAccess dataAccess)
        {
            this.bot = bot;
            this.message = new Message();
            this.callbackQuery = new CallbackQuery();
            this.appUser = new Telegram.BotAPI.AvailableTypes.User();
            this.pendingDataHandler = new PendingDataHandler(dataAccess);
            this.userDataHandler = new UserDataHandler(dataAccess);
            this.teacherDataHandler = new TeacherDataHandler(dataAccess);
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

            var user = userDataHandler.GetUser(appUser);
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
                                CallbackData = $"ClassIntervention//1//{(int)InteractionType.ClassIntervention}",
                                Text = "Intervención en Clase"
                            },
                            new InlineKeyboardButton
                            {
                                CallbackData = $"ClassTitle//1//{(int)InteractionType.ClassTitle}",
                                Text = "Cambiar Título de la Clase"
                            }
                        },
                        new InlineKeyboardButton[]{
                            new InlineKeyboardButton
                            {
                                CallbackData = $"Meme//1//{(int)InteractionType.Meme}",
                                Text = "Meme"
                            },
                            new InlineKeyboardButton
                            {
                                CallbackData = $"Joke//1//{(int)InteractionType.Joke}",
                                Text = "Chiste"
                            }
                        },
                        new InlineKeyboardButton[]{
                            new InlineKeyboardButton
                            {
                                CallbackData = $"RectificationToTheTeacher//1//{(int)InteractionType.RectificationToTheTeacher}",
                                Text = "Rectificación al Profesor"
                            },
                            new InlineKeyboardButton
                            {
                                CallbackData = $"StatusPhrase//1//{(int)InteractionType.StatusPhrase}",
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
                Menu.PendingsPaginators(bot, message, pendings.Item1, pendings.Item2, page, interactionType);
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
                bool giveMeExplication = false;
                @object = pendingDataHandler.GetPendingByCode(code, out imageID, out giveMeExplication);
                pendingDataHandler.RemovePending(pending);
                if (isText)
                    bot.SendMessage(chatId: pending.Student.User.ChatId,
                                    text: $"El profesor @{user.Username} ha denegado su solicitud de créditos \n\n{@object}\n si tienes algún problema pregúntele a él, no la cojas conmigo.");
                else
                    bot.SendPhoto(chatId: pending.Student.User.ChatId,
                                  photo: imageID,
                                  caption: $"El profesor @{user.Username} ha denegado su solicitud de créditos si tienes algún problema pregúntele a él, no la cojas conmigo.");
                var pendings = pendingDataHandler.GetPendings(user);
                Menu.PendingsFilters(bot, message, pendings.Item1, pendings.Item2);
            }
            else if(callbackQuery.Data.Contains("GiveMeExplication//"))
            {
                var data = callbackQuery.Data.Split("//");

                var pendingCode = data[1];
                var username = data[2];

                var pending = pendingDataHandler.GetPending(pendingCode);

                var res = pendingDataHandler.GetContentObjectById(pending, username);

                if (string.IsNullOrEmpty(res.Item2))
                {
                    bot.SendMessage(chatId: pending.Student.User.ChatId,
                                    text: res.Item1);
                }
                else
                {
                    bot.SendPhoto(chatId: pending.Student.User.ChatId,
                                  caption: res.Item1,
                                  photo: res.Item2);
                }
                var pendings = pendingDataHandler.GetPendings(user);
                Menu.PendingsFilters(bot, message, pendings.Item1, pendings.Item2);
            }
            else if (callbackQuery.Data.Contains("AssignDirectPending//"))
            {
                var data = callbackQuery.Data.Split("//");
                var command = data[1];
                var teacherUsername = data[2];
                var pending = pendingDataHandler.GetPending(command);
                if (teacherDataHandler.ExistTeacher(teacherUsername))
                {
                    var teacherChatId = pendingDataHandler.AddDirectPending(teacherUsername, pending.Id);
                    bot.SendMessage(chatId: teacherChatId,
                        text: "Le han asignado un pendiente que tiene que revisar.");
                }
                else
                {
                    bot.SendMessage(chatId: user.ChatId,
                        text: $"No existe un usuario con el user name {teacherUsername}.");
                }
                var pendings = pendingDataHandler.GetPendings(user);
                Menu.PendingsFilters(bot, message, pendings.Item1, pendings.Item2);
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
                Menu.PendingsFilters(bot, message, pendings.Item1, pendings.Item2, interactionType);
            }
        }
    }
}

