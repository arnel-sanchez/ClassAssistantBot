using ClassAssistantBot.Models;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using User = ClassAssistantBot.Models.User;

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
                        new KeyboardButton("*Ver Aula Actual*")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Pendientes*"),
                        new KeyboardButton("*Pendientes Directos*")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Revisar Clase Práctica*"),
                        new KeyboardButton("*Asignar Créditos*")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Iniciar Clase*"),
                        new KeyboardButton("*Ver Clases Inscritas*")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Crear Clase Práctica*"),
                        new KeyboardButton("*Enviar información a todos los estudiantes del aula*")
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
                        new KeyboardButton("*Llave del Estudiante*"),
                        new KeyboardButton("*Llave del Profesor*")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Todas las Aulas con Pendientes*"),
                        new KeyboardButton("*Cambiar de Aula*")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Asignar Canal de Memes*"),
                        new KeyboardButton("*Asignar Canal de Chistes*")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Asignar Canal de Interveciones en Clases*"),
                        new KeyboardButton("*Asignar Canal de Actulización de Diario*")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Asignar Canal de Frases de Estado*"),
                        new KeyboardButton("*Asignar Canal de Títulos de Clases*")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("*Asignar Canal de Rectificaciones de Profesores*"),
                        new KeyboardButton("*Asignar Canal de Misceláneas*")
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
                        new KeyboardButton("*Misceláneas*"),
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

        public static void CancelMenu(BotClient bot, Message message, string text = "")
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
            if (string.IsNullOrEmpty(text))
                text = "Cancelar";
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

        public static void PendingsFilters(BotClient bot, Message message, string text, int countPages, InteractionType interactionType = InteractionType.None)
        {
            var inline = new List<InlineKeyboardButton>();
            if (2 <= countPages)
                inline.Add(new InlineKeyboardButton
                {
                    CallbackData = $"NextPending//2//{(int)interactionType}",
                    Text = $">>2/{countPages}"
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
                    new InlineKeyboardButton[]{
                        new InlineKeyboardButton
                        {
                            CallbackData = $"Miscellaneous//1//{(int)InteractionType.Miscellaneous}",
                            Text = "Miscelánea"
                        }
                    },
                }
            };
            Menu.CancelMenu(bot, message, "Menú:");
            bot.SendMessage(chatId: message.Chat.Id,
                            text: text,
                            replyMarkup: keyboard);
        }

        public static void PendingsPaginators(BotClient bot, Message message, string text, int countPages, int thisPage, InteractionType interactionType = InteractionType.None)
        {
            var inline = new List<InlineKeyboardButton>();
            if (thisPage - 1 >= 1)
                inline.Add(new InlineKeyboardButton
                {
                    CallbackData = $"BackPending//{thisPage - 1}//{(int)interactionType}",
                    Text = $"{thisPage - 1}/{countPages}<<"
                });
            if (thisPage + 1 <= countPages)
                inline.Add(new InlineKeyboardButton
                {
                    CallbackData = $"NextPending//{thisPage + 1}//{(int)interactionType}",
                    Text = $">>{thisPage + 1}/{countPages}"
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
                        new InlineKeyboardButton[]{
                            new InlineKeyboardButton
                            {
                                CallbackData = $"Miscellaneous//1//{(int)InteractionType.Miscellaneous}",
                                Text = "Miscelánea"
                            }
                        },
                    }
            };
            Menu.CancelMenu(bot, message, "Menú:");
            bot.SendMessage(chatId: message.Chat.Id,
                            text: text,
                            replyMarkup: keyboard);
        }

        public static bool PendingCommands(BotClient bot, Message message, string pending, List<Teacher> teachers, bool giveMeExplication, string command, User user, string file)
        {
            var buttonGiveMeExplication = new List<InlineKeyboardButton>();
            if (giveMeExplication)
            {
                buttonGiveMeExplication.Add(new InlineKeyboardButton
                {
                    CallbackData = $"GiveMeExplication//{command}//{user.Username}",
                    Text = "Pedir Explicación al Alumno"
                });
            }
            var buttonTeachers = new List<InlineKeyboardButton[]>();
            teachers = teachers.Where(x => x.UserId != user.Id).ToList();
            if (teachers.Count != 0)
            {
                for (int i = 0; i < teachers.Count; i++)
                {
                    var temp = new List<InlineKeyboardButton>()
                    {
                        new InlineKeyboardButton
                        {
                            CallbackData = $"AssignDirectPending//{command}//{teachers[i].User.Username}",
                            Text = $"@{teachers[i].User.Username}"
                        }
                    };
                    buttonTeachers.Add(temp.ToArray());
                }
            }
            buttonTeachers.Add(buttonGiveMeExplication.ToArray());
            buttonTeachers.Add(
                new InlineKeyboardButton[]{
                    new InlineKeyboardButton
                    {
                        CallbackData = "DenialOfCreditApplications",
                        Text = "Denegar Solicitud de Créditos"
                    }
                });
            var keyboard = new InlineKeyboardMarkup()
            {
                InlineKeyboard = buttonTeachers.ToArray()
            };
            if (!string.IsNullOrEmpty(pending))
            {
                Menu.CancelMenu(bot, message);
                if (string.IsNullOrEmpty(file))
                {
                    bot.SendMessage(chatId: message.Chat.Id,
                        text: pending,
                        replyMarkup: keyboard);
                }
                else
                {
                    bot.SendPhoto(chatId: message.Chat.Id,
                        photo: file,
                        caption: pending,
                        replyMarkup: keyboard);
                }
                return true;
            }
            return false;
        }

        public static void PracticalClassList(BotClient bot, Message message, List<PracticClass> practicClasses, string text = "")
        {
            var buttonTeachers = new List<InlineKeyboardButton[]>();
            for (int i = 0; i < practicClasses.Count; i++)
            {
                var temp = new List<InlineKeyboardButton>()
                    {
                        new InlineKeyboardButton
                        {
                            CallbackData = $"PracticalClassID//{practicClasses[i].Id}",
                            Text = $"{practicClasses[i].Name}"
                        }
                    };
                buttonTeachers.Add(temp.ToArray());
            }
            var keyboard = new InlineKeyboardMarkup()
            {
                InlineKeyboard = buttonTeachers.ToArray()
            };
            bot.SendMessage(chatId: message.Chat.Id,
                        text: text,
                        replyMarkup: keyboard);
        }

        public static void PracticalClassStudentsList(BotClient bot, Message message, List<Student> students, string practicClassId, string text = "")
        {
            var buttonTeachers = new List<InlineKeyboardButton[]>();
            for (int i = 0; i < students.Count; i++)
            {
                var temp = new List<InlineKeyboardButton>()
                    {
                        new InlineKeyboardButton
                        {
                            CallbackData = $"StudentID//{students[i].UserId}//{practicClassId}",
                            Text = $"{(string.IsNullOrEmpty(students[i].User.FirstName) ? students[i].User.Name : students[i].User.FirstName + students[i].User.LastName )}"
                        }
                    };
                buttonTeachers.Add(temp.ToArray());
            }
            var keyboard = new InlineKeyboardMarkup()
            {
                InlineKeyboard = buttonTeachers.ToArray()
            };
            bot.SendMessage(chatId: message.Chat.Id,
                        text: text,
                        replyMarkup: keyboard);
        }
    }
}