using ClassAssistantBot.Models;
using Microsoft.EntityFrameworkCore;

namespace ClassAssistantBot.Services
{
    public class MiscellaneousDataHandler
    {
        private DataAccess dataAccess { get; set; }

        public MiscellaneousDataHandler(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public async Task CreateMiscellaneous(User user)
        {
            user.Status = UserStatus.Miscellaneous;
            dataAccess.Users.Update(user);
            await dataAccess.SaveChangesAsync();
        }

        public async Task CreateMiscellaneous(User user, string message)
        {
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            var miscellaneous = new Miscellaneous
            {
                DateTime = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                Text = message,
                UserId = user.Id,
                ClassRoomId = user.ClassRoomActiveId,
            };
            dataAccess.Miscellaneous.Add(miscellaneous);
            var random = new Random();
            string code = random.Next(100000, 999999).ToString();
            while (dataAccess.Pendings.Where(x => x.Code == code).Count() != 0)
                code = random.Next(100000, 999999).ToString();
            var pending = new Pending
            {
                Id = Guid.NewGuid().ToString(),
                Type = InteractionType.Miscellaneous,
                ClassRoomId = user.ClassRoomActiveId,
                ObjectId = miscellaneous.Id,
                StudentId = dataAccess.Students.Where(x => x.UserId == user.Id).First().Id,
                Code = code
            };
            dataAccess.Pendings.Add(pending);
            await dataAccess.SaveChangesAsync();
        }

        public async Task<Miscellaneous> GetMiscellaneous(string miscellaneousId)
        {
            return await dataAccess.Miscellaneous.Where(x => x.Id == miscellaneousId).FirstAsync();
        }
    }
}

