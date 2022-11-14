﻿using System.Text;
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
        private DiaryDataHandler diaryDataHandler;
        private JokeDataHandler jokeDataHandler;
        private StatusPhraseDataHandler statusPhraseDataHandler;
        private ClassRoomDataHandler classRoomDataHandler;
        private UserDataHandler userDataHandler;
        private PendingDataHandler pendingDataHandler;
        private ClassInterventionDataHandler classInterventionDataHandler;
        private RectificationToTheTeacherDataHandler rectificationToTheTeacherDataHandler;
        private ClassTitleDataHandler classTitleDataHandler;
        private MiscellaneousDataHandler miscellaneousDataHandler;
        private PracticClassDataHandler practicClassDataHandler;
        private GuildDataHandler guildDataHandler;
        private BotClient bot;
        private Message message;
        private bool hasText = false;
        private Telegram.BotAPI.AvailableTypes.User appUser;

        public CommandController(BotClient bot, DataAccess dataAccess) 
        {
            this.teacherDataHandler = new TeacherDataHandler(dataAccess);
            this.studentDataHandler = new StudentDataHandler(dataAccess);
            this.diaryDataHandler = new DiaryDataHandler(dataAccess);
            this.memeDataHandler = new MemeDataHandler(dataAccess);
            this.creditsDataHandler = new CreditsDataHandler(dataAccess);
            this.jokeDataHandler = new JokeDataHandler(dataAccess);
            this.statusPhraseDataHandler = new StatusPhraseDataHandler(dataAccess);
            this.classRoomDataHandler = new ClassRoomDataHandler(dataAccess);
            this.userDataHandler = new UserDataHandler(dataAccess);
            this.pendingDataHandler = new PendingDataHandler(dataAccess);
            this.classInterventionDataHandler = new ClassInterventionDataHandler(dataAccess);
            this.rectificationToTheTeacherDataHandler = new RectificationToTheTeacherDataHandler(dataAccess);
            this.classTitleDataHandler = new ClassTitleDataHandler(dataAccess);
            this.miscellaneousDataHandler = new MiscellaneousDataHandler(dataAccess);
            this.practicClassDataHandler = new PracticClassDataHandler(dataAccess);
            this.guildDataHandler = new GuildDataHandler(dataAccess);
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
             Logger.Warning($"New message from chat id: {(string.IsNullOrEmpty(message.From.Username) ? message.Chat.Id : message.From.Username )}");

            appUser = message.From; // Save current user;
            this.message = message; // Save current message;
            hasText = !string.IsNullOrEmpty(message.Text); // True if message has text;

             Logger.Warning($"Message Text: {(hasText ? message.Text : "|:O")}");
            var user =  userDataHandler.GetUser(appUser);

            if (user != null && user.ClassRoomActiveId == 0 && user.Status!=UserStatus.Created && user.Status!=UserStatus.Verified && user.Status!=UserStatus.StudentEnteringClass && user.Status!=UserStatus.CreatingTecaher && user.Status!=UserStatus.TeacherCreatingClass && user.Status!=UserStatus.TeacherEnteringClass)
            {
                 userDataHandler.VerifyUser(user);
                 Menu.RegisterMenu(bot, message);
                return;
            }

            if (hasText)
            {
                if (user != null && user.Status == UserStatus.Created)
                {
                     OnRegister(user, message.Text);
                    return;
                }
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
                         Logger.Command($"New command: {command}");
                         OnCommand(command, parameters, user);
                    }
                }
                else if (message.Text.StartsWith("*") && message.Text.EndsWith("*") && message.Text.Length >= 4 && !message.Text.Contains("*//*"))
                {
                    var commands = message.Text.Substring(1, message.Text.Length - 2).ToLower().Split(' ');
                    var command = new StringBuilder();

                    foreach (var item in commands)
                    {
                        command.Append(item);
                    }

                     Logger.Command($"New command: {command}");
                     OnCommand(command.ToString(), new string[0], user);
                }
                else if (message.Text.StartsWith("*") && message.Text.EndsWith("*") && message.Text.Length >= 4 && message.Text.ToString().Contains("*//*"))
                {
                    var commands = message.Text.Substring(1, message.Text.Length - 2).Split(' ');
                    var command = new StringBuilder();

                    foreach (var item in commands)
                    {
                        command.Append(item);
                    }

                     Logger.Command($"New command: {command}");
                    if (user.Status == UserStatus.RectificationAtTeacher)
                    {
                         OnRectificationToTheTeacherAtTeacherUserName(user, command.ToString().Split("*//*")[1]);
                    }
                    else if(user.Status == UserStatus.ClassIntervention)
                    {
                         OnClassIntervention(user, long.Parse(command.ToString().Split("*//*")[0]));
                    }
                    else if(user.Status == UserStatus.ClassTitle)
                    {
                         OnChangeClassTitle(user, long.Parse(command.ToString().Split("*//*")[0]));
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
                    switch (user.Status)
                    {
                        case UserStatus.StudentEnteringClass:
                            var assignedStudentSuccesfully =  OnAssignStudentAtClass(appUser.Id, message.Text);
                            if(assignedStudentSuccesfully)
                                 Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.Pending:
                             OnPendings(user, message);
                            return;
                        case UserStatus.TeacherEnteringClass:
                            var assignedTeacherSuccesfully =  OnAssignTeacherAtClass(appUser.Id, message.Text);
                            if(assignedTeacherSuccesfully)
                                 Menu.TeacherMenu(bot, message);
                            return;
                        case UserStatus.TeacherCreatingClass:
                             OnCreateClassRoom(appUser.Id, message.Text);
                             Menu.TeacherMenu(bot, message);
                            return;
                        case UserStatus.Credits:
                            var res1 =  creditsDataHandler.GetCreditsByUserName(user.Id, message.Text, false, true);
                             Menu.TeacherMenu(bot, message, res1);
                            return;
                        case UserStatus.StatusPhrase:
                             statusPhraseDataHandler.ChangeStatusPhrase(user.Id, message.Text);
                            if (!string.IsNullOrEmpty( classRoomDataHandler.GetStatusPhraseChannel(user)))
                                 bot.SendMessage(chatId:  classRoomDataHandler.GetStatusPhraseChannel(user),
                                    text: "Frase de Estado enviada por: @" + user.Username + "\n\"" + message.Text + "\"");
                             Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.Joke:
                             jokeDataHandler.DoJoke(user.Id, message.Text);
                            if (!string.IsNullOrEmpty( classRoomDataHandler.GetJokesChannel(user)))
                                 bot.SendMessage(chatId:  classRoomDataHandler.GetJokesChannel(user),
                                    text: "Chiste enviado por: @" + user.Username + "\n\"" + message.Text + "\"");
                             Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.Diary:
                             diaryDataHandler.UpdateDiary(user.Id, message.Text);
                            if (!string.IsNullOrEmpty( classRoomDataHandler.GetDiaryChannel(user)))
                                 bot.SendMessage(chatId:  classRoomDataHandler.GetDiaryChannel(user),
                                    text: "Actualización del Diario enviada por: @" + user.Username + "\n\"" + message.Text + "\"");
                             Logger.Success($"The student {user.Username} is ready to update Diary");
                             Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.RemoveStudentFromClassRoom:
                            var res =  studentDataHandler.RemoveStudentFromClassRoom(user.Id, message.Text);
                             bot.SendMessage(chatId: res.Item2,
                                    text: $"El profesor @{user.Username} le ha sacado del aula.",
                                    replyMarkup: new ReplyKeyboardMarkup
                                    {
                                        Keyboard = new KeyboardButton[][]{
                                            new KeyboardButton[]
                                            {
                                                new KeyboardButton("*Entrar a la Facultad*")
                                            }
                                        },
                                        ResizeKeyboard = true
                                    });
                             Menu.TeacherMenu(bot, message, res.Item1);
                            return;
                        case UserStatus.ClassIntervention:
                             classInterventionDataHandler.CreateIntervention(user, message.Text);
                             Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.RectificationToTheTeacherUserName:
                             OnRectificationToTheTeacherAtText(user, message.Text);
                            if (!string.IsNullOrEmpty( classRoomDataHandler.GetRectificationToTheTeacherChannel(user)))
                                 bot.SendMessage(chatId:  classRoomDataHandler.GetRectificationToTheTeacherChannel(user),
                                    text: "Rectificación al Profesor enviada por: @" + user.Username + "\n\"" + message.Text + "\"");
                             Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.CreateClass:
                             OnStartClass(user, message.Text);
                             Menu.TeacherMenu(bot, message);
                            return;
                        case UserStatus.ClassTitleSelect:
                            if (!string.IsNullOrEmpty( classRoomDataHandler.GetClassTitleChannel(user)))
                                 bot.SendMessage(chatId:  classRoomDataHandler.GetClassTitleChannel(user),
                                    text: "Cambio de Título de Clase enviada por: @" + user.Username + "\n\"" + message.Text + "\"");
                             OnChangeClassTitle(user, message.Text);
                             Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.ClassInterventionSelect:
                             OnClassIntervention(user, message.Text);
                            if (!string.IsNullOrEmpty( classRoomDataHandler.GetClassInterventionChannel(user)))
                                 bot.SendMessage(chatId:  classRoomDataHandler.GetClassInterventionChannel(user),
                                    text: "Intervención en Clase enviada por: @" + user.Username + "\n\"" + message.Text + "\"");
                             Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.AssignCreditsStudent:
                             OnAssignCredits(user, message.Text);
                            return;
                        case UserStatus.AssignCredits:
                             OnAssignCreditsMessage(user, message.Text);
                             Menu.TeacherMenu(bot, message);
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
                        case UserStatus.AssignMemeChannel:
                             classRoomDataHandler.AssignMemeChannel(user, message.Text);
                            var memeChannelConfigurationText = "Canal de Meme asignado satisfactoriamente";
                            try
                            {
                                 bot.SendMessage(chatId: message.Text,
                                    text: "Bot configurado satisfactoriamente.");
                            }
                            catch
                            {
                                memeChannelConfigurationText = "El nombre de usuario del bot insertado no existe";
                            }
                             Menu.TeacherConfigurationMenu(bot, message, memeChannelConfigurationText);
                            return;
                        case UserStatus.AssignJokeChannel:
                             classRoomDataHandler.AssignJokesChannel(user, message.Text);
                            var jokeChannelConfigurationText = "Canal de Chistes asignado satisfactoriamente";
                            try
                            {
                                 bot.SendMessage(chatId: message.Text,
                                    text: "Bot configurado satisfactoriamente.");
                            }
                            catch
                            {
                                jokeChannelConfigurationText = "El nombre de usuario del bot insertado no existe";
                            }
                             Menu.TeacherConfigurationMenu(bot, message, jokeChannelConfigurationText);
                            return;
                        case UserStatus.AssignClassInterventionChannel:
                             classRoomDataHandler.AssignClassInterventionChannel(user, message.Text);
                            var classInterventionsChannelConfigurationText = "Canal de Intervenciones en Clases asignado satisfactoriamente";
                            try
                            {
                                 bot.SendMessage(chatId: message.Text,
                                    text: "Bot configurado satisfactoriamente.");
                            }
                            catch
                            {
                                classInterventionsChannelConfigurationText = "El nombre de usuario del bot insertado no existe";
                            }
                             Menu.TeacherConfigurationMenu(bot, message, classInterventionsChannelConfigurationText);
                            return;
                        case UserStatus.AssignClassTitleChannel:
                             classRoomDataHandler.AssignClassTitleChannel(user, message.Text);
                            var classTitlesChannelConfigurationText = "Canal de propuestas de Títulos de Clases asignado satisfactoriamente";
                            try
                            {
                                 bot.SendMessage(chatId: message.Text,
                                    text: "Bot configurado satisfactoriamente.");
                            }
                            catch
                            {
                                classTitlesChannelConfigurationText = "El nombre de usuario del bot insertado no existe";
                            }
                             Menu.TeacherConfigurationMenu(bot, message, classTitlesChannelConfigurationText);
                            return;
                        case UserStatus.AssignDiaryChannel:
                             classRoomDataHandler.AssignDiaryChannel(user, message.Text);
                            var diaryChannelConfigurationText = "Canal de Actualizaciones de Diario asignado satisfactoriamente";
                            try
                            {
                                 bot.SendMessage(chatId: message.Text,
                                    text: "Bot configurado satisfactoriamente.");
                            }
                            catch
                            {
                                diaryChannelConfigurationText = "El nombre de usuario del bot insertado no existe";
                            }
                             Menu.TeacherConfigurationMenu(bot, message, diaryChannelConfigurationText);
                            return;
                        case UserStatus.AssignRectificationToTheTeacherChannel:
                             classRoomDataHandler.AssignRectificationToTheTeacherChannel(user, message.Text);
                            var rectificationToTheTeacherChannelConfigurationText = "Canal de Rectificaciones a los profesores asignado satisfactoriamente";
                            try
                            {
                                 bot.SendMessage(chatId: message.Text,
                                    text: "Bot configurado satisfactoriamente.");
                            }
                            catch
                            {
                                rectificationToTheTeacherChannelConfigurationText = "El nombre de usuario del bot insertado no existe";
                            }
                             Menu.TeacherConfigurationMenu(bot, message, rectificationToTheTeacherChannelConfigurationText);
                            return;
                        case UserStatus.AssignStatusPhraseChannel:
                             classRoomDataHandler.AssignStatusPhraseChannel(user, message.Text);
                            var statusPhraseChannelConfigurationText = "Canal de Frases de Estado asignado satisfactoriamente";
                            try
                            {
                                 bot.SendMessage(chatId: message.Text,
                                    text: "Bot configurado satisfactoriamente.");
                            }
                            catch
                            {
                                statusPhraseChannelConfigurationText = "El nombre de usuario del bot insertado no existe";
                            }
                             Menu.TeacherConfigurationMenu(bot, message, statusPhraseChannelConfigurationText);
                            return;
                        case UserStatus.AssignMiscellaneousChannel:
                             classRoomDataHandler.AssignMiscellaneousChannel(user, message.Text);
                            var miscellaneousChannelConfigurationText = "Canal de Misceláneas asignado satisfactoriamente";
                            try
                            {
                                 bot.SendMessage(chatId: message.Text,
                                    text: "Bot configurado satisfactoriamente.");
                            }
                            catch
                            {
                                miscellaneousChannelConfigurationText = "El nombre de usuario del bot insertado no existe";
                            }
                             Menu.TeacherConfigurationMenu(bot, message, miscellaneousChannelConfigurationText);
                            return;
                        case UserStatus.SendInformation:
                            var students =  classRoomDataHandler.GetStudentsOnClassRoom(user);
                            foreach (var student in students)
                            {
                                 bot.SendMessage(chatId: student.Student.User.ChatId,
                                    text: message.Text);
                            }
                            var teachers =  classRoomDataHandler.GetTeachersOnClassRoom(user);
                            foreach (var teacher in teachers)
                            {
                                 bot.SendMessage(chatId: teacher.Teacher.User.ChatId,
                                    text: message.Text);
                            }
                             Menu.TeacherMenu(bot, message);
                            return;
                        case UserStatus.EditPracticalClasss:
                            var editPracticalClassRes =  practicClassDataHandler.EditPracticalClassName(user, message.Text);
                             Menu.TeacherMenu(bot, message, editPracticalClassRes);
                            return;
                        case UserStatus.Miscellaneous:
                             miscellaneousDataHandler.CreateMiscellaneous(user, message.Text);
                            if (!string.IsNullOrEmpty( classRoomDataHandler.GetMiscellaneousChannel(user)))
                                 bot.SendMessage(chatId:  classRoomDataHandler.GetMiscellaneousChannel(user),
                                    text: "Miscelánea enviada por: @" + user.Username + "\n\"" + message.Text + "\"");
                             Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.CreatePracticClass:
                            var canCreateClass =  practicClassDataHandler.CreatePracticClass(user, message.Text);
                            if(canCreateClass)
                            {
                                 Menu.TeacherMenu(bot, message);
                            }
                            else
                            {
                                 Menu.CancelMenu(bot, message, "Clase Práctica con Formato Incorrecto");
                            }
                            return;
                        case UserStatus.CreateGuild:
                             guildDataHandler.CreateGuild(user, message.Text);
                             Menu.GuildMenu(bot, message);
                            return;
                        case UserStatus.AssignCreditsAtGuild:
                            var data =  guildDataHandler.AssignCreditsAtGuild(user, message.Text);
                            foreach (var student in data.Item1)
                            {
                                 bot.SendMessage(chatId: student.User.ChatId,
                                    text: $"Ha recibido {data.Item2} créditos y su profesor le dijo \"{data.Item3}\".");
                            }
                             Menu.GuildMenu(bot, message);
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
            else if(message.Photo != null)
            {
                if (user.Status == UserStatus.Meme )
                {
                     memeDataHandler.SendMeme(user.Id, message.Photo[0]);
                     Menu.StudentMenu(bot, message);
                }
                else
                {
                     Logger.Error($"Error: El usuario {user.Username} está escribiendo cosas sin sentido");
                     bot.SendMessage(chatId: message.Chat.Id,
                                text: "Por favor, atienda lo que hace, no me haga perder tiempo.");
                }
            }
            else if(message.Audio != null)
            {
                if(user.Status == UserStatus.SendInformation)
                {
                    var students = classRoomDataHandler.GetStudentsOnClassRoom(user);
                    foreach (var student in students)
                    {
                        bot.SendAudio(
                            chatId: student.Student.User.ChatId,
                            caption: message.Caption,
                            audio: message.Audio.FileId
                        );
                    }
                    var teachers = classRoomDataHandler.GetTeachersOnClassRoom(user);
                    foreach (var teacher in teachers)
                    {
                        bot.SendAudio(
                            chatId: teacher.Teacher.User.ChatId,
                            caption: message.Caption,
                            audio: message.Audio.FileId
                        );
                    }
                    Menu.TeacherMenu(bot, message);
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
            switch (cmd)
            {
                case "start":
                     StartCommand(user);
                    break;
                case "estudiante":
                     StudentCommand(user);
                    break;
                case "profesor":
                     TeacherCommand(user);
                    break;
                case "crear":
                     CreateCommand(user);
                    break;
                case "entrar":
                     EnterCommand(user);
                    break;
                case "estudiantes":
                     StudentsCommand(user);
                    break;
                case "créditos":
                     CreditsCommand(user);
                    break;
                case "revisarclasepráctica":
                     ReviewPracticClassCommand(user);
                    break;
                case "crearclasepráctica":
                     CreatePracticClassCommand(user);
                    break;
                case "pendientesdirectos":
                     DirectPendingCommand(user);
                    break;
                case "misceláneas":
                     CreateMiscellaneousCommand(user);
                    break;
                case "pendientes":
                     PendingsCommand(user);
                    break;
                case "todaslasaulasconpendientes":
                     AllClassRoomWithPendingsCommand(user);
                    break;
                case "llavedelestudiante":
                     StudentAccessKeyCommand(user);
                    break;
                case "llavedelprofesor":
                     TeacherAccessKeyCommand(user);
                    break;
                case "cambiardeaula":
                     ChangeClassRoomCommand(user);
                    break;
                case "rectificaralprofesor":
                     RectificationToTheTeacherCommand(user);
                    break;
                case "asignarcréditos":
                     AssignCreditsCommand(user);
                    break;
                case "eliminarestudiantedelaula":
                     RemoveStudentFromClassRoomCommand(user);
                    break;
                case "cancelar":
                     CancelCommand(user);
                    break;
                case "intervenciónenclase":
                     ClassInterventionCommand(user);
                    break;
                case "meme":
                     MemeCommand(user);
                    break;
                case "chiste":
                     JokeCommand(user);
                    break;
                case "diario":
                     DiaryCommand(user);
                    break;
                case "frasedeestado":
                     StatusPhraseCommand(user);
                    break;
                case "cambiartítulodeclase":
                     ClassTitleCommand(user);
                    break;
                case "configuración":
                     ConfigurationCommand(user);
                    break;
                case "asignarcanaldemisceláneas":
                     AssignMiscellaneousChannelCommand(user);
                    break;
                case "iniciarclase":
                     StartClassCommand(user);
                    break;
                case "registrar":
                     Menu.RegisterMenu(bot, message);
                    break;
                case "gremios":
                     Menu.GuildMenu(bot, message);
                    break;
                case "veraulaactual":
                     SeeClassRoomActiveCommand(user);
                    break;
                case "verclasesinscritas":
                     SeeListClass(user);
                    break;
                case "asignarcanaldememes":
                     AssignMemeChannelCommand(user);
                    break;
                case "asignarcanaldechistes":
                     AssignJokesChannelCommand(user);
                    break;
                case "asignarcanaldeintervecionesenclases":
                     AssignClassInterventionChannelCommand(user);
                    break;
                case "asignarcanaldeactulizacióndediario":
                     AssignDiaryChannelCommand(user);
                    break;
                case "asignarcanaldefrasesdeestado":
                     AssignStatusPhraseChannelCommand(user);
                    break;
                case "asignarcanaldetítulosdeclases":
                     AssignClassTitleChannelCommand(user);
                    break;
                case "asignarcanalderectificacionesdeprofesores":
                     AssignRectificationToTheTeacherChannelCommand(user);
                    break;
                case "enviarinformaciónatodoslosestudiantesdelaula":
                     SendStudentsInformationCommand(user);
                    break;
                case "editarnombredeclasepráctica":
                     EditPracticalClassNameCommand(user);
                    break;
                case "verestadodecréditos":
                     CreditsStatusCommand(user);
                    break;
                case "creargremio":
                     CreateGuildCommand(user);
                    break;
                case "asignarestudiantealgremio":
                     AssignStudentsAtGuildCommand(user);
                    break;
                case "asignarcréditosalgremio":
                     AssignCretidAtGuildCommand(user);
                    break;
                case "eliminarestudiantedelgremio":
                     DeleteStudentFromGuildCommand(user);
                    break;
                case "eliminargremio":
                     DeleteGuildCommand(user);
                    break;
                case "detallesdelgremio":
                     DetailsGuildCommand(user);
                    break;
                case "eliminarclasepráctica":
                     PracticalClassDeleteCommand(user);
                    break;
                default:
                     DefaultCommand(user, cmd);
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
                if(user.Status == UserStatus.Ready)
                {
                     Logger.Error($"Error: El usuario {user.Username} está intentando registrarse nuevamente en el bot.");
                    if (user.IsTecaher)
                    {
                         Menu.TeacherMenu(bot, message);
                    }
                    else
                    {
                         Menu.StudentMenu(bot, message);
                    }
                }
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
            if (user.Status != UserStatus.Verified && user.Status != UserStatus.ChangeClassRoom)
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
            if (user.Status != UserStatus.Verified && user.Status != UserStatus.ChangeClassRoom)
            {
                 Logger.Error($"Error: El usuario {user.Username} está intentando convertirse en profesor sin estar verificado.");
                 bot.SendMessage(chatId: message.Chat.Id,
                            text: "Por favor, atienda lo que hace, no me haga perder tiempo, inserte su nombre y 2 apellidos.");
                return;
            }
            if(user.Status == UserStatus.Verified)
                 teacherDataHandler.CreateTeacher(user);
            var keyboard = new ReplyKeyboardMarkup
            {
                Keyboard = new KeyboardButton[][]{
                                            new KeyboardButton[]{
                                                new KeyboardButton("*Crear*"),
                                                new KeyboardButton("*Entrar*")
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
            if (user.Status != UserStatus.CreatingTecaher && user.Status != UserStatus.ChangeClassRoom)
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
            if (user.Status != UserStatus.CreatingTecaher && user.Status != UserStatus.ChangeClassRoom)
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
                var res =  studentDataHandler.GetStudentsOnClassByTeacherId(user.Id);
                 Menu.TeacherMenu(bot, message, res);
            }
        }

        private void CreditsCommand(Models.User? user)
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
                var text =  creditsDataHandler.GetCreditsById(user.Id);
                 Menu.StudentMenu(bot, message, text);
                return;
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
                var pendings =  pendingDataHandler.GetPendings(user);
                 Menu.PendingsFilters(bot, message, pendings.Item1, pendings.Item2);
            }
        }

        private void AllClassRoomWithPendingsCommand(Models.User? user)
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
                var res =  pendingDataHandler.GetAllClassRoomWithPendings(user);
                if (string.IsNullOrEmpty(res))
                    res = "No tienen pendientes en ninguna de sus aulas";
                 Menu.TeacherConfigurationMenu(bot, message, res);
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
                var classes =  classTitleDataHandler.GetClasses(user);
                 classTitleDataHandler.ChangeClassTitle(user);
                 Menu.ClassesList(bot, message, classes);
            }
        }

        private void DefaultCommand(Models.User? user, string command)
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
                string file = "";
                bool giveMeExplication = false;
                var pending =  pendingDataHandler.GetPendingByCode(command, file, giveMeExplication);

                if (!string.IsNullOrEmpty(pending.Item1))
                {
                    var type =  pendingDataHandler.GetPending(command);
                    if (type.Type == InteractionType.Diary)
                    {
                         Menu.PendingDiaryCommands(bot, message, pending.Item1, pending.Item3, command, user);
                        return;
                    }
                }

                var teachers =  teacherDataHandler.GetTeachers(user);

                if(teachers.Count() != 0)
                {
                    if ( Menu.PendingCommands(bot, message, pending.Item1, teachers, pending.Item3, command, user, pending.Item2))
                        return;
                }

                var student =  creditsDataHandler.GetCreditsByUserName(user.Id, command, true, true);
                if (!string.IsNullOrEmpty(student))
                {
                     Menu.TeacherMenu(bot, message, student);
                    return;
                }
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
                var status =  statusPhraseDataHandler.ChangeStatusPhrase(user);
                 bot.SendMessage(chatId: message.Chat.Id,
                            text: $"Su frase de estado actual es: {status}",
                            replyMarkup: new ReplyKeyboardRemove());
                 Menu.CancelMenu(bot, message, "Inserte la nueva frase de estado:");
            }
        }

        private void DiaryCommand(Models.User? user)
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
                 diaryDataHandler.UpdateDiary(user);
                 Menu.CancelMenu(bot, message, "Inserte la actualización de su diario:");
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
                 rectificationToTheTeacherDataHandler.DoRectificationToTheTaecher(user);
                var teachers =  teacherDataHandler.GetTeachers(user);
                 Menu.TeachersList(bot, message, teachers, "Seleccione el profesor al que desea rectificar:");
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
                 Menu.CancelMenu(bot, message, "Haga el chiste:");
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
                 Menu.CancelMenu(bot, message, "Inserte un meme:");
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
                 classInterventionDataHandler.CreateIntervention(user);
                var classes =  classTitleDataHandler.GetClasses(user);
                 Menu.ClassesList(bot, message, classes);
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
                var res =  studentDataHandler.RemoveStudentFromClassRoom(user.Id);
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
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando changeclass");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var res =  classRoomDataHandler.ChangeClassRoom(user.Id);
                user.Status = UserStatus.ChangeClassRoom;
                 userDataHandler.VerifyUser(user);
                 Menu.ChangeClassRoomMenu(bot, message, res);
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
                var accessKey =  teacherDataHandler.GetTeacherAccessKey(user.Id);
                 Menu.TeacherConfigurationMenu(bot, message, $"La clave de acceso para sus profesores es {accessKey}.");
            }
        }

        private void StartClassCommand(Models.User? user)
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
                 classTitleDataHandler.CreateClass(user);
                 Menu.CancelMenu(bot, message, "Inserte el título de la clase:");
            }
        }

        private void AssignCreditsCommand(Models.User? user)
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
            else if (user.IsTecaher)
            {
                 creditsDataHandler.AssignCredit(user);
                 Menu.CancelMenu(bot, message, "Inserte el nombre de usuario del estudiante:");
                return;
            }
            else
            {
                 Logger.Error($"Error: El usuario {user.Username} está intentando intentando registrar su nombre y apellidos en formato incorrecto.");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Por favor, no me haga perder el tiempo, inserte su nombre y sus 2 apellidos.");
                return;
            }
        }

        private void SeeClassRoomActiveCommand(Models.User? user)
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
            else
            {
                var res = classRoomDataHandler.SeeClassRoomActive(user);
                if (user.IsTecaher)
                     Menu.TeacherMenu(bot, message, "Se encuentra en el aula: " + res);
                else
                     Menu.StudentMenu(bot, message, "Se encuentra en el aula: " + res);
                return;
            }
        }

        private void SeeListClass(Models.User? user)
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
            else
            {
                var res = classRoomDataHandler.GetClassesCreated(user);
                var classRoom =  classRoomDataHandler.SeeClassRoomActive(user);
                if (user.IsTecaher)
                     Menu.TeacherMenu(bot, message, $"El aula {classRoom} tiene creadas las clases:\n{res}");
                else
                     Menu.StudentMenu(bot, message, $"El aula {classRoom} tiene creadas las clases:\n{res}");
                return;
            }
        }

        private void DirectPendingCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var pendings =  pendingDataHandler.GetPendings(user, true);
                var inline = new List<InlineKeyboardButton>();
                if (2 <= pendings.Item2)
                    inline.Add(new InlineKeyboardButton
                    {
                        CallbackData = $"NextPending//2//{(int)InteractionType.None}",
                        Text = $">>2/{pendings.Item2}"
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
        }

        private void AssignMemeChannelCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                 classRoomDataHandler.AssignMemeChannel(user);
                 Menu.CancelMenu(bot, message, "Inserte el Username del canal en el que se publicarán los memes(Tenga presente que el bot debe ser miembro del canal y con privilegios de Administrador):");
            }
        }

        private void AssignJokesChannelCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                 classRoomDataHandler.AssignJokesChannel(user);
                 Menu.CancelMenu(bot, message, "Inserte el Username del canal en el que se publicarán los chistes(Tenga presente que el bot debe ser miembro del canal y con privilegios de Administrador):");
            }
        }

        private void AssignClassInterventionChannelCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                 classRoomDataHandler.AssignClassInterventionChannel(user);
                 Menu.CancelMenu(bot, message, "Inserte el Username del canal en el que se publicarán las intervenciones en clase(Tenga presente que el bot debe ser miembro del canal y con privilegios de Administrador):");
            }
        }

        private void AssignDiaryChannelCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                 classRoomDataHandler.AssignDiaryChannel(user);
                 Menu.CancelMenu(bot, message, "Inserte el Username del canal en el que se publicarán las actualizaciones a los diarios(Tenga presente que el bot debe ser miembro del canal y con privilegios de Administrador):");
            }
        }

        private void AssignStatusPhraseChannelCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                 classRoomDataHandler.AssignStatusPhraseChannel(user);
                 Menu.CancelMenu(bot, message, "Inserte el Username del canal en el que se publicarán las frases de estado(Tenga presente que el bot debe ser miembro del canal y con privilegios de Administrador):");
            }
        }

        private void AssignClassTitleChannelCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                 classRoomDataHandler.AssignClassTitleChannel(user);
                 Menu.CancelMenu(bot, message, "Inserte el Username del canal en el que se publicarán las propuestas de títulos de clases(Tenga presente que el bot debe ser miembro del canal y con privilegios de Administrador):");
            }
        }

        private void AssignRectificationToTheTeacherChannelCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                 classRoomDataHandler.AssignRectificationToTheTeacherChannel(user);
                 Menu.CancelMenu(bot, message, "Inserte el Username del canal en el que se publicarán las rectificaciones a los profesores(Tenga presente que el bot debe ser miembro del canal y con privilegios de Administrador):");
            }
        }

        private void SendStudentsInformationCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                 classRoomDataHandler.SendInformationToTheStudents(user);
                 Menu.CancelMenu(bot, message, "Inserte la información que le quiere enviar a sus estudiantes:");
            }
        }

        private void CreateMiscellaneousCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                 miscellaneousDataHandler.CreateMiscellaneous(user);
                 Menu.CancelMenu(bot, message, "Inserte la miscelánea que quiere proponer:");
            }
        }

        private void AssignMiscellaneousChannelCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Asignar Canal de Misceláneas");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                 classRoomDataHandler.AssignMiscellaneousChannel(user);
                 Menu.CancelMenu(bot, message, "Inserte el Username del canal en el que se publicarán las misceláneas(Tenga presente que el bot debe ser miembro del canal y con privilegios de Administrador):");
            }
        }

        private void CreatePracticClassCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Crear Clase Práctica");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                 practicClassDataHandler.CreatePracticClass(user);
                 Menu.CancelMenu(bot, message, "Inserte la Clase Práctica en el formato siguiente: \n CP1 1 10000 1a 20000 2a 400000 ......");
            }
        }

        private void ReviewPracticClassCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Crear Clase Práctica");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var practicalClasses =  practicClassDataHandler.GetPracticClasses(user);
                 Menu.CancelMenu(bot, message);
                 Menu.PracticalClassList(bot, message, practicalClasses, "Seleccione una Clase Práctica:");
            }
        }

        private void EditPracticalClassNameCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Crear Clase Práctica");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var practicalClasses =  practicClassDataHandler.EditPracticalClassName(user);
                 Menu.CancelMenu(bot, message, practicalClasses);
            }
        }

        private void CreditsStatusCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Estado de Creditos");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var credits =  creditsDataHandler.GetCreditListOfUser(user);
                 Menu.StudentMenu(bot, message, credits);
            }
        }

        private void CreateGuildCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Crear Gremio");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                 guildDataHandler.CreateGuild(user);
                 Menu.CancelMenu(bot, message, "Inserte el nombre del Gremio:");
            }
        }

        private void AssignStudentsAtGuildCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Asignar Estudiante al Gremio");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var guilds =  guildDataHandler.AssignStudentAtGuild(user);
                 Menu.CancelMenu(bot, message);
                 Menu.GuildList(bot, message, guilds, "Selecciona el Gremio a dónde va a añadir el estudiante");
            }
        }

        private void AssignCretidAtGuildCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Asignar Créditos al Gremio");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var guilds =  guildDataHandler.AssignCreditsAtGuild(user);
                 Menu.CancelMenu(bot, message);
                 Menu.GuildList(bot, message, guilds, "Selecciona el Gremio que desea asignar créditos");
            }
        }

        private void DeleteStudentFromGuildCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Eliminar Estudiante del Gremio");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var guilds =  guildDataHandler.DeleteStudentFromGuild(user);
                 Menu.CancelMenu(bot, message);
                 Menu.GuildList(bot, message, guilds, "Selecciona el Gremio donde está el estudiante que desea elminar");
            }
        }

        private void DeleteGuildCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Eliminar Gremio");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var guilds =  guildDataHandler.DeleteGuild(user);
                 Menu.CancelMenu(bot, message);
                 Menu.GuildList(bot, message, guilds, "Selecciona el Gremio que desea elminar");
            }
        }

        private void DetailsGuildCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Detalles del Gremio");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var guilds =  guildDataHandler.DetailsGuild(user);
                 Menu.CancelMenu(bot, message);
                 Menu.GuildList(bot, message, guilds, "Seleccione un Gremio para consultar sus detalles");
            }
        }

        private void PracticalClassDeleteCommand(Models.User? user)
        {
            if (user == null)
            {
                 Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                 Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Estado de Creditos");
                 bot.SendMessage(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var practicalClasses =  practicClassDataHandler.DeletePracticClasses(user);
                 Menu.CancelMenu(bot, message);
                 Menu.PracticalClassList(bot, message, practicalClasses, "Seleccione una Clase Práctica:");
            }
        }
        #endregion

        #region Procesos que completan comandos de varias operaciones
        private void OnRegister(Models.User user, string text)
        {
            var list = text.Split(' ');

            user.Name = text;
             userDataHandler.VerifyUser(user);
             Logger.Success($"Verifying the user {user.Username}");

             bot.SendMessage(chatId: message.Chat.Id,
                            text: "Bienvenido " + text,
                            replyMarkup: new ReplyKeyboardRemove());

             Menu.RegisterMenu(bot, message);
        }

        private bool OnAssignStudentAtClass(long id, string text)
        {
            var success = false;
            var res = studentDataHandler.AssignStudentAtClass(id, text, success);
             bot.SendMessage(chatId: message.Chat.Id,
                            text: res.Item1,
                            replyMarkup: new ReplyKeyboardRemove());
            return res.Item2;
        }

        private bool OnAssignTeacherAtClass(long id, string text)
        {
            bool success = false;
            var res = teacherDataHandler.AssignTeacherAtClass(id, text, success);
             bot.SendMessage(chatId: message.Chat.Id,
                            text: res.Item1,
                            replyMarkup: new ReplyKeyboardRemove());
            return res.Item2;
        }

        private void OnCreateClassRoom(long id, string name)
        {
            var res =  classRoomDataHandler.CreateClassRoom(id, name);
             bot.SendMessage(chatId: message.Chat.Id,
                            text: res,
                            replyMarkup: new ReplyKeyboardRemove());
        }

        private void OnRectificationToTheTeacherAtTeacherUserName(Models.User user, string teacherUserName)
        {
             rectificationToTheTeacherDataHandler.DoRectificationToTheTaecherUserName(user, teacherUserName);
             Menu.CancelMenu(bot, message, "Explique a grosso modo en qué se equivocó el profesor y su correción:");
        }

        private void OnRectificationToTheTeacherAtText(Models.User user, string text)
        {
            var res =  rectificationToTheTeacherDataHandler.DoRectificationToTheTaecherText(user, text);
             bot.SendMessage(chatId: message.Chat.Id,
                            text: res,
                            replyMarkup: new ReplyKeyboardRemove());
        }

        private void OnStartClass(Models.User user, string classTitle)
        {
             classTitleDataHandler.CreateClass(user, classTitle);
             bot.SendMessage(chatId: message.Chat.Id,
                            text: "Clase creada satisfactoriamente.",
                            replyMarkup: new ReplyKeyboardRemove());
        }

        private void OnChangeClassTitle(Models.User user, long classId)
        {
             classTitleDataHandler.ChangeClassTitle(user, classId);
             Menu.CancelMenu(bot, message, "Inserte el nombre la clase:");
        }

        private void OnChangeClassTitle(Models.User user, string classTitle)
        {
             classTitleDataHandler.ChangeClassTitle(user, classTitle);
             bot.SendMessage(chatId: message.Chat.Id,
                            text: "Título propuesto satisfactoriamente.",
                            replyMarkup: new ReplyKeyboardRemove());
        }

        private void OnClassIntervention(Models.User user, long classId)
        {
             classInterventionDataHandler.CreateIntervention(user, classId);
             Menu.CancelMenu(bot, message, "Inserte su intervención:");
        }

        private void OnClassIntervention(Models.User user, string classIntervention)
        {
             classInterventionDataHandler.CreateIntervention(user, classIntervention);
             bot.SendMessage(chatId: message.Chat.Id,
                            text: "Intervención hecha satisfactoriamente.",
                            replyMarkup: new ReplyKeyboardRemove());
        }

        private void OnPendings(Models.User user, Message message)
        {
            var response = message.Text.Split(' ');
            long credits = 0;
            var canParse = long.TryParse(response[0], out credits);
            if (canParse)
            {
                var text = "";
                var pendingCode = "";
                if (!string.IsNullOrEmpty(message.ReplyToMessage.Text))
                    pendingCode = message.ReplyToMessage.Text.Split(" /")[1];
                else
                    pendingCode = message.ReplyToMessage.Caption.Split(" /")[1];
                var pending =  pendingDataHandler.GetPending(pendingCode);
                var comment = "";

                if (pending.Type == InteractionType.ClassIntervention)
                {
                    var data =  classInterventionDataHandler.GetClassIntenvention(pending.ObjectId);
                    comment = data.Text;
                }
                else if (pending.Type == InteractionType.ClassTitle)
                {
                    var data =  classTitleDataHandler.GetClassTitle(pending.ObjectId);
                    comment = data.Title;
                }
                else if (pending.Type == InteractionType.Diary)
                {
                    var data =  diaryDataHandler.GetDiary(pending.ObjectId);
                    comment = data.Text;
                }
                else if (pending.Type == InteractionType.Joke)
                {
                    var data =  jokeDataHandler.GetJoke(pending.ObjectId);
                    comment = data.Text;
                }
                else if (pending.Type == InteractionType.RectificationToTheTeacher)
                {
                    var data =  rectificationToTheTeacherDataHandler.GetRectificationToTheTeacher(pending.ObjectId);
                    comment = data.Text;
                }
                else if (pending.Type == InteractionType.StatusPhrase)
                {
                    var data =  statusPhraseDataHandler.GetStatusPhrase(pending.ObjectId);
                    comment = data.Phrase;
                }
                else if(pending.Type == InteractionType.Miscellaneous)
                {
                    var data =  miscellaneousDataHandler.GetMiscellaneous(pending.ObjectId);
                    comment = data.Text;
                }
                string type = "";

                if (pending.Type == InteractionType.ClassIntervention)
                {
                    type = "Intervención en Clases";
                }
                else if (pending.Type == InteractionType.ClassTitle)
                {
                    type = "Cambio de Título a la Clase";
                }
                else if (pending.Type == InteractionType.Diary)
                {
                    type = "Actualización del Diario";
                }
                else if (pending.Type == InteractionType.Joke)
                {
                    type = "Chiste";
                }
                else if (pending.Type == InteractionType.RectificationToTheTeacher)
                {
                    type = "Rectificación a los Profesores";
                }
                else if (pending.Type == InteractionType.StatusPhrase)
                {
                    type = "Cambio de Frase de Estado";
                }
                else if (pending.Type == InteractionType.Miscellaneous)
                {
                    type = "Miscelánea";
                }
                else if (pending.Type == InteractionType.Meme)
                {
                    type = "Meme";
                }

                if (response.Length != 1 && !string.IsNullOrEmpty(comment))
                {
                    var res = response[1];
                    for (int i = 2; i < response.Length; i++)
                    {
                        res += " ";
                        res += response[i];
                    }
                    text = $"Ha recibido {credits} créditos por su {type} ({comment}) y su profesor le ha hecho la siguiente recomendación: \"{res}\".";
                }
                else if (response.Length != 1 && string.IsNullOrEmpty(comment))
                {
                    var res = response[1];
                    for (int i = 2; i < response.Length; i++)
                    {
                        res += " ";
                        res += response[i];
                    }
                    text = $"Ha recibido {credits} créditos por su {type} y su profesor le ha hecho la siguiente recomendación: \"{res}\".";
                }
                else if (response.Length == 1 && !string.IsNullOrEmpty(comment))
                    text = $"Ha recibido {credits} créditos por su {type} ({comment}).";
                else
                    text = $"Ha recibido {credits} créditos por su {type}.";
                 bot.SendMessage(chatId: pending.Student.User.ChatId,
                                text: text);
                string credit_information = "";
                if (response.Length != 1)
                {
                    for (int i = 1; i < response.Length; i++)
                    {
                        credit_information += response[i] + " ";
                    }
                }
                 creditsDataHandler.AddCredits(credits, user.Id, pending.ObjectId, pending.Student.User.Id, pending.ClassRoomId, credit_information);
                 pendingDataHandler.RemovePending(pending);
                 PendingsCommand(user);

                if (pending.Type == InteractionType.Meme)
                {
                    var meme =  memeDataHandler.GetMeme(pending.ObjectId);
                    if (!string.IsNullOrEmpty( classRoomDataHandler.GetMemeChannel(user)))
                        bot.SendPhoto(chatId:  classRoomDataHandler.GetMemeChannel(user),
                            photo: meme.FileId,
                            caption: "Meme enviado por: @" + pending.Student.User.Username);
                }

                /*if (pending.Type == InteractionType.ClassIntervention)
                {
                    comment = classInterventionDataHandler.GetClassIntenvention(pending.ObjectId).Text;
                    if (!string.IsNullOrEmpty(classRoomDataHandler.GetClassInterventionChannel(user)))
                         bot.SendMessage(chatId: classRoomDataHandler.GetClassInterventionChannel(user),
                            text: "Intervención en Clase enviada por: @" + pending.Student.User.Username + "\n\"" + comment + "\"");
                }
                else if (pending.Type == InteractionType.ClassTitle)
                {
                    comment = classTitleDataHandler.GetClassTitle(pending.ObjectId).Title;
                    if (!string.IsNullOrEmpty(classRoomDataHandler.GetClassTitleChannel(user)))
                         bot.SendMessage(chatId: classRoomDataHandler.GetClassTitleChannel(user),
                            text: "Cambio de Título de Clase enviado por: @" + pending.Student.User.Username + "\n\"" + comment + "\"");
                }
                else if (pending.Type == InteractionType.Diary)
                {
                    comment = diaryDataHandler.GetDiary(pending.ObjectId).Text;
                    if (!string.IsNullOrEmpty(classRoomDataHandler.GetDiaryChannel(user)))
                         bot.SendMessage(chatId: classRoomDataHandler.GetDiaryChannel(user),
                            text: "Actualización al Diario enviado por: @" + pending.Student.User.Username + "\n\"" + comment + "\"");
                }
                else if (pending.Type == InteractionType.Joke)
                {
                    comment = jokeDataHandler.GetJoke(pending.ObjectId).Text;
                    if (!string.IsNullOrEmpty(classRoomDataHandler.GetJokesChannel(user)))
                         bot.SendMessage(chatId: classRoomDataHandler.GetJokesChannel(user),
                            text: "Chiste enviado por: @" + pending.Student.User.Username + "\n\"" + comment + "\"");
                }
                else if (pending.Type == InteractionType.RectificationToTheTeacher)
                {
                    comment = rectificationToTheTeacherDataHandler.GetRectificationToTheTeacher(pending.ObjectId).Text;
                    if (!string.IsNullOrEmpty(classRoomDataHandler.GetRectificationToTheTeacherChannel(user)))
                         bot.SendMessage(chatId: classRoomDataHandler.GetRectificationToTheTeacherChannel(user),
                            text: "Rectificación al Profesor enviada por: @" + pending.Student.User.Username + "\n\"" + comment + "\"");
                }
                else if (pending.Type == InteractionType.StatusPhrase)
                {
                    comment = statusPhraseDataHandler.GetStatusPhrase(pending.ObjectId).Phrase;
                    if (!string.IsNullOrEmpty(classRoomDataHandler.GetStatusPhraseChannel(user)))
                         bot.SendMessage(chatId: classRoomDataHandler.GetStatusPhraseChannel(user),
                            text: "Frase de Estado enviada por: @" + pending.Student.User.Username + "\n\"" + comment + "\"");
                }
                else if(pending.Type == InteractionType.Miscellaneous)
                {
                    comment = miscellaneousDataHandler.GetMiscellaneous(pending.ObjectId).Text;
                    if (!string.IsNullOrEmpty(classRoomDataHandler.GetMiscellaneousChannel(user)))
                         bot.SendMessage(chatId: classRoomDataHandler.GetMiscellaneousChannel(user),
                            text: "Miscelánea enviada por: @" + pending.Student.User.Username + "\n\"" + comment + "\"");
                }*/
            }
            else
            {
                var pendingCode = "";
                if (!string.IsNullOrEmpty(message.ReplyToMessage.Text))
                    pendingCode = message.ReplyToMessage.Text.Split(" /")[1];
                else
                    pendingCode = message.ReplyToMessage.Caption.Split(" /")[1];
                var pending =  pendingDataHandler.GetPending(pendingCode);
                foreach (var username in response)
                {
                    if ( teacherDataHandler.ExistTeacher(username))
                    {
                        var teacherChatId =  pendingDataHandler.AddDirectPending(username, pending.Id);
                         bot.SendMessage(chatId: teacherChatId,
                            text: "Le han asignado un pendiente que tiene que revisar.");
                    }
                    else
                    {
                         bot.SendMessage(chatId: user.ChatId,
                            text: $"No existe un usuario con el user name {username}.");
                    }
                }
                 PendingsCommand(user);
            }
        }

        private void OnAssignCredits(Models.User user, string username)
        {
            if(! creditsDataHandler.AssignCredit(user, username))
            {
                 Menu.TeacherMenu(bot, message);
            }
            else
                 Menu.CancelMenu(bot, message, "Inserte los créditos y su mensaje:");
        }

        private void OnAssignCreditsMessage(Models.User user, string text)
        {
            var res =  creditsDataHandler.AssignCreditMessage(user, text);
             bot.SendMessage(chatId: res.Item2,
                            text: $"Ha recibido {res.Item3} créditos y su profesor le dijo \"{res.Item1}\".");
             Menu.TeacherMenu(bot, message, "Créditos asignados satisfactoriamente.");
        }
        #endregion
    }
}