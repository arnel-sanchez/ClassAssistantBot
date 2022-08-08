using System.Text;
using System.Text.RegularExpressions;
using ClassAssistantBot.Models;
using ClassAssistantBot.Services;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;

namespace ClassAssistantBot.Controllers
{
    public class CommandController
    {     
        private StudentDataHandler studentDataHandler;
        private TeacherDataHandler teacherDataHandler;
        private MemeDataHandler memeDataHandler;
        private CreditsDataHandler creditsDataHandler;
        private DailyDataHandler dailyDataHandler;
        private JokeDataHandler jokeDataHandler;
        private StatusPhraseDataHandler statusPhraseDataHandler;
        private ClassRoomDataHandler classRoomDataHandler;
        private UserDataHandler userDataHandler;
        private BotClient bot;
        private Message message;
        private bool hasText = false;
        private Telegram.BotAPI.AvailableTypes.User appUser;

        public CommandController(BotClient bot, DataAccess dataAccess)
        {
            this.teacherDataHandler = new TeacherDataHandler(dataAccess);
            this.studentDataHandler = new StudentDataHandler(dataAccess);
            this.dailyDataHandler = new DailyDataHandler(dataAccess);
            this.memeDataHandler = new MemeDataHandler(dataAccess);
            this.creditsDataHandler = new CreditsDataHandler(dataAccess);
            this.jokeDataHandler = new JokeDataHandler(dataAccess);
            this.statusPhraseDataHandler = new StatusPhraseDataHandler(dataAccess);
            this.classRoomDataHandler = new ClassRoomDataHandler(dataAccess);
            this.userDataHandler = new UserDataHandler(dataAccess);
            this.bot = bot;
            this.message = new Message();
            this.appUser = new Telegram.BotAPI.AvailableTypes.User();
        }

        public void ProcessCommand(Message message)
        {
            if (message.From == null)
            {
                Logger.Error($"Error: Mensaje con usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            // Ignore user 777000 (Telegram)s
            if (message.From.Id == 777000)
            {
                return;
            }
            Logger.Warning($"New message from chat id: {message.Chat.Id}");

            appUser = message.From; // Save current user;
            this.message = message; // Save current message;
            hasText = !string.IsNullOrEmpty(message.Text); // True if message has text;

            Logger.Warning($"Message Text: {(hasText ? message.Text : "|:O")}");
            var user = userDataHandler.GetUser(appUser.Id);

            if (hasText)
            {
                if (message.Text.StartsWith('/')) // New commands
                {
                    var splitText = message.Text.Split(' ');
                    var command = splitText.First();
                    var parameters = splitText.Skip(1).ToArray();
                    // If the command includes a mention, you should verify that it is for your bot, otherwise you will need to ignore the command.
                    var pattern = string.Format(@"^\/(?<cmd>\w*)(?:$|@{0}$)", appUser.Username);
                    var match = Regex.Match(command, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        command = match.Groups.Values.Last().Value; // Get command name
                        Logger.Warning($"New command: {command}");
                        OnCommand(command, parameters, user);
                    }
                }
                else if (message.Text.StartsWith("*") && message.Text.EndsWith("*"))
                {
                    var commands = message.Text.Substring(1, message.Text.Length - 2).ToLower().Split(' ');
                    var command = new StringBuilder();

                    foreach (var item in commands)
                    {
                        command.Append(item);
                    }

                    Logger.Warning($"New command: {command}");
                    OnCommand(command.ToString(), new string[0], user);
                }
                else
                {
                    switch (user.Status)
                    {
                        case UserStatus.Created:
                            OnRegister(user, message.Text);
                            return;
                        case UserStatus.StudentEnteringClass:
                            OnAssignStudentAtClass(appUser.Id, message.Text);
                            Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.TeacherEnteringClass:
                            OnAssignTeacherAtClass(appUser.Id, message.Text);
                            Menu.TeacherMenu(bot, message);
                            return;
                        case UserStatus.TeacherCreatingClass:
                            OnCreateClassRoom(appUser.Id, message.Text);
                            Menu.TeacherMenu(bot, message);
                            return;
                        case UserStatus.Credits:
                            var res1 = creditsDataHandler.GetCreditsByUserName(user.Id, message.Text);
                            Menu.TeacherMenu(bot, message, res1);
                            return;
                        case UserStatus.StatusPhrase:
                            statusPhraseDataHandler.ChangeStatusPhrase(user.Id, message.Text);
                            Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.Joke:
                            jokeDataHandler.DoJoke(user.Id, message.Text);
                            Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.Daily:
                            dailyDataHandler.UpdateDaily(user.Id, message.Text);
                            Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.RemoveStudentFromClassRoom:
                            var res = studentDataHandler.RemoveStudentFromClassRoom(user.Id, message.Text);
                            Menu.TeacherMenu(bot, message, res);
                            return;
                        case UserStatus.ChangeClassRoom:
                            var outPut = 0;
                            var canParser = int.TryParse(message.Text, out outPut);
                            if (!canParser)
                            {
                                bot.SendMessage(chatId: message.Chat.Id,
                                                text: "Por favor, atienda lo que hace, necesito un ID de una clase.");
                                return;
                            }
                            classRoomDataHandler.ChangeClassRoom(user.Id, outPut);
                            if (user.IsTecaher)
                                Menu.TeacherMenu(bot, message);
                            else
                                Menu.StudentMenu(bot, message);
                            return;
                    }
                    Logger.Error($"Error: El usuario {user.Username} está escribiendo cosas sin sentido");
                    bot.SendMessage(chatId: message.Chat.Id,
                                    text: "Por favor, atienda lo que hace, no me haga perder tiempo.");
                }
            }
            else if (message.Document != null)
            {
                if (user.Status == UserStatus.Meme)
                {
                    memeDataHandler.SendMeme(user.Id, message.Document);
                    Menu.StudentMenu(bot, message);
                }
                else
                {
                    Logger.Error($"Error: El usuario {user.Username} está escribiendo cosas sin sentido");
                    bot.SendMessage(chatId: message.Chat.Id,
                                text: "Por favor, atienda lo que hace, no me haga perder tiempo.");
                }
            }
            else
            {
                Logger.Error($"Error: El usuario {user.Username} está escribiendo cosas sin sentido");
                bot.SendMessage(chatId: message.Chat.Id,
                            text: "Por favor, atienda lo que hace, no me haga perder tiempo.");
            }
        }

        private void OnCommand(string cmd, string[] args, ClassAssistantBot.Models.User user)
        {
            Logger.Warning($"Params: {args.Length}");
            if (user != null && user.ClassRoomActiveId == 0)
            {
                Logger.Error($"Error: El usuario {user.Username} está accediendo a una clase a la que no tiene privilegios.");
                Menu.RegisterMenu(bot, message, "No se encuentra registrado en ninguna clase.");
                return;
            }
            switch (cmd)
            {
                case "start":
                    StartCommand(user);
                    break;
                case "student":
                    StudentCommand(user);
                    break;
                case "teacher":
                    TeacherCommand(user);
                    break;
                case "create":
                    CreateCommand(user);
                    break;
                case "enter":
                    EnterCommand(user);
                    break;
                case "students":
                    StudentsCommand(user);
                    break;
                case "credits":
                    CreaditsCommand(user);
                    break;
                case "pendings":
                    PendingsCommand(user);
                    break;
                case "allpendings":
                    AllPendingsCommand(user);
                    break;
                case "studentaccesskey":
                    StudentAccessKeyCommand(user);
                    break;
                case "teacheraccesskey":
                    TeacherAccessKeyCommand(user);
                    break;
                case "changeclassroom":
                    ChangeClassRoomCommand(user);
                    break;
                case "removestudentfromclassroom":
                    RemoveStudentFromClassRoomCommand(user);
                    break;
                case "cancel":
                    CancelCommand(user);
                    break;
                case "classintervention":
                    ClassInterventionCommand(user);
                    break;
                case "meme":
                    MemeCommand(user);
                    break;
                case "joke":
                    JokeCommand(user);
                    break;
                case "rectificationtotheteacher":
                    RectificationToTheTeacherCommand(user);
                    break;
                case "daily":
                    DailyCommand(user);
                    break;
                case "statusphrase":
                    StatusPhraseCommand(user);
                    break;
                case "classtitle":
                    ClassTitleCommand(user);
                    break;
                case "configuration":
                    ConfigurationCommand(user);
                    break;
                case "poll":
                    PollCommand(user);
                    break;
                default:
                    DefaultCommand(user);
                    break;
            }
        }

        #region Comandos
        private void StartCommand(Models.User? user)
        {
            if (user == null)
            {
                userDataHandler.CreateUser(appUser, message.Chat.Id);
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Inserte su nombre y dos apellidos, por favor.",
                                replyMarkup: new ReplyKeyboardRemove());
                return;
            }
            else
            {
                Logger.Error($"Error: El usuario {user.Username} está intentando registrarse nuevamente en el bot.");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Ya está registrado en el bot.");
            }
        }

        private void StudentCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Verified)
            {
                Logger.Error($"Error: El estudiante {user.Username} está intentando unirse a un aula sin estar verificado.");
                bot.SendMessage(chatId: message.Chat.Id,
                            text: "Por favor, atienda lo que hace, no me haga perder tiempo, inserte su nombre y 2 apellidos.");
                return;
            }
            studentDataHandler.StudentEnterClass(user);
            bot.SendMessage(chatId: message.Chat.Id,
                            text: "Inserte el código que le dio su profesor.",
                            replyMarkup: new ReplyKeyboardRemove());
        }

        private void TeacherCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Verified)
            {
                Logger.Error($"Error: El usuario {user.Username} está intentando convertirse en profesor sin estar verificado.");
                bot.SendMessage(chatId: message.Chat.Id,
                            text: "Por favor, atienda lo que hace, no me haga perder tiempo, inserte su nombre y 2 apellidos.");
                return;
            }
            teacherDataHandler.CreateTeacher(user);
            var keyboard = new ReplyKeyboardMarkup
            {
                Keyboard = new KeyboardButton[][]{
                                            new KeyboardButton[]{
                                                new KeyboardButton("*Create*"),
                                                new KeyboardButton("*Enter*")
                                                },
                                        },
                ResizeKeyboard = true
            };
            bot.SendMessage(chatId: message.Chat.Id,
                            text: "Va a crear un aula nueva o va a entrar a una existente?",
                            replyMarkup: keyboard);
        }

        private void CreateCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.CreatingTecaher)
            {
                Logger.Error($"Error: El profesor {user.Username} está intentando crear un aula sin haber creado el profesor.");
                bot.SendMessage(chatId: message.Chat.Id,
                            text: "Por favor, atienda lo que hace, no me haga perder tiempo.");
                return;
            }
            classRoomDataHandler.CreateClassRoom(user);
            bot.SendMessage(chatId: message.Chat.Id,
                            text: "Inserte el nombre de la clase.",
                            replyMarkup: new ReplyKeyboardRemove());
        }

        private void EnterCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.CreatingTecaher)
            {
                Logger.Error($"Error: El profesor {user.Username} está intentando entrar a un aula sin creado el profesor.");
                bot.SendMessage(chatId: message.Chat.Id,
                            text: "Por favor, atienda lo que hace, no me haga perder tiempo.");
                return;
            }
            teacherDataHandler.TeacherEnterClass(user);
            bot.SendMessage(chatId: message.Chat.Id,
                            text: "Inserte el código que le dio su profesor.",
                            replyMarkup: new ReplyKeyboardRemove());
        }

        private void StudentsCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready)
            {
                Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con los comandos avanzados");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else if (!user.IsTecaher)
            {
                Logger.Error($"Error: El usuario {user.Username} está intentando solicitar un listado de estudiantes pero no es profesor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var res = studentDataHandler.GetStudentsOnClassByTeacherId(user.Id);
                Menu.TeacherMenu(bot, message, res);
            }
        }

        private void CreaditsCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready)
            {
                Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else if (!user.IsTecaher)
            {
                var text = creditsDataHandler.GetCreditsById(user.Id);
                Menu.StudentMenu(bot, message, text);
                return;
            }
            else
            {
                creditsDataHandler.CreditByTeacher(user);
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Inserte el nombre de usuario del estudainte.",
                                replyMarkup: new ReplyKeyboardRemove());
            }
        }

        private void PendingsCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && !user.IsTecaher)
            {
                Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {

            }
        }

        private void AllPendingsCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && !user.IsTecaher)
            {
                Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {

            }
        }

        private void StudentAccessKeyCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && !user.IsTecaher)
            {
                Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var accessKey = teacherDataHandler.GetStudentAccessKey(user.Id);
                Menu.TeacherConfigurationMenu(bot, message, $"La clave de acceso para sus estudianes es {accessKey}.");
            }
        }

        private void ConfigurationCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready)
            {
                Logger.Error($"Error: El usuario {user.Username} no está listo para solicitar la configuración de su cuenta");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else if (!user.IsTecaher)
            {
                Menu.StudentConfigurationMenu(bot, message,"Menú de configuración");
                return;
            }
            else
            {
                Menu.TeacherConfigurationMenu(bot, message, "Menú de configuración");
            }
        }

        private void ClassTitleCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && user.IsTecaher)
            {
                Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {

            }
        }

        private void DefaultCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (!user.IsTecaher)
            {
                Logger.Error($"Error: El usuario {user.Username} está interactuando con un comando que no existe");
                Menu.StudentMenu(bot, message, "El comando insertado no existe, por favor, no me haga perder tiempo.");
                return;
            }
            else
            {
                Logger.Error($"Error: El usuario {user.Username} está interactuando con un comando que no existe");
                Menu.TeacherMenu(bot, message, "El comando insertado no existe, por favor, no me haga perder tiempo.");
                return;
            }
        }

        private void StatusPhraseCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && user.IsTecaher)
            {
                Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var status = statusPhraseDataHandler.ChangeStatusPhrase(user);
                bot.SendMessage(chatId: message.Chat.Id,
                            text: $"Su frase de estado actual es: {status}",
                            replyMarkup: new ReplyKeyboardRemove());
                bot.SendMessage(chatId: message.Chat.Id,
                            text: "Inserte la frase de estado:",
                            replyMarkup: new ReplyKeyboardRemove());
            }
        }

        private void DailyCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && user.IsTecaher)
            {
                Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                dailyDataHandler.UpdateDaily(user);
                bot.SendMessage(chatId: message.Chat.Id,
                            text: "Inserte la actualización de su diario:",
                            replyMarkup: new ReplyKeyboardRemove());
            }
        }

        private void RectificationToTheTeacherCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && user.IsTecaher)
            {
                Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {

            }
        }

        private void JokeCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && user.IsTecaher)
            {
                Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                jokeDataHandler.DoJoke(user);
                bot.SendMessage(chatId: message.Chat.Id,
                            text: "Haga el chiste:",
                            replyMarkup: new ReplyKeyboardRemove());
            }
        }

        private void MemeCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && user.IsTecaher)
            {
                Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                memeDataHandler.SendMeme(user);
                bot.SendMessage(chatId: message.Chat.Id,
                            text: "Inserte un meme:",
                            replyMarkup: new ReplyKeyboardRemove());
            }
        }

        private void ClassInterventionCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && user.IsTecaher)
            {
                Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {

            }
        }

        private void RemoveStudentFromClassRoomCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && !user.IsTecaher)
            {
                Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var res = studentDataHandler.RemoveStudentFromClassRoom(user.Id);
                Menu.CancelMenu(bot, message, res);
            }
        }

        private void CancelCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            userDataHandler.CancelAction(user);
            if (user.IsTecaher)
                Menu.TeacherMenu(bot, message);
            else
                Menu.StudentMenu(bot, message);
        }

        private void ChangeClassRoomCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && !user.IsTecaher)
            {
                Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var res = classRoomDataHandler.ChangeClassRoom(user.Id);
                Menu.CancelMenu(bot, message, res);
            }
        }

        private void TeacherAccessKeyCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && !user.IsTecaher)
            {
                Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var accessKey = teacherDataHandler.GetTeacherAccessKey(user.Id);
                Menu.TeacherConfigurationMenu(bot, message, $"La clave de acceso para sus profesores es {accessKey}.");
            }
        }

        private void PollCommand(Models.User? user)
        {
            if (user == null)
            {
                Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            throw new NotImplementedException();
        }
        #endregion

        #region Procesos que completan comandos de varias operaciones
        private void OnRegister(Models.User user, string text)
        {
            var list = text.Split(' ');
            var name = "";
            var lastName = "";
            if (list.Length == 3)
            {
                name = list[0];
                lastName = list[1] + " " + list[2];
            }
            else if (list.Length == 4)
            {
                name = list[0] + " " + list[1];
                lastName = list[2] + " " + list[3];
            }
            else
            {
                Logger.Error($"Error: El usuario {user.Username} está intentando intentando registrar su nombre y apellidos en formato incorrecto.");
                bot.SendMessage(chatId: message.Chat.Id,
                                text: "Por favor, no me haga perder el tiempo, inserte su nombre y sus 2 apellidos.",
                                replyMarkup: new ReplyKeyboardRemove());
                return;
            }

            user.FirstName = name;
            user.LastName = lastName;
            user.Status = UserStatus.Verified;
            userDataHandler.VerifyUser(user);
            Logger.Success($"Verifying the user {user.Username}");

            bot.SendMessage(chatId: message.Chat.Id,
                            text: "Bienvenido " + user.FirstName + " " + user.LastName,
                            replyMarkup: new ReplyKeyboardRemove());

            Menu.RegisterMenu(bot, message);
        }

        private void OnAssignStudentAtClass(long id, string text)
        {
            var res = studentDataHandler.AssignStudentAtClass(id, text);
            bot.SendMessage(chatId: message.Chat.Id,
                            text: res,
                            replyMarkup: new ReplyKeyboardRemove());
        }

        private void OnAssignTeacherAtClass(long id, string text)
        {
            var res = teacherDataHandler.AssignTeacherAtClass(id, text);
            bot.SendMessage(chatId: message.Chat.Id,
                            text: res,
                            replyMarkup: new ReplyKeyboardRemove());
        }

        private void OnCreateClassRoom(long id, string name)
        {
            var res = classRoomDataHandler.CreateClassRoom(id, name);
            bot.SendMessage(chatId: message.Chat.Id,
                            text: res,
                            replyMarkup: new ReplyKeyboardRemove());
        }
        #endregion
    }
}