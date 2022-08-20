using System;
using ClassAssistantBot.Models;

namespace ClassAssistantBot.Services
{
    public class DailyDataHandler
    {
        private DataAccess dataAccess { get; set; }

        public DailyDataHandler(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public void UpdateDaily(User user)
        {
            user.Status = UserStatus.Daily;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
            Console.WriteLine($"The student {user.Username} is ready to update Daily");
        }

        public void UpdateDaily(long id, string message)
        {
            var user = dataAccess.Users.First(x => x.Id == id);
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            var daily = new Daily
            {
                DateTime = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                Text = message,
                UserId = user.Id
            };
            dataAccess.Dailies.Add(daily);
            var pending = new Pending
            {
                Id = Guid.NewGuid().ToString(),
                Type = InteractionType.Daily,
                ClassRoomId = user.ClassRoomActiveId,
                ObjectId = daily.Id,
                StudentId = dataAccess.Students.Where(x => x.UserId == id).First().Id
            };
            dataAccess.Pendings.Add(pending);
            dataAccess.SaveChanges();
        }
    }
}

