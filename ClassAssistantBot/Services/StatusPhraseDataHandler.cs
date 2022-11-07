using System;
using ClassAssistantBot.Models;
using Microsoft.EntityFrameworkCore;

namespace ClassAssistantBot.Services
{
    public class StatusPhraseDataHandler
    {
        private DataAccess dataAccess { get; set; }

        public StatusPhraseDataHandler(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public async Task<string> ChangeStatusPhrase(User user)
        {
            user.Status = UserStatus.StatusPhrase;
            dataAccess.Users.Update(user);
            await dataAccess.SaveChangesAsync();
            var res = await dataAccess.StatusPhrases.Where(x => x.UserId == user.Id).ToListAsync();
            var res1 = res.MaxBy(x => x.DateTime);
            if (res1 == null)
                return "";
            else
                return res1.Phrase;
        }

        public async Task ChangeStatusPhrase(long id, string message)
        {
            var user = dataAccess.Users.First(x => x.Id == id);
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            var statusPhrase = new StatusPhrase
            {
                DateTime = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                Phrase = message,
                UserId = user.Id,
                ClassRoomId = user.ClassRoomActiveId
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
            await dataAccess.SaveChangesAsync();
        }

        public async Task<StatusPhrase> GetStatusPhrase(string id)
        {
            return await dataAccess.StatusPhrases.FirstAsync(x => x.Id == id);
        }
    }
}

