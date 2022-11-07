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

        public async Task ProcessCommand(Message message)
        {
            if (message.From == null)
            {
                await Logger.Error($"Error: Mensaje con usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            // Ignore user 777000 (Telegram)s
            if (message.From.Id == 777000)
            {
                return;
            }
            await Logger.Warning($"New message from chat id: {(string.IsNullOrEmpty(message.From.Username) ? message.Chat.Id : message.From.Username )}");

            appUser = message.From; // Save current user;
            this.message = message; // Save current message;
            hasText = !string.IsNullOrEmpty(message.Text); // True if message has text;

            await Logger.Warning($"Message Text: {(hasText ? message.Text : "|:O")}");
            var user = await userDataHandler.GetUser(appUser);

            if (user != null && user.ClassRoomActiveId == 0 && user.Status!=UserStatus.Verified && user.Status!=UserStatus.StudentEnteringClass && user.Status!=UserStatus.CreatingTecaher && user.Status!=UserStatus.TeacherCreatingClass && user.Status!=UserStatus.TeacherEnteringClass)
            {
                await userDataHandler.VerifyUser(user);
                await Menu.RegisterMenu(bot, message);
                return;
            }

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
                        await Logger.Command($"New command: {command}");
                        await OnCommand(command, parameters, user);
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

                    await Logger.Command($"New command: {command}");
                    await OnCommand(command.ToString(), new string[0], user);
                }
                else if (message.Text.StartsWith("*") && message.Text.EndsWith("*") && message.Text.Length >= 4 && message.Text.ToString().Contains("*//*"))
                {
                    var commands = message.Text.Substring(1, message.Text.Length - 2).Split(' ');
                    var command = new StringBuilder();

                    foreach (var item in commands)
                    {
                        command.Append(item);
                    }

                    await Logger.Command($"New command: {command}");
                    if (user.Status == UserStatus.RectificationAtTeacher)
                    {
                        await OnRectificationToTheTeacherAtTeacherUserName(user, command.ToString().Split("*//*")[1]);
                    }
                    else if(user.Status == UserStatus.ClassIntervention)
                    {
                        await OnClassIntervention(user, long.Parse(command.ToString().Split("*//*")[0]));
                    }
                    else if(user.Status == UserStatus.ClassTitle)
                    {
                        await OnChangeClassTitle(user, long.Parse(command.ToString().Split("*//*")[0]));
                    }
                    else
                    {
                        await Logger.Error($"Error: El usuario {user.Username} está escribiendo cosas sin sentido");
                        await bot.SendMessageAsync(chatId: message.Chat.Id,
                                        text: "Por favor, atienda lo que hace, no me haga perder tiempo.");
                    }
                }
                else
                {
                    switch (user.Status)
                    {
                        case UserStatus.Created:
                            await OnRegister(user, message.Text);
                            return;
                        case UserStatus.StudentEnteringClass:
                            var assignedStudentSuccesfully = await OnAssignStudentAtClass(appUser.Id, message.Text);
                            if(assignedStudentSuccesfully)
                                await Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.Pending:
                            await OnPendings(user, message);
                            return;
                        case UserStatus.TeacherEnteringClass:
                            var assignedTeacherSuccesfully = await OnAssignTeacherAtClass(appUser.Id, message.Text);
                            if(assignedTeacherSuccesfully)
                                await Menu.TeacherMenu(bot, message);
                            return;
                        case UserStatus.TeacherCreatingClass:
                            await OnCreateClassRoom(appUser.Id, message.Text);
                            await Menu.TeacherMenu(bot, message);
                            return;
                        case UserStatus.Credits:
                            var res1 = await creditsDataHandler.GetCreditsByUserName(user.Id, message.Text, false, true);
                            await Menu.TeacherMenu(bot, message, res1);
                            return;
                        case UserStatus.StatusPhrase:
                            await statusPhraseDataHandler.ChangeStatusPhrase(user.Id, message.Text);
                            if (!string.IsNullOrEmpty(await classRoomDataHandler.GetStatusPhraseChannel(user)))
                                await bot.SendMessageAsync(chatId: await classRoomDataHandler.GetStatusPhraseChannel(user),
                                    text: "Frase de Estado enviada por: @" + user.Username + "\n\"" + message.Text + "\"");
                            await Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.Joke:
                            await jokeDataHandler.DoJoke(user.Id, message.Text);
                            if (!string.IsNullOrEmpty(await classRoomDataHandler.GetJokesChannel(user)))
                                await bot.SendMessageAsync(chatId: await classRoomDataHandler.GetJokesChannel(user),
                                    text: "Chiste enviado por: @" + user.Username + "\n\"" + message.Text + "\"");
                            await Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.Diary:
                            await diaryDataHandler.UpdateDiary(user.Id, message.Text);
                            if (!string.IsNullOrEmpty(await classRoomDataHandler.GetDiaryChannel(user)))
                                await bot.SendMessageAsync(chatId: await classRoomDataHandler.GetDiaryChannel(user),
                                    text: "Actualización del Diario enviada por: @" + user.Username + "\n\"" + message.Text + "\"");
                            await Logger.Success($"The student {user.Username} is ready to update Diary");
                            await Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.RemoveStudentFromClassRoom:
                            var res = await studentDataHandler.RemoveStudentFromClassRoom(user.Id, message.Text);
                            await bot.SendMessageAsync(chatId: res.Item2,
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
                            await Menu.TeacherMenu(bot, message, res.Item1);
                            return;
                        case UserStatus.ClassIntervention:
                            await classInterventionDataHandler.CreateIntervention(user, message.Text);
                            await Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.RectificationToTheTeacherUserName:
                            await OnRectificationToTheTeacherAtText(user, message.Text);
                            if (!string.IsNullOrEmpty(await classRoomDataHandler.GetRectificationToTheTeacherChannel(user)))
                                await bot.SendMessageAsync(chatId: await classRoomDataHandler.GetRectificationToTheTeacherChannel(user),
                                    text: "Rectificación al Profesor enviada por: @" + user.Username + "\n\"" + message.Text + "\"");
                            await Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.CreateClass:
                            await OnStartClass(user, message.Text);
                            await Menu.TeacherMenu(bot, message);
                            return;
                        case UserStatus.ClassTitleSelect:
                            if (!string.IsNullOrEmpty(await classRoomDataHandler.GetClassTitleChannel(user)))
                                await bot.SendMessageAsync(chatId: await classRoomDataHandler.GetClassTitleChannel(user),
                                    text: "Cambio de Título de Clase enviada por: @" + user.Username + "\n\"" + message.Text + "\"");
                            await OnChangeClassTitle(user, message.Text);
                            await Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.ClassInterventionSelect:
                            await OnClassIntervention(user, message.Text);
                            if (!string.IsNullOrEmpty(await classRoomDataHandler.GetClassInterventionChannel(user)))
                                await bot.SendMessageAsync(chatId: await classRoomDataHandler.GetClassInterventionChannel(user),
                                    text: "Intervención en Clase enviada por: @" + user.Username + "\n\"" + message.Text + "\"");
                            await Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.AssignCreditsStudent:
                            await OnAssignCredits(user, message.Text);
                            return;
                        case UserStatus.AssignCredits:
                            await OnAssignCreditsMessage(user, message.Text);
                            await Menu.TeacherMenu(bot, message);
                            return;
                        case UserStatus.ChangeClassRoom:
                            var outPut = 0;
                            var canParser = int.TryParse(message.Text, out outPut);
                            if (!canParser)
                            {
                                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                                text: "Por favor, atienda lo que hace, necesito un ID de una clase.");
                                return;
                            }
                            await classRoomDataHandler.ChangeClassRoom(user.Id, outPut);
                              if (user.IsTecaher)
                                await Menu.TeacherMenu(bot, message);
                            else
                                await Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.AssignMemeChannel:
                            await classRoomDataHandler.AssignMemeChannel(user, message.Text);
                            var memeChannelConfigurationText = "Canal de Meme asignado satisfactoriamente";
                            try
                            {
                                await bot.SendMessageAsync(chatId: message.Text,
                                    text: "Bot configurado satisfactoriamente.");
                            }
                            catch
                            {
                                memeChannelConfigurationText = "El nombre de usuario del bot insertado no existe";
                            }
                            await Menu.TeacherConfigurationMenu(bot, message, memeChannelConfigurationText);
                            return;
                        case UserStatus.AssignJokeChannel:
                            await classRoomDataHandler.AssignJokesChannel(user, message.Text);
                            var jokeChannelConfigurationText = "Canal de Chistes asignado satisfactoriamente";
                            try
                            {
                                await bot.SendMessageAsync(chatId: message.Text,
                                    text: "Bot configurado satisfactoriamente.");
                            }
                            catch
                            {
                                jokeChannelConfigurationText = "El nombre de usuario del bot insertado no existe";
                            }
                            await Menu.TeacherConfigurationMenu(bot, message, jokeChannelConfigurationText);
                            return;
                        case UserStatus.AssignClassInterventionChannel:
                            await classRoomDataHandler.AssignClassInterventionChannel(user, message.Text);
                            var classInterventionsChannelConfigurationText = "Canal de Intervenciones en Clases asignado satisfactoriamente";
                            try
                            {
                                await bot.SendMessageAsync(chatId: message.Text,
                                    text: "Bot configurado satisfactoriamente.");
                            }
                            catch
                            {
                                classInterventionsChannelConfigurationText = "El nombre de usuario del bot insertado no existe";
                            }
                            await Menu.TeacherConfigurationMenu(bot, message, classInterventionsChannelConfigurationText);
                            return;
                        case UserStatus.AssignClassTitleChannel:
                            await classRoomDataHandler.AssignClassTitleChannel(user, message.Text);
                            var classTitlesChannelConfigurationText = "Canal de propuestas de Títulos de Clases asignado satisfactoriamente";
                            try
                            {
                                await bot.SendMessageAsync(chatId: message.Text,
                                    text: "Bot configurado satisfactoriamente.");
                            }
                            catch
                            {
                                classTitlesChannelConfigurationText = "El nombre de usuario del bot insertado no existe";
                            }
                            await Menu.TeacherConfigurationMenu(bot, message, classTitlesChannelConfigurationText);
                            return;
                        case UserStatus.AssignDiaryChannel:
                            await classRoomDataHandler.AssignDiaryChannel(user, message.Text);
                            var diaryChannelConfigurationText = "Canal de Actualizaciones de Diario asignado satisfactoriamente";
                            try
                            {
                                await bot.SendMessageAsync(chatId: message.Text,
                                    text: "Bot configurado satisfactoriamente.");
                            }
                            catch
                            {
                                diaryChannelConfigurationText = "El nombre de usuario del bot insertado no existe";
                            }
                            await Menu.TeacherConfigurationMenu(bot, message, diaryChannelConfigurationText);
                            return;
                        case UserStatus.AssignRectificationToTheTeacherChannel:
                            await classRoomDataHandler.AssignRectificationToTheTeacherChannel(user, message.Text);
                            var rectificationToTheTeacherChannelConfigurationText = "Canal de Rectificaciones a los profesores asignado satisfactoriamente";
                            try
                            {
                                await bot.SendMessageAsync(chatId: message.Text,
                                    text: "Bot configurado satisfactoriamente.");
                            }
                            catch
                            {
                                rectificationToTheTeacherChannelConfigurationText = "El nombre de usuario del bot insertado no existe";
                            }
                            await Menu.TeacherConfigurationMenu(bot, message, rectificationToTheTeacherChannelConfigurationText);
                            return;
                        case UserStatus.AssignStatusPhraseChannel:
                            await classRoomDataHandler.AssignStatusPhraseChannel(user, message.Text);
                            var statusPhraseChannelConfigurationText = "Canal de Frases de Estado asignado satisfactoriamente";
                            try
                            {
                                await bot.SendMessageAsync(chatId: message.Text,
                                    text: "Bot configurado satisfactoriamente.");
                            }
                            catch
                            {
                                statusPhraseChannelConfigurationText = "El nombre de usuario del bot insertado no existe";
                            }
                            await Menu.TeacherConfigurationMenu(bot, message, statusPhraseChannelConfigurationText);
                            return;
                        case UserStatus.AssignMiscellaneousChannel:
                            await classRoomDataHandler.AssignMiscellaneousChannel(user, message.Text);
                            var miscellaneousChannelConfigurationText = "Canal de Misceláneas asignado satisfactoriamente";
                            try
                            {
                                await bot.SendMessageAsync(chatId: message.Text,
                                    text: "Bot configurado satisfactoriamente.");
                            }
                            catch
                            {
                                miscellaneousChannelConfigurationText = "El nombre de usuario del bot insertado no existe";
                            }
                            await Menu.TeacherConfigurationMenu(bot, message, miscellaneousChannelConfigurationText);
                            return;
                        case UserStatus.SendInformation:
                            var students = await classRoomDataHandler.GetStudentsOnClassRoom(user);
                            foreach (var student in students)
                            {
                                await bot.SendMessageAsync(chatId: student.Student.User.ChatId,
                                    text: message.Text);
                            }
                            var teachers = await classRoomDataHandler.GetTeachersOnClassRoom(user);
                            foreach (var teacher in teachers)
                            {
                                await bot.SendMessageAsync(chatId: teacher.Teacher.User.ChatId,
                                    text: message.Text);
                            }
                            await Menu.TeacherMenu(bot, message);
                            return;
                        case UserStatus.EditPracticalClasss:
                            var editPracticalClassRes = await practicClassDataHandler.EditPracticalClassName(user, message.Text);
                            await Menu.TeacherMenu(bot, message, editPracticalClassRes);
                            return;
                        case UserStatus.Miscellaneous:
                            await miscellaneousDataHandler.CreateMiscellaneous(user, message.Text);
                            if (!string.IsNullOrEmpty(await classRoomDataHandler.GetMiscellaneousChannel(user)))
                                await bot.SendMessageAsync(chatId: await classRoomDataHandler.GetMiscellaneousChannel(user),
                                    text: "Miscelánea enviada por: @" + user.Username + "\n\"" + message.Text + "\"");
                            await Menu.StudentMenu(bot, message);
                            return;
                        case UserStatus.CreatePracticClass:
                            var canCreateClass = await practicClassDataHandler.CreatePracticClass(user, message.Text);
                            if(canCreateClass)
                            {
                                await Menu.TeacherMenu(bot, message);
                            }
                            else
                            {
                                await Menu.CancelMenu(bot, message, "Clase Práctica con Formato Incorrecto");
                            }
                            return;
                        case UserStatus.CreateGuild:
                            await guildDataHandler.CreateGuild(user, message.Text);
                            await Menu.GuildMenu(bot, message);
                            return;
                        case UserStatus.AssignCreditsAtGuild:
                            var data = await guildDataHandler.AssignCreditsAtGuild(user, message.Text);
                            foreach (var student in data.Item1)
                            {
                                await bot.SendMessageAsync(chatId: student.User.ChatId,
                                    text: $"Ha recibido {data.Item2} créditos y su profesor le dijo \"{data.Item3}\".");
                            }
                            await Menu.GuildMenu(bot, message);
                            return;
                    }
                    await Logger.Error($"Error: El usuario {user.Username} está escribiendo cosas sin sentido");
                    await bot.SendMessageAsync(chatId: message.Chat.Id,
                                    text: "Por favor, atienda lo que hace, no me haga perder tiempo.");
                }
            }
            else if (message.Document != null)
            {
                if (user.Status == UserStatus.Meme)
                {
                    await memeDataHandler.SendMeme(user.Id, message.Document);
                    await Menu.StudentMenu(bot, message);
                }
                else
                {
                    await Logger.Error($"Error: El usuario {user.Username} está escribiendo cosas sin sentido");
                    await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Por favor, atienda lo que hace, no me haga perder tiempo.");
                }
            }
            else if(message.Photo != null)
            {
                if (user.Status == UserStatus.Meme )
                {
                    await memeDataHandler.SendMeme(user.Id, message.Photo[0]);
                    await Menu.StudentMenu(bot, message);
                }
                else
                {
                    await Logger.Error($"Error: El usuario {user.Username} está escribiendo cosas sin sentido");
                    await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Por favor, atienda lo que hace, no me haga perder tiempo.");
                }
            }
            else
            {
                await Logger.Error($"Error: El usuario {user.Username} está escribiendo cosas sin sentido");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                            text: "Por favor, atienda lo que hace, no me haga perder tiempo.");
            }
        }

        private async Task OnCommand(string cmd, string[] args, ClassAssistantBot.Models.User user)
        {
            switch (cmd)
            {
                case "start":
                    await StartCommand(user);
                    break;
                case "estudiante":
                    await StudentCommand(user);
                    break;
                case "profesor":
                    await TeacherCommand(user);
                    break;
                case "crear":
                    await CreateCommand(user);
                    break;
                case "entrar":
                    await EnterCommand(user);
                    break;
                case "estudiantes":
                    await StudentsCommand(user);
                    break;
                case "créditos":
                    await CreditsCommand(user);
                    break;
                case "revisarclasepráctica":
                    await ReviewPracticClassCommand(user);
                    break;
                case "crearclasepráctica":
                    await CreatePracticClassCommand(user);
                    break;
                case "pendientesdirectos":
                    await DirectPendingCommand(user);
                    break;
                case "misceláneas":
                    await CreateMiscellaneousCommand(user);
                    break;
                case "pendientes":
                    await PendingsCommand(user);
                    break;
                case "todaslasaulasconpendientes":
                    await AllClassRoomWithPendingsCommand(user);
                    break;
                case "llavedelestudiante":
                    await StudentAccessKeyCommand(user);
                    break;
                case "llavedelprofesor":
                    await TeacherAccessKeyCommand(user);
                    break;
                case "cambiardeaula":
                    await ChangeClassRoomCommand(user);
                    break;
                case "rectificaralprofesor":
                    await RectificationToTheTeacherCommand(user);
                    break;
                case "asignarcréditos":
                    await AssignCreditsCommand(user);
                    break;
                case "eliminarestudiantedelaula":
                    await RemoveStudentFromClassRoomCommand(user);
                    break;
                case "cancelar":
                    await CancelCommand(user);
                    break;
                case "intervenciónenclase":
                    await ClassInterventionCommand(user);
                    break;
                case "meme":
                    await MemeCommand(user);
                    break;
                case "chiste":
                    await JokeCommand(user);
                    break;
                case "diario":
                    await DiaryCommand(user);
                    break;
                case "frasedeestado":
                    await StatusPhraseCommand(user);
                    break;
                case "cambiartítulodeclase":
                    await ClassTitleCommand(user);
                    break;
                case "configuración":
                    await ConfigurationCommand(user);
                    break;
                case "asignarcanaldemisceláneas":
                    await AssignMiscellaneousChannelCommand(user);
                    break;
                case "iniciarclase":
                    await StartClassCommand(user);
                    break;
                case "registrar":
                    await Menu.RegisterMenu(bot, message);
                    break;
                case "gremios":
                    await Menu.GuildMenu(bot, message);
                    break;
                case "veraulaactual":
                    await SeeClassRoomActiveCommand(user);
                    break;
                case "verclasesinscritas":
                    await SeeListClass(user);
                    break;
                case "asignarcanaldememes":
                    await AssignMemeChannelCommand(user);
                    break;
                case "asignarcanaldechistes":
                    await AssignJokesChannelCommand(user);
                    break;
                case "asignarcanaldeintervecionesenclases":
                    await AssignClassInterventionChannelCommand(user);
                    break;
                case "asignarcanaldeactulizacióndediario":
                    await AssignDiaryChannelCommand(user);
                    break;
                case "asignarcanaldefrasesdeestado":
                    await AssignStatusPhraseChannelCommand(user);
                    break;
                case "asignarcanaldetítulosdeclases":
                    await AssignClassTitleChannelCommand(user);
                    break;
                case "asignarcanalderectificacionesdeprofesores":
                    await AssignRectificationToTheTeacherChannelCommand(user);
                    break;
                case "enviarinformaciónatodoslosestudiantesdelaula":
                    await SendStudentsInformationCommand(user);
                    break;
                case "editarnombredeclasepráctica":
                    await EditPracticalClassNameCommand(user);
                    break;
                case "verestadodecréditos":
                    await CreditsStatusCommand(user);
                    break;
                case "creargremio":
                    await CreateGuildCommand(user);
                    break;
                case "asignarestudiantealgremio":
                    await AssignStudentsAtGuildCommand(user);
                    break;
                case "asignarcréditosalgremio":
                    await AssignCretidAtGuildCommand(user);
                    break;
                case "eliminarestudiantedelgremio":
                    await DeleteStudentFromGuildCommand(user);
                    break;
                case "eliminargremio":
                    await DeleteGuildCommand(user);
                    break;
                case "detallesdelgremio":
                    await DetailsGuildCommand(user);
                    break;
                case "eliminarclasepráctica":
                    await PracticalClassDeleteCommand(user);
                    break;
                default:
                    await DefaultCommand(user, cmd);
                    break;
            }
        }

        #region Comandos
        private async Task StartCommand(Models.User? user)
        {
            if (user == null)
            {
                await userDataHandler.CreateUser(appUser, message.Chat.Id);
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Inserte su nombre y dos apellidos, por favor.",
                                replyMarkup: new ReplyKeyboardRemove());
                return;
            }
            else
            {
                if(user.Status == UserStatus.Ready)
                {
                    await Logger.Error($"Error: El usuario {user.Username} está intentando registrarse nuevamente en el bot.");
                    if (user.IsTecaher)
                    {
                        await Menu.TeacherMenu(bot, message);
                    }
                    else
                    {
                        await Menu.StudentMenu(bot, message);
                    }
                }
            }
        }

        private async Task StudentCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Verified && user.Status != UserStatus.ChangeClassRoom)
            {
                await Logger.Error($"Error: El estudiante {user.Username} está intentando unirse a un aula sin estar verificado.");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                            text: "Por favor, atienda lo que hace, no me haga perder tiempo, inserte su nombre y 2 apellidos.");
                return;
            }
            await studentDataHandler.StudentEnterClass(user);
            await bot.SendMessageAsync(chatId: message.Chat.Id,
                            text: "Inserte el código que le dio su profesor.",
                            replyMarkup: new ReplyKeyboardRemove());
        }

        private async Task TeacherCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Verified && user.Status != UserStatus.ChangeClassRoom)
            {
                await Logger.Error($"Error: El usuario {user.Username} está intentando convertirse en profesor sin estar verificado.");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                            text: "Por favor, atienda lo que hace, no me haga perder tiempo, inserte su nombre y 2 apellidos.");
                return;
            }
            if(user.Status == UserStatus.Verified)
                await teacherDataHandler.CreateTeacher(user);
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
            await bot.SendMessageAsync(chatId: message.Chat.Id,
                            text: "Va a crear un aula nueva o va a entrar a una existente?",
                            replyMarkup: keyboard);
        }

        private async Task CreateCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.CreatingTecaher && user.Status != UserStatus.ChangeClassRoom)
            {
                await Logger.Error($"Error: El profesor {user.Username} está intentando crear un aula sin haber creado el profesor.");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                            text: "Por favor, atienda lo que hace, no me haga perder tiempo.");
                return;
            }
            await classRoomDataHandler.CreateClassRoom(user);
            await bot.SendMessageAsync(chatId: message.Chat.Id,
                            text: "Inserte el nombre de la clase.",
                            replyMarkup: new ReplyKeyboardRemove());
        }

        private async Task EnterCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.CreatingTecaher && user.Status != UserStatus.ChangeClassRoom)
            {
                await Logger.Error($"Error: El profesor {user.Username} está intentando entrar a un aula sin creado el profesor.");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                            text: "Por favor, atienda lo que hace, no me haga perder tiempo.");
                return;
            }
            await teacherDataHandler.TeacherEnterClass(user);
            await bot.SendMessageAsync(chatId: message.Chat.Id,
                            text: "Inserte el código que le dio su profesor.",
                            replyMarkup: new ReplyKeyboardRemove());
        }

        private async Task StudentsCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con los comandos avanzados");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else if (!user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} está intentando solicitar un listado de estudiantes pero no es profesor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var res = await studentDataHandler.GetStudentsOnClassByTeacherId(user.Id);
                await Menu.TeacherMenu(bot, message, res);
            }
        }

        private async Task CreditsCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else if (!user.IsTecaher)
            {
                var text = await creditsDataHandler.GetCreditsById(user.Id);
                await Menu.StudentMenu(bot, message, text);
                return;
            }
        }

        private async Task PendingsCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var pendings = await pendingDataHandler.GetPendings(user);
                await Menu.PendingsFilters(bot, message, pendings.Item1, pendings.Item2);
            }
        }

        private async Task AllClassRoomWithPendingsCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var res = await pendingDataHandler.GetAllClassRoomWithPendings(user);
                if (string.IsNullOrEmpty(res))
                    res = "No tienen pendientes en ninguna de sus aulas";
                await Menu.TeacherConfigurationMenu(bot, message, res);
            }
        }

        private async Task StudentAccessKeyCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var accessKey = teacherDataHandler.GetStudentAccessKey(user.Id);
                await Menu.TeacherConfigurationMenu(bot, message, $"La clave de acceso para sus estudianes es {accessKey}.");
            }
        }

        private async Task ConfigurationCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para solicitar la configuración de su cuenta");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else if (!user.IsTecaher)
            {
                await Menu.StudentConfigurationMenu(bot, message,"Menú de configuración");
                return;
            }
            else
            {
                await Menu.TeacherConfigurationMenu(bot, message, "Menú de configuración");
            }
        }

        private async Task ClassTitleCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var classes = await classTitleDataHandler.GetClasses(user);
                await classTitleDataHandler.ChangeClassTitle(user);
                await Menu.ClassesList(bot, message, classes);
            }
        }

        private async Task DefaultCommand(Models.User? user, string command)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (!user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} está interactuando con un comando que no existe");
                await Menu.StudentMenu(bot, message, "El comando insertado no existe, por favor, no me haga perder tiempo.");
                return;
            }
            else
            {
                string file = "";
                bool giveMeExplication = false;
                var pending = await pendingDataHandler.GetPendingByCode(command, file, giveMeExplication);
                var type = await pendingDataHandler.GetPending(command);

                if (!string.IsNullOrEmpty(pending.Item1) && type.Type == InteractionType.Diary)
                {
                    await Menu.PendingDiaryCommands(bot, message, pending.Item1, pending.Item3, command, user);
                    return;
                }

                var teachers = await teacherDataHandler.GetTeachers(user);

                if(teachers.Count() != 0)
                {
                    if (await Menu.PendingCommands(bot, message, pending.Item1, teachers, pending.Item3, command, user, pending.Item2))
                        return;
                }

                var student = await creditsDataHandler.GetCreditsByUserName(user.Id, command, true, true);
                if (!string.IsNullOrEmpty(student))
                {
                    await Menu.TeacherMenu(bot, message, student);
                    return;
                }
                await Logger.Error($"Error: El usuario {user.Username} está interactuando con un comando que no existe");
                await Menu.TeacherMenu(bot, message, "El comando insertado no existe, por favor, no me haga perder tiempo.");
                return;
            }
        }

        private async Task StatusPhraseCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var status = await statusPhraseDataHandler.ChangeStatusPhrase(user);
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                            text: $"Su frase de estado actual es: {status}",
                            replyMarkup: new ReplyKeyboardRemove());
                await Menu.CancelMenu(bot, message, "Inserte la nueva frase de estado:");
            }
        }

        private async Task DiaryCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                await diaryDataHandler.UpdateDiary(user);
                await Menu.CancelMenu(bot, message, "Inserte la actualización de su diario:");
            }
        }

        private async Task RectificationToTheTeacherCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                await rectificationToTheTeacherDataHandler.DoRectificationToTheTaecher(user);
                var teachers = await teacherDataHandler.GetTeachers(user);
                await Menu.TeachersList(bot, message, teachers, "Seleccione el profesor al que desea rectificar:");
            }
        }

        private async Task JokeCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                await jokeDataHandler.DoJoke(user);
                await Menu.CancelMenu(bot, message, "Haga el chiste:");
            }
        }

        private async Task MemeCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                await memeDataHandler.SendMeme(user);
                await Menu.CancelMenu(bot, message, "Inserte un meme:");
            }
        }

        private async Task ClassInterventionCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                await classInterventionDataHandler.CreateIntervention(user);
                var classes = await classTitleDataHandler.GetClasses(user);
                await Menu.ClassesList(bot, message, classes);
            }
        }

        private async Task RemoveStudentFromClassRoomCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var res = await studentDataHandler.RemoveStudentFromClassRoom(user.Id);
                await Menu.CancelMenu(bot, message, res);
            }
        }

        private async Task CancelCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            await userDataHandler.CancelAction(user);
            if (user.IsTecaher)
                await Menu.TeacherMenu(bot, message);
            else
                await Menu.StudentMenu(bot, message);
        }

        private async Task ChangeClassRoomCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando changeclass");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var res = await classRoomDataHandler.ChangeClassRoom(user.Id);
                user.Status = UserStatus.ChangeClassRoom;
                await userDataHandler.VerifyUser(user);
                await Menu.ChangeClassRoomMenu(bot, message, res);
            }
        }

        private async Task TeacherAccessKeyCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var accessKey = await teacherDataHandler.GetTeacherAccessKey(user.Id);
                await Menu.TeacherConfigurationMenu(bot, message, $"La clave de acceso para sus profesores es {accessKey}.");
            }
        }

        private async Task StartClassCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready && !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                await classTitleDataHandler.CreateClass(user);
                await Menu.CancelMenu(bot, message, "Inserte el título de la clase:");
            }
        }

        private async Task AssignCreditsCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else if (user.IsTecaher)
            {
                await creditsDataHandler.AssignCredit(user);
                await Menu.CancelMenu(bot, message, "Inserte el nombre de usuario del estudiante:");
                return;
            }
            else
            {
                await Logger.Error($"Error: El usuario {user.Username} está intentando intentando registrar su nombre y apellidos en formato incorrecto.");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Por favor, no me haga perder el tiempo, inserte su nombre y sus 2 apellidos.");
                return;
            }
        }

        private async Task SeeClassRoomActiveCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var res = classRoomDataHandler.SeeClassRoomActive(user);
                if (user.IsTecaher)
                    await Menu.TeacherMenu(bot, message, "Se encuentra en el aula: " + res);
                else
                    await Menu.StudentMenu(bot, message, "Se encuentra en el aula: " + res);
                return;
            }
        }

        private async Task SeeListClass(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var res =await classRoomDataHandler.GetClassesCreated(user);
                var classRoom = await classRoomDataHandler.SeeClassRoomActive(user);
                if (user.IsTecaher)
                    await Menu.TeacherMenu(bot, message, $"El aula {classRoom} tiene creadas las clases:\n{res}");
                else
                    await Menu.StudentMenu(bot, message, $"El aula {classRoom} tiene creadas las clases:\n{res}");
                return;
            }
        }

        private async Task DirectPendingCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var pendings = await pendingDataHandler.GetPendings(user, true);
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
                await Menu.CancelMenu(bot, message, "Menú:");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: pendings.Item1,
                                replyMarkup: keyboard);
            }
        }

        private async Task AssignMemeChannelCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                await classRoomDataHandler.AssignMemeChannel(user);
                await Menu.CancelMenu(bot, message, "Inserte el Username del canal en el que se publicarán los memes(Tenga presente que el bot debe ser miembro del canal y con privilegios de Administrador):");
            }
        }

        private async Task AssignJokesChannelCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                await classRoomDataHandler.AssignJokesChannel(user);
                await Menu.CancelMenu(bot, message, "Inserte el Username del canal en el que se publicarán los chistes(Tenga presente que el bot debe ser miembro del canal y con privilegios de Administrador):");
            }
        }

        private async Task AssignClassInterventionChannelCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                await classRoomDataHandler.AssignClassInterventionChannel(user);
                await Menu.CancelMenu(bot, message, "Inserte el Username del canal en el que se publicarán las intervenciones en clase(Tenga presente que el bot debe ser miembro del canal y con privilegios de Administrador):");
            }
        }

        private async Task AssignDiaryChannelCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                await classRoomDataHandler.AssignDiaryChannel(user);
                await Menu.CancelMenu(bot, message, "Inserte el Username del canal en el que se publicarán las actualizaciones a los diarios(Tenga presente que el bot debe ser miembro del canal y con privilegios de Administrador):");
            }
        }

        private async Task AssignStatusPhraseChannelCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                await classRoomDataHandler.AssignStatusPhraseChannel(user);
                await Menu.CancelMenu(bot, message, "Inserte el Username del canal en el que se publicarán las frases de estado(Tenga presente que el bot debe ser miembro del canal y con privilegios de Administrador):");
            }
        }

        private async Task AssignClassTitleChannelCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                await classRoomDataHandler.AssignClassTitleChannel(user);
                await Menu.CancelMenu(bot, message, "Inserte el Username del canal en el que se publicarán las propuestas de títulos de clases(Tenga presente que el bot debe ser miembro del canal y con privilegios de Administrador):");
            }
        }

        private async Task AssignRectificationToTheTeacherChannelCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                await classRoomDataHandler.AssignRectificationToTheTeacherChannel(user);
                await Menu.CancelMenu(bot, message, "Inserte el Username del canal en el que se publicarán las rectificaciones a los profesores(Tenga presente que el bot debe ser miembro del canal y con privilegios de Administrador):");
            }
        }

        private async Task SendStudentsInformationCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                await classRoomDataHandler.SendInformationToTheStudents(user);
                await Menu.CancelMenu(bot, message, "Inserte la información que le quiere enviar a sus estudiantes:");
            }
        }

        private async Task CreateMiscellaneousCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando credits");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                await miscellaneousDataHandler.CreateMiscellaneous(user);
                await Menu.CancelMenu(bot, message, "Inserte la miscelánea que quiere proponer:");
            }
        }

        private async Task AssignMiscellaneousChannelCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Asignar Canal de Misceláneas");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                await classRoomDataHandler.AssignMiscellaneousChannel(user);
                await Menu.CancelMenu(bot, message, "Inserte el Username del canal en el que se publicarán las misceláneas(Tenga presente que el bot debe ser miembro del canal y con privilegios de Administrador):");
            }
        }

        private async Task CreatePracticClassCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Crear Clase Práctica");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                await practicClassDataHandler.CreatePracticClass(user);
                await Menu.CancelMenu(bot, message, "Inserte la Clase Práctica en el formato siguiente: \n CP1 1 10000 1a 20000 2a 400000 ......");
            }
        }

        private async Task ReviewPracticClassCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Crear Clase Práctica");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var practicalClasses = await practicClassDataHandler.GetPracticClasses(user);
                await Menu.CancelMenu(bot, message);
                await Menu.PracticalClassList(bot, message, practicalClasses, "Seleccione una Clase Práctica:");
            }
        }

        private async Task EditPracticalClassNameCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Crear Clase Práctica");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var practicalClasses = await practicClassDataHandler.EditPracticalClassName(user);
                await Menu.CancelMenu(bot, message, practicalClasses);
            }
        }

        private async Task CreditsStatusCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Estado de Creditos");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var credits = await creditsDataHandler.GetCreditListOfUser(user);
                await Menu.StudentMenu(bot, message, credits);
            }
        }

        private async Task CreateGuildCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Crear Gremio");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                await guildDataHandler.CreateGuild(user);
                await Menu.CancelMenu(bot, message, "Inserte el nombre del Gremio:");
            }
        }

        private async Task AssignStudentsAtGuildCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Asignar Estudiante al Gremio");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var guilds = await guildDataHandler.AssignStudentAtGuild(user);
                await Menu.CancelMenu(bot, message);
                await Menu.GuildList(bot, message, guilds, "Selecciona el Gremio a dónde va a añadir el estudiante");
            }
        }

        private async Task AssignCretidAtGuildCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Asignar Créditos al Gremio");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var guilds = await guildDataHandler.AssignCreditsAtGuild(user);
                await Menu.CancelMenu(bot, message);
                await Menu.GuildList(bot, message, guilds, "Selecciona el Gremio que desea asignar créditos");
            }
        }

        private async Task DeleteStudentFromGuildCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Eliminar Estudiante del Gremio");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var guilds = await guildDataHandler.DeleteStudentFromGuild(user);
                await Menu.CancelMenu(bot, message);
                await Menu.GuildList(bot, message, guilds, "Selecciona el Gremio donde está el estudiante que desea elminar");
            }
        }

        private async Task DeleteGuildCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Eliminar Gremio");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var guilds = await guildDataHandler.DeleteGuild(user);
                await Menu.CancelMenu(bot, message);
                await Menu.GuildList(bot, message, guilds, "Selecciona el Gremio que desea elminar");
            }
        }

        private async Task DetailsGuildCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Detalles del Gremio");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var guilds = await guildDataHandler.DetailsGuild(user);
                await Menu.CancelMenu(bot, message);
                await Menu.GuildList(bot, message, guilds, "Seleccione un Gremio para consultar sus detalles");
            }
        }

        private async Task PracticalClassDeleteCommand(Models.User? user)
        {
            if (user == null)
            {
                await Logger.Error($"Error: Usuario nulo, problemas en el servidor");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "Lo siento, estoy teniendo problemas mentales y estoy en una consulta del psiquiátra.");
                return;
            }
            if (user.Status != UserStatus.Ready || !user.IsTecaher)
            {
                await Logger.Error($"Error: El usuario {user.Username} no está listo para comenzar a interactuar con el comando Estado de Creditos");
                await bot.SendMessageAsync(chatId: message.Chat.Id,
                                text: "No tiene acceso al comando, por favor no lo repita.");
                return;
            }
            else
            {
                var practicalClasses = await practicClassDataHandler.GetPracticClasses(user);
                await Menu.CancelMenu(bot, message);
                await Menu.PracticalClassList(bot, message, practicalClasses, "Seleccione una Clase Práctica:");
            }
        }
        #endregion

        #region Procesos que completan comandos de varias operaciones
        private async Task OnRegister(Models.User user, string text)
        {
            var list = text.Split(' ');

            user.Name = text;
            await userDataHandler.VerifyUser(user);
            await Logger.Success($"Verifying the user {user.Username}");

            await bot.SendMessageAsync(chatId: message.Chat.Id,
                            text: "Bienvenido " + text,
                            replyMarkup: new ReplyKeyboardRemove());

            await Menu.RegisterMenu(bot, message);
        }

        private async Task<bool> OnAssignStudentAtClass(long id, string text)
        {
            var success = false;
            var res = await studentDataHandler.AssignStudentAtClass(id, text, success);
            await bot.SendMessageAsync(chatId: message.Chat.Id,
                            text: res.Item1,
                            replyMarkup: new ReplyKeyboardRemove());
            return res.Item2;
        }

        private async Task<bool> OnAssignTeacherAtClass(long id, string text)
        {
            bool success = false;
            var res = await teacherDataHandler.AssignTeacherAtClass(id, text, success);
            await bot.SendMessageAsync(chatId: message.Chat.Id,
                            text: res.Item1,
                            replyMarkup: new ReplyKeyboardRemove());
            return res.Item2;
        }

        private async Task OnCreateClassRoom(long id, string name)
        {
            var res = await classRoomDataHandler.CreateClassRoom(id, name);
            await bot.SendMessageAsync(chatId: message.Chat.Id,
                            text: res,
                            replyMarkup: new ReplyKeyboardRemove());
        }

        private async Task OnRectificationToTheTeacherAtTeacherUserName(Models.User user, string teacherUserName)
        {
            await rectificationToTheTeacherDataHandler.DoRectificationToTheTaecherUserName(user, teacherUserName);
            await Menu.CancelMenu(bot, message, "Explique a grosso modo en qué se equivocó el profesor y su correción:");
        }

        private async Task OnRectificationToTheTeacherAtText(Models.User user, string text)
        {
            var res = await rectificationToTheTeacherDataHandler.DoRectificationToTheTaecherText(user, text);
            await bot.SendMessageAsync(chatId: message.Chat.Id,
                            text: res,
                            replyMarkup: new ReplyKeyboardRemove());
        }

        private async Task OnStartClass(Models.User user, string classTitle)
        {
            await classTitleDataHandler.CreateClass(user, classTitle);
            await bot.SendMessageAsync(chatId: message.Chat.Id,
                            text: "Clase creada satisfactoriamente.",
                            replyMarkup: new ReplyKeyboardRemove());
        }

        private async Task OnChangeClassTitle(Models.User user, long classId)
        {
            await classTitleDataHandler.ChangeClassTitle(user, classId);
            await Menu.CancelMenu(bot, message, "Inserte el nombre la clase:");
        }

        private async Task OnChangeClassTitle(Models.User user, string classTitle)
        {
            await classTitleDataHandler.ChangeClassTitle(user, classTitle);
            await bot.SendMessageAsync(chatId: message.Chat.Id,
                            text: "Título propuesto satisfactoriamente.",
                            replyMarkup: new ReplyKeyboardRemove());
        }

        private async Task OnClassIntervention(Models.User user, long classId)
        {
            await classInterventionDataHandler.CreateIntervention(user, classId);
            await Menu.CancelMenu(bot, message, "Inserte su intervención:");
        }

        private async Task OnClassIntervention(Models.User user, string classIntervention)
        {
            await classInterventionDataHandler.CreateIntervention(user, classIntervention);
            await bot.SendMessageAsync(chatId: message.Chat.Id,
                            text: "Intervención hecha satisfactoriamente.",
                            replyMarkup: new ReplyKeyboardRemove());
        }

        private async Task OnPendings(Models.User user, Message message)
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
                var pending = await pendingDataHandler.GetPending(pendingCode);
                var comment = "";

                if (pending.Type == InteractionType.ClassIntervention)
                {
                    var data = await classInterventionDataHandler.GetClassIntenvention(pending.ObjectId);
                    comment = data.Text;
                }
                else if (pending.Type == InteractionType.ClassTitle)
                {
                    var data = await classTitleDataHandler.GetClassTitle(pending.ObjectId);
                    comment = data.Title;
                }
                else if (pending.Type == InteractionType.Diary)
                {
                    var data = await diaryDataHandler.GetDiary(pending.ObjectId);
                    comment = data.Text;
                }
                else if (pending.Type == InteractionType.Joke)
                {
                    var data = await jokeDataHandler.GetJoke(pending.ObjectId);
                    comment = data.Text;
                }
                else if (pending.Type == InteractionType.RectificationToTheTeacher)
                {
                    var data = await rectificationToTheTeacherDataHandler.GetRectificationToTheTeacher(pending.ObjectId);
                    comment = data.Text;
                }
                else if (pending.Type == InteractionType.StatusPhrase)
                {
                    var data = await statusPhraseDataHandler.GetStatusPhrase(pending.ObjectId);
                    comment = data.Phrase;
                }
                else if(pending.Type == InteractionType.Miscellaneous)
                {
                    var data = await miscellaneousDataHandler.GetMiscellaneous(pending.ObjectId);
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
                await bot.SendMessageAsync(chatId: pending.Student.User.ChatId,
                                text: text);
                string credit_information = "";
                if (response.Length != 1)
                {
                    for (int i = 1; i < response.Length; i++)
                    {
                        credit_information += response[i] + " ";
                    }
                }
                await creditsDataHandler.AddCredits(credits, user.Id, pending.ObjectId, pending.Student.User.Id, pending.ClassRoomId, credit_information);
                await pendingDataHandler.RemovePending(pending);
                await PendingsCommand(user);

                if (pending.Type == InteractionType.Meme)
                {
                    var meme = await memeDataHandler.GetMeme(pending.ObjectId);
                    if (!string.IsNullOrEmpty(await classRoomDataHandler.GetMemeChannel(user)))
                        bot.SendPhoto(chatId: await classRoomDataHandler.GetMemeChannel(user),
                            photo: meme.FileId,
                            caption: "Meme enviado por: @" + pending.Student.User.Username);
                }

                /*if (pending.Type == InteractionType.ClassIntervention)
                {
                    comment = classInterventionDataHandler.GetClassIntenvention(pending.ObjectId).Text;
                    if (!string.IsNullOrEmpty(classRoomDataHandler.GetClassInterventionChannel(user)))
                        await bot.SendMessageAsync(chatId: classRoomDataHandler.GetClassInterventionChannel(user),
                            text: "Intervención en Clase enviada por: @" + pending.Student.User.Username + "\n\"" + comment + "\"");
                }
                else if (pending.Type == InteractionType.ClassTitle)
                {
                    comment = classTitleDataHandler.GetClassTitle(pending.ObjectId).Title;
                    if (!string.IsNullOrEmpty(classRoomDataHandler.GetClassTitleChannel(user)))
                        await bot.SendMessageAsync(chatId: classRoomDataHandler.GetClassTitleChannel(user),
                            text: "Cambio de Título de Clase enviado por: @" + pending.Student.User.Username + "\n\"" + comment + "\"");
                }
                else if (pending.Type == InteractionType.Diary)
                {
                    comment = diaryDataHandler.GetDiary(pending.ObjectId).Text;
                    if (!string.IsNullOrEmpty(classRoomDataHandler.GetDiaryChannel(user)))
                        await bot.SendMessageAsync(chatId: classRoomDataHandler.GetDiaryChannel(user),
                            text: "Actualización al Diario enviado por: @" + pending.Student.User.Username + "\n\"" + comment + "\"");
                }
                else if (pending.Type == InteractionType.Joke)
                {
                    comment = jokeDataHandler.GetJoke(pending.ObjectId).Text;
                    if (!string.IsNullOrEmpty(classRoomDataHandler.GetJokesChannel(user)))
                        await bot.SendMessageAsync(chatId: classRoomDataHandler.GetJokesChannel(user),
                            text: "Chiste enviado por: @" + pending.Student.User.Username + "\n\"" + comment + "\"");
                }
                else if (pending.Type == InteractionType.RectificationToTheTeacher)
                {
                    comment = rectificationToTheTeacherDataHandler.GetRectificationToTheTeacher(pending.ObjectId).Text;
                    if (!string.IsNullOrEmpty(classRoomDataHandler.GetRectificationToTheTeacherChannel(user)))
                        await bot.SendMessageAsync(chatId: classRoomDataHandler.GetRectificationToTheTeacherChannel(user),
                            text: "Rectificación al Profesor enviada por: @" + pending.Student.User.Username + "\n\"" + comment + "\"");
                }
                else if (pending.Type == InteractionType.StatusPhrase)
                {
                    comment = statusPhraseDataHandler.GetStatusPhrase(pending.ObjectId).Phrase;
                    if (!string.IsNullOrEmpty(classRoomDataHandler.GetStatusPhraseChannel(user)))
                        await bot.SendMessageAsync(chatId: classRoomDataHandler.GetStatusPhraseChannel(user),
                            text: "Frase de Estado enviada por: @" + pending.Student.User.Username + "\n\"" + comment + "\"");
                }
                else if(pending.Type == InteractionType.Miscellaneous)
                {
                    comment = miscellaneousDataHandler.GetMiscellaneous(pending.ObjectId).Text;
                    if (!string.IsNullOrEmpty(classRoomDataHandler.GetMiscellaneousChannel(user)))
                        await bot.SendMessageAsync(chatId: classRoomDataHandler.GetMiscellaneousChannel(user),
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
                var pending = await pendingDataHandler.GetPending(pendingCode);
                foreach (var username in response)
                {
                    if (await teacherDataHandler.ExistTeacher(username))
                    {
                        var teacherChatId = await pendingDataHandler.AddDirectPending(username, pending.Id);
                        await bot.SendMessageAsync(chatId: teacherChatId,
                            text: "Le han asignado un pendiente que tiene que revisar.");
                    }
                    else
                    {
                        await bot.SendMessageAsync(chatId: user.ChatId,
                            text: $"No existe un usuario con el user name {username}.");
                    }
                }
                await PendingsCommand(user);
            }
        }

        private async Task OnAssignCredits(Models.User user, string username)
        {
            if(!await creditsDataHandler.AssignCredit(user, username))
            {
                await Menu.TeacherMenu(bot, message);
            }
            else
                await Menu.CancelMenu(bot, message, "Inserte los créditos y su mensaje:");
        }

        private async Task OnAssignCreditsMessage(Models.User user, string text)
        {
            var res = await creditsDataHandler.AssignCreditMessage(user, text);
            await bot.SendMessageAsync(chatId: res.Item2,
                            text: $"Ha recibido {res.Item3} créditos y su profesor le dijo \"{res.Item1}\".");
            await Menu.TeacherMenu(bot, message, "Créditos asignados satisfactoriamente.");
        }
        #endregion
    }
}