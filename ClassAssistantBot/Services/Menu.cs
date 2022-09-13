using ClassAssistantBot.Models;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;

namespace ClassAssistantBot.Services
{
    public static class Menu
    {
        public static void RegisterMenu(BotClient bot, Message message, string text = "")
        {
            if (string.IsNullOrEmpty(text))
                text = "Es estudiante o profesor?";

            var keyboard = new ReplyKeyboardMarkup
            {
                Keyboard = new KeyboardButton[][]{
                                            new KeyboardButton[]{
                                                new KeyboardButton("*Student*"),
                                                new KeyboardButton("*Teacher*")
                                                },
                                        },
                ResizeKeyboard = true
            };
            bot.SendMessage(chatId: message.Chat.Id,
                            text: text,
                            replyMarkup: keyboard);
        }

        public static void TeacherMenu(BotClient bot, Message message, string text = "")
        {
            if (string.IsNullOrEmpty(text))
                text = "Menú";
            var keyboard = new ReplyKeyboardMarkup
            {
                Keyboard = new KeyboardButton[][]{
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Students*"),
                        new KeyboardButton("*Credits*")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Pendings*"),
                        new KeyboardButton("*Start Class*")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Configuration*")
                    }
                },
                ResizeKeyboard = true
            }; ;
            bot.SendMessage(chatId: message.Chat.Id,
                            text: text,
                            replyMarkup: keyboard);
        }

        public static void TeacherConfigurationMenu(BotClient bot, Message message, string text = "")
        {
            if (string.IsNullOrEmpty(text))
                text = "Menú";
            var keyboard = new ReplyKeyboardMarkup
            {
                Keyboard = new KeyboardButton[][]{
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*All Pendings*"),
                        new KeyboardButton("*Teacher Access Key*")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Student Access Key*"),
                        new KeyboardButton("*Change Classroom*")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Remove Student From Classroom*"),
                        new KeyboardButton("*Cancel*")
                    },
                },
                ResizeKeyboard = true
            }; ;
            bot.SendMessage(chatId: message.Chat.Id,
                            text: text,
                            replyMarkup: keyboard);
        }

        public static void StudentMenu(BotClient bot, Message message, string text = "")
        {
            if (string.IsNullOrEmpty(text))
                text = "Menú";
            var keyboard = new ReplyKeyboardMarkup
            {
                Keyboard = new KeyboardButton[][]{
                    new KeyboardButton[]{
                        new KeyboardButton("*Class Intervention*"),
                        new KeyboardButton("*Meme*"),
                        },
                    new KeyboardButton[]{
                        new KeyboardButton("*Rectification At Teacher*"),
                        new KeyboardButton("*Joke*")
                        },
                    new KeyboardButton[]{
                        new KeyboardButton("*Daily*"),
                        new KeyboardButton("*Credits*"),
                        },
                    new KeyboardButton[]{
                        new KeyboardButton("*Status Phrase*"),
                        new KeyboardButton("*Class Title*")
                        },
                    new KeyboardButton[]{
                        new KeyboardButton("*Configuration*"),
                        },
                },
                ResizeKeyboard = true
            }; ;
            bot.SendMessage(chatId: message.Chat.Id,
                            text: text,
                            replyMarkup: keyboard);
        }

        public static void StudentConfigurationMenu(BotClient bot, Message message, string text = "")
        {

        }

        public static void CancelMenu(BotClient bot, Message message, string text)
        {
            var keyboard = new ReplyKeyboardMarkup
            {
                Keyboard = new KeyboardButton[][]{
                    new KeyboardButton[]{
                        new KeyboardButton("*Cancel*")
                    }
                },
                ResizeKeyboard = true
            }; ;
            bot.SendMessage(chatId: message.Chat.Id,
                            text: text,
                            replyMarkup: keyboard);
        }

        public static void ChangeClassRoomMenu(BotClient bot, Message message, string text)
        {
            var keyboard = new ReplyKeyboardMarkup
            {
                Keyboard = new KeyboardButton[][]{
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Register*")
                    },
                    new KeyboardButton[]{
                        new KeyboardButton("*Cancel*")
                    }
                },
                ResizeKeyboard = true
            }; ;
            bot.SendMessage(chatId: message.Chat.Id,
                            text: text,
                            replyMarkup: keyboard);
        }

        public static void TeachersList(BotClient bot, Message message,List<Teacher> teachers, string text = "")
        {
            var listButton = new KeyboardButton[teachers.Count][];

            for (int i = 0; i < teachers.Count; i++)
            {
                listButton[i] = new KeyboardButton[]
                {
                    new KeyboardButton($"*{teachers[i].User.FirstName}*//*{teachers[i].User.Username}*")
                };
            }
            var keyboard = new ReplyKeyboardMarkup
            {
                Keyboard = listButton,
                ResizeKeyboard = true
            };
            if (string.IsNullOrEmpty(text)) text = "Lista de Profesores:";
            bot.SendMessage(chatId: message.Chat.Id,
                            text: text,
                            replyMarkup: keyboard);
        }

        public static void ClassesList(BotClient bot, Message message, List<Class> classes, string text = "")
        {
            if (string.IsNullOrEmpty(text)) text = "Lista de Clases:";
            var listButton = new KeyboardButton[classes.Count][];

            for (int i = 0; i < classes.Count; i++)
            {
                listButton[i] = new KeyboardButton[]
                {
                    new KeyboardButton($"*{classes[i].Id}*//*{classes[i].Title}*")
                };
            }
            if (string.IsNullOrEmpty(text)) text = "Lista de Clases:";
            if (classes.Count == 0)
            {
                CancelMenu(bot, message, text);
                return;
            }
            var keyboard = new ReplyKeyboardMarkup
            {
                Keyboard = listButton,
                ResizeKeyboard = true
            };
            bot.SendMessage(chatId: message.Chat.Id,
                            text: text,
                            replyMarkup: keyboard);
        }
    }
}