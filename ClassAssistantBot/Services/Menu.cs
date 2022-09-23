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
                                                new KeyboardButton("*Estudiante*"),
                                                new KeyboardButton("*Profesor*")
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
                        new KeyboardButton("*Estudiantes*"),
                        new KeyboardButton("*Pendientes*")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Revisar Clases Prácticas*"),
                        new KeyboardButton("*Asignar Créditos*")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Iniciar Clase*"),
                        new KeyboardButton("*Ver Clases Inscritas*")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Ver Aula Actual*"),
                        new KeyboardButton("*Pendientes Directos*")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Configuración*")
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
                        new KeyboardButton("*Todos Los Pendientes*"),
                        new KeyboardButton("*Llave del Profesor*")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Llave del Estudiante*"),
                        new KeyboardButton("*Cambiar de Aula*")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Eliminar Estudiante del Aula*"),
                        new KeyboardButton("*Cancelar*")
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
                        new KeyboardButton("*Intervención en Clase*"),
                        new KeyboardButton("*Meme*"),
                        },
                    new KeyboardButton[]{
                        new KeyboardButton("*Rectificar al Profesor*"),
                        new KeyboardButton("*Chiste*")
                        },
                    new KeyboardButton[]{
                        new KeyboardButton("*Diario*"),
                        new KeyboardButton("*Créditos*"),
                        },
                    new KeyboardButton[]{
                        new KeyboardButton("*Frase de Estado*"),
                        new KeyboardButton("*Cambiar Título de Clase*")
                        },
                    new KeyboardButton[]{
                        new KeyboardButton("*Configuración*"),
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
                        new KeyboardButton("*Cancelar*")
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
                        new KeyboardButton("*Registrar*")
                    },
                    new KeyboardButton[]{
                        new KeyboardButton("*Cancelar*")
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
            var listButton = new KeyboardButton[teachers.Count+1][];

            for (int i = 0; i < teachers.Count; i++)
            {
                listButton[i] = new KeyboardButton[]
                {
                    new KeyboardButton($"*{teachers[i].User.FirstName}*//*{teachers[i].User.Username}*")
                };
            }
            listButton[teachers.Count] = new KeyboardButton[]
            {
                new KeyboardButton("*Cancelar*")
            };
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
            var listButton = new KeyboardButton[classes.Count+1][];

            for (int i = 0; i < classes.Count; i++)
            {
                listButton[i] = new KeyboardButton[]
                {
                    new KeyboardButton($"*{classes[i].Id}*//*{classes[i].Title}*")
                };
            }
            listButton[classes.Count] = new KeyboardButton[]
            {
                new KeyboardButton("*Cancelar*")
            };
            if (string.IsNullOrEmpty(text)) text = "Lista de Clases:";
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