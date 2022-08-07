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
            Console.WriteLine($"The student {user.Username} is redy to change the Status Phrase");
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
            dataAccess.SaveChanges();
            Console.WriteLine($"The student {user.Username} changed the StatusPhrase");
        }
    }
}

