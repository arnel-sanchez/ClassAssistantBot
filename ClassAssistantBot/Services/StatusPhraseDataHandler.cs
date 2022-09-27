using System;
using ClassAssistantBot.Models;

namespace ClassAssistantBot.Services
{
    public class StatusPhraseDataHandler
    {
        private DataAccess dataAccess { get; set; }

        public StatusPhraseDataHandler(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public string ChangeStatusPhrase(User user)
        {
            user.Status = UserStatus.StatusPhrase;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
            var res = dataAccess.StatusPhrases.Where(x => x.UserId == user.Id).ToList().MaxBy(x => x.DateTime);
            if (res == null)
                return "";
            else
                return res.Phrase;
        }

        public void ChangeStatusPhrase(long id, string message)
        {
            var user = dataAccess.Users.First(x => x.Id == id);
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            var statusPhrase = new StatusPhrase
            {
                DateTime = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                Phrase = message,
                UserId = user.Id
            };
            dataAccess.StatusPhrases.Add(statusPhrase);
            var random = new Random();
            string code = random.Next(100000, 999999).ToString();
            while (dataAccess.Pendings.Where(x => x.Code == code).Count() != 0)
                code = random.Next(100000, 999999).ToString();
            var pending = new Pending
            {
                Id = Guid.NewGuid().ToString(),
                Type = InteractionType.StatusPhrase,
                ClassRoomId = user.ClassRoomActiveId,
                ObjectId = statusPhrase.Id,
                StudentId = dataAccess.Students.Where(x => x.UserId == id).First().Id,
                Code = code
            };
            dataAccess.Pendings.Add(pending);
            dataAccess.SaveChanges();
        }

        public StatusPhrase GetStatusPhrase(string id)
        {
            return dataAccess.StatusPhrases.First(x => x.Id == id);
        }
    }
}

