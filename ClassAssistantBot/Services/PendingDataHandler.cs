using System.Text;
using ClassAssistantBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.BotAPI.AvailableTypes;
using User = ClassAssistantBot.Models.User;

namespace ClassAssistantBot.Services
{
    public class PendingDataHandler
    {
        private DataAccess dataAccess { get; set; }

        public PendingDataHandler(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public string GetPendings(User user)
        {
            var pendings = dataAccess.Pendings
                .Where(x => x.ClassRoomId == user.ClassRoomActiveId)
                .Include(x => x.Student)
                .ThenInclude(x => x.User)
                .ToList();
            var classRoom = dataAccess.ClassRooms
                .Where(x => x.Id == user.ClassRoomActiveId)
                .First();

            user.Status = UserStatus.Pending;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();

            var res = new StringBuilder($"Pendientes de la clase {classRoom.Name}:\n");

            foreach (var item in pendings)
            {
                res.Append(item.Type.ToString());
                res.Append(": ");
                res.Append(item.Student.User.Username);
                res.Append(" -> /");
                res.Append(item.Code);
                res.Append("\n");
            }
            if (pendings.Count == 0)
                res.Append("No tiene revisiones pendientes en esta aula");
            return res.ToString();
        }

        public string GetPendingByCode(string code, out string pend)
        {
            var res = new StringBuilder();
            var pending = dataAccess.Pendings
                .Where(x => x.Code == code)
                .FirstOrDefault();
            pend = "";
            if (pending == null)
                return pend;
            if (pending.Type == InteractionType.ClassIntervention)
            {
                var classIntervention = dataAccess.ClassInterventions
                    .Include(x => x.User)
                    .Include(x => x.Class)
                    .First(x => x.Id == pending.ObjectId);
                res.Append($"ClassIntervention de {classIntervention.User.Username}\n");
                res.Append($"Clase: {classIntervention.Class.Title}\n");
                res.Append($"Intevención: {classIntervention.Text}\n");
                res.Append($"Código de Pendiente: /{pending.Code}\n");
            }
            else if (pending.Type == InteractionType.ClassTitle)
            {
                var classTitle = dataAccess.ClassTitles
                    .Include(x => x.Class)
                    .Include(x => x.User)
                    .First(x => x.Id == pending.ObjectId);
                res.Append($"ClassTitle de {classTitle.User.Username}\n");
                res.Append($"Clase: {classTitle.Class.Title}\n");
                res.Append($"Título: {classTitle.Title}\n");
                res.Append($"Código de Pendiente: /{pending.Code}\n");
            }
            else if (pending.Type == InteractionType.Daily)
            {
                var daily = dataAccess.Dailies
                    .Include(x => x.User)
                    .First(x => x.Id == pending.ObjectId);
                res.Append($"Daily de {daily.User.Username}\n");
                res.Append($"Actualización: {daily.Text}\n");
                res.Append($"Código de Pendiente: /{pending.Code}\n");
            }
            else if (pending.Type == InteractionType.Joke)
            {
                var joke = dataAccess.Jokes
                    .Include(x => x.User)
                    .First(x => x.Id == pending.ObjectId);
                res.Append($"Joke de {joke.User.Username}\n");
                res.Append($"Texto: {joke.Text}\n");
                res.Append($"Código de Pendiente: /{pending.Code}\n");
            }
            else if (pending.Type == InteractionType.Meme)
            {
                var meme = dataAccess.Memes
                    .Include(x => x.User)
                    .First(x => x.Id == pending.ObjectId);
                res.Append($"Meme de {meme.User.Username}\n");
                res.Append($"Código de Pendiente: /{pending.Code}\n");
                pend = meme.FileId;
            }
            else if (pending.Type == InteractionType.RectificationToTheTeacher)
            {
                var rectificationToTheTeacher = dataAccess.RectificationToTheTeachers
                    .Include(x => x.Teacher.User)
                    .Include(x => x.User)
                    .First(x => x.Id == pending.ObjectId);
                res.Append($"RectificationToTheTeachers de {rectificationToTheTeacher.User.Username}\n");
                res.Append($"Profesor: {rectificationToTheTeacher.Teacher.User.Username}\n");
                res.Append($"Texto: {rectificationToTheTeacher.Text}\n");
                res.Append($"Código de Pendiente: /{pending.Code}\n");
            }
            else
            {
                var statusphrase = dataAccess.StatusPhrases
                    .Include(x => x.User)
                    .First(x => x.Id == pending.ObjectId);
                res.Append($"StatusPhrases de {statusphrase.User.Username}\n");
                res.Append($"Frase: {statusphrase.Phrase}\n");
                res.Append($"Código de Pendiente: /{pending.Code}\n");
            }

            return res.ToString();
        }

        public Pending GetPending(string code)
        {
            return dataAccess.Pendings
                .Where(x => x.Code == code)
                .Include(x => x.Student)
                .Include(x => x.Student.User)
                .Include(x => x.ClassRoom)
                .First();
        }

        public void RemovePending(Pending pending)
        {
            dataAccess.Pendings.Remove(pending);
            dataAccess.SaveChanges();
        }
    }
}

