        using System.Text;
using ClassAssistantBot.Models;
using Microsoft.EntityFrameworkCore;
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

        public (string, int) GetPendings(User user, bool directPendings = false, InteractionType interactionType = InteractionType.None, int page = 1)
        {
            var pendings = new List<Pending>();
            if (!directPendings)
            {
                pendings = dataAccess.Pendings
                .Where(x => x.ClassRoomId == user.ClassRoomActiveId)
                .Include(x => x.Student)
                .ThenInclude(x => x.User)
                .ToList();
                var removePendings = new List<Pending>();
                foreach (var pending in pendings)
                {
                    var directPending = dataAccess.DirectPendings.Where(x => x.PendingId == pending.Id).FirstOrDefault();
                    if (directPending != null)
                    {
                        removePendings.Add(pending);
                    }
                }

                pendings.RemoveAll(x => removePendings.Contains(x));
            }
            else
            {
                var directPending = dataAccess.DirectPendings
                    .Where(x => x.UserId == user.Id && x.Pending.ClassRoomId == user.ClassRoomActiveId)
                    .Include(x => x.Pending)
                    .Include(x => x.Pending.Student)
                    .Include(x => x.Pending.Student.User)
                    .ToList();
                foreach (var item in directPending)
                {
                    pendings.Add(item.Pending);
                }
            }

            var classRoom = dataAccess.ClassRooms
                .Where(x => x.Id == user.ClassRoomActiveId)
                .First();
            
            if(interactionType != InteractionType.None && !directPendings)
            {
                 pendings.RemoveAll(x => x.Type != interactionType);
            }

            user.Status = UserStatus.Pending;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
            int count = pendings.Count/10;
            if (pendings.Count % 10 != 0)
                count++;
            pendings = pendings.Skip((page - 1)*10).Take(10).ToList();
            var res = new StringBuilder($"Pendientes de la clase {classRoom.Name}:\n");

            foreach (var item in pendings)
            {
                var type = "Rectificación al Profesor";
                if(item.Type == InteractionType.ClassIntervention)
                {
                    type = "Interveción en Clase";
                }
                else if (item.Type == InteractionType.ClassTitle)
                {
                    type = "Cambio de Título de Clase";
                }
                else if (item.Type == InteractionType.Diary)
                {
                    type = "Actualización al Diario";
                }
                else if (item.Type == InteractionType.Joke)
                {
                    type = "Chiste";
                }
                else if (item.Type == InteractionType.Meme)
                {
                    type = "Meme";
                }
                else if (item.Type == InteractionType.Miscellaneous)
                {
                    type = "Miscelánea";
                }
                else if (item.Type == InteractionType.StatusPhrase)
                {
                    type = "Frase de Estado";
                }
                else if (item.Type == InteractionType.Diary)
                {
                    type = "Actualización al Diario";
                }
                res.Append(type);
                res.Append(": ");
                if (!string.IsNullOrEmpty(item.Student.User.Name))
                    res.Append(item.Student.User.Name);
                else
                    res.Append(item.Student.User.FirstName + " " + item.Student.User.LastName);
                res.Append("(" + item.Student.User.Username);
                res.Append(") -> /");
                res.Append(item.Code);
                if (item.GiveMeAnExplication)
                {
                    res.Append(" (A espera de explicación)");
                }
                res.Append("\n");
            }
            if (pendings.Count == 0)
                res.Append("No tiene revisiones pendientes en esta aula");
            return (res.ToString(), count);
        }

        public string GetPendingByCode(string code, out string pend, out bool giveMeExplication)
        {
            var res = new StringBuilder();
            var pending = dataAccess.Pendings
                .Where(x => x.Code == code)
                .FirstOrDefault();
            pend = "";
            giveMeExplication = false;
            if (pending == null)
                return pend;
            if (pending.Type == InteractionType.ClassIntervention)
            {
                var classIntervention = dataAccess.ClassInterventions
                    .Include(x => x.User)
                    .Include(x => x.Class)
                    .First(x => x.Id == pending.ObjectId);
                res.Append($"Intervención en Clase de {classIntervention.User.Username}\n");
                res.Append($"Clase: {classIntervention.Class.Title}\n");
                res.Append($"Intevención: {classIntervention.Text}\n");
                res.Append($"Código de Pendiente: /{pending.Code}\n");
            }
            else if (pending.Type == InteractionType.Miscellaneous)
            {
                var miscellaneous = dataAccess.Miscellaneous
                    .Include(x => x.User)
                    .Include(x => x.ClassRoom)
                    .First(x => x.Id == pending.ObjectId);
                res.Append($"Miscelánea de {miscellaneous.User.Username}\n");
                res.Append($"Comentario: {miscellaneous.Text}\n");
                res.Append($"Código de Pendiente: /{pending.Code}\n");
            }
            else if (pending.Type == InteractionType.ClassTitle)
            {
                var classTitle = dataAccess.ClassTitles
                    .Include(x => x.Class)
                    .Include(x => x.User)
                    .First(x => x.Id == pending.ObjectId);
                res.Append($"Cambio de Título de la Clase {classTitle.User.Username}\n");
                res.Append($"Clase: {classTitle.Class.Title}\n");
                res.Append($"Título: {classTitle.Title}\n");
                res.Append($"Código de Pendiente: /{pending.Code}\n");
            }
            else if (pending.Type == InteractionType.Diary)
            {
                var diary = dataAccess.Dailies
                    .Include(x => x.User)
                    .First(x => x.Id == pending.ObjectId);
                res.Append($"Actualización de Diario de {diary.User.Username}\n");
                res.Append($"Actualización: {diary.Text}\n");
                res.Append($"Código de Pendiente: /{pending.Code}\n");
            }
            else if (pending.Type == InteractionType.Joke)
            {
                var joke = dataAccess.Jokes
                    .Include(x => x.User)
                    .First(x => x.Id == pending.ObjectId);
                res.Append($"Chiste de {joke.User.Username}\n");
                res.Append($"Texto: {joke.Text}\n");
                res.Append($"Código de Pendiente: /{pending.Code}\n");
                giveMeExplication = true;
            }
            else if (pending.Type == InteractionType.Meme)
            {
                var meme = dataAccess.Memes
                    .Include(x => x.User)
                    .First(x => x.Id == pending.ObjectId);
                res.Append($"Meme de {meme.User.Username}\n");
                res.Append($"Código de Pendiente: /{pending.Code}\n");
                pend = meme.FileId;
                giveMeExplication = true;
            }
            else if (pending.Type == InteractionType.RectificationToTheTeacher)
            {
                var rectificationToTheTeacher = dataAccess.RectificationToTheTeachers
                    .Include(x => x.Teacher.User)
                    .Include(x => x.User)
                    .First(x => x.Id == pending.ObjectId);
                res.Append($"Rectificación al Profesor de {rectificationToTheTeacher.User.Username}\n");
                res.Append($"Profesor: {rectificationToTheTeacher.Teacher.User.Username}\n");
                res.Append($"Texto: {rectificationToTheTeacher.Text}\n");
                res.Append($"Código de Pendiente: /{pending.Code}\n");
            }
            else
            {
                var statusphrase = dataAccess.StatusPhrases
                    .Include(x => x.User)
                    .First(x => x.Id == pending.ObjectId);
                res.Append($"Cambio de Frase de Estado de {statusphrase.User.Username}\n");
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
            var directPendings = dataAccess.DirectPendings.Where(x => x.PendingId == pending.Id).ToList();
            dataAccess.Pendings.Remove(pending);
            dataAccess.DirectPendings.RemoveRange(directPendings);
            dataAccess.SaveChanges();
        }

        public long AddDirectPending(string username, string pendingId)
        {
            var user = dataAccess.Users.Where(x => x.Username == username || x.Username == username.Substring(1)).First();

            var directPending = new DirectPending
            {
                Id = Guid.NewGuid().ToString(),
                PendingId = pendingId,
                UserId = user.Id
            };
            dataAccess.DirectPendings.Add(directPending);
            dataAccess.SaveChanges();
            return user.ChatId;
        }

        public string GetAllClassRoomWithPendings(User user)
        {
            var classRooms = dataAccess.TeachersByClassRooms
                            .Where(x => x.Teacher.UserId == user.Id)
                            .Include(x => x.ClassRoom)
                            .ToList();
            StringBuilder res = new StringBuilder();
            int i = 0;
            foreach (var classRoom in classRooms)
            {
                if (dataAccess.Pendings.Where(x=>x.ClassRoomId==classRoom.ClassRoomId).Count()!=0)
                {
                    res.Append(++i);
                    res.Append(": ");
                    res.Append(classRoom.ClassRoom.Name);
                    res.Append("\n");
                }
            }

            return res.ToString();
        }

        public (string, string) GetPendingExplicationData(Pending pending, string teacherUsername)
        {
            pending.GiveMeAnExplication = true;
            dataAccess.Pendings.Update(pending);
            dataAccess.SaveChanges();
            if (pending.Type == InteractionType.ClassIntervention)
            {
                var classIntervention = dataAccess.ClassInterventions
                    .Include(x => x.User)
                    .Include(x => x.Class)
                    .Include(x => x.Class.ClassRoom)
                    .First(x => x.Id == pending.ObjectId);
                return ($"El profesor @{teacherUsername} le está pidiendo una explicación sobre su intervención en clases({classIntervention.Text}) en la clase \"{classIntervention.Class.Title}\" del aula \"{classIntervention.Class.ClassRoom.Name}\" debido a que no entendió de qué iba.", "");
            }
            else if (pending.Type == InteractionType.Miscellaneous)
            {
                var classIntervention = dataAccess.Miscellaneous
                    .Include(x => x.User)
                    .Include(x => x.ClassRoom)
                    .First(x => x.Id == pending.ObjectId);
                return ($"El profesor @{teacherUsername} le está pidiendo una explicación sobre su miscelánea({classIntervention.Text}) en lel aula \"{classIntervention.ClassRoom.Name}\" debido a que no entendió de qué iba.", "");
            }
            else if (pending.Type == InteractionType.ClassTitle)
            {
                var classTitle = dataAccess.ClassTitles
                    .Include(x => x.Class)
                    .Include(x => x.Class.ClassRoom)
                    .Include(x => x.User)
                    .First(x => x.Id == pending.ObjectId);
                return ($"El profesor @{teacherUsername} le está pidiendo una explicación sobre su propuesta de cambio de título de la clase({classTitle.Title}) en la clase \"{classTitle.Class.Title}\" del aula \"{classTitle.Class.ClassRoom.Name}\" debido a que no entendió de qué iba.", "");
            }
            else if (pending.Type == InteractionType.Joke)
            {
                var joke = dataAccess.Jokes
                    .Include(x => x.User)
                    .Include(x => x.ClassRoom)
                    .First(x => x.Id == pending.ObjectId);
                return ($"El profesor @{teacherUsername} le está pidiendo una explicación sobre su chiste({joke.Text}) en el aula \"{joke.ClassRoom.Name}\" debido a que no entendió de qué iba.", "");
            }
            else if (pending.Type == InteractionType.Meme)
            {
                var meme = dataAccess.Memes
                    .Include(x => x.User)
                    .Include(x => x.ClassRoom)
                    .First(x => x.Id == pending.ObjectId);
                return ($"El profesor @{teacherUsername} le está pidiendo una explicación sobre su meme en el aula \"{meme.ClassRoom.Name}\" debido a que no entendió de qué iba.", meme.FileId);
            }
            else if (pending.Type == InteractionType.RectificationToTheTeacher)
            {
                var rectificationToTheTeacher = dataAccess.RectificationToTheTeachers
                    .Include(x => x.Teacher.User)
                    .Include(x => x.ClassRoom)
                    .Include(x => x.User)
                    .First(x => x.Id == pending.ObjectId);
                return ($"El profesor @{teacherUsername} le está pidiendo una explicación sobre su rectificación al profesor {rectificationToTheTeacher.Teacher.User.Username}({rectificationToTheTeacher.Text}) en el aula \"{rectificationToTheTeacher.ClassRoom.Name}\" debido a que no entendió de qué iba.", "");
            }
            else
            {
                var statusPhrase = dataAccess.StatusPhrases
                    .Include(x => x.User)
                    .Include(x => x.ClassRoom)
                    .First(x => x.Id == pending.ObjectId);
                return ($"El profesor @{teacherUsername} le está pidiendo una explicación sobre su frase de estado({statusPhrase.Phrase}) en el aula \"{statusPhrase.ClassRoom.Name}\" debido a que no entendió de qué iba.", "");
            }
        } 
    }
}

