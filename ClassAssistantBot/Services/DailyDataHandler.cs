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
            dataAccess.SaveChanges();
            var dailies = dataAccess.Dailies.OrderByDescending(x => x.DateTime).ToList();
            int count = 0;
            DateTime dateTime = DateTime.Today;
            foreach (var item in dailies)
            {
                if(item.DateTime.Date == dateTime)
                {
                    count++;
                    dateTime.AddDays(-1);
                }
                else
                {
                    break;
                }
            }
            var credit = new Credits
            {
                Id = Guid.NewGuid().ToString(),
                DateTime = DateTime.Now,
                UserId = user.Id,
                ClassRoomId = user.ClassRoomActiveId,
                Value = count * 10000,
                Text = $"Has recibido {count * 10000} créditos por haber actualizado tu diario {count} días seguidos."
            };
            dataAccess.Credits.Add(credit);
            dataAccess.SaveChanges();
        }

        public Daily GetDaily(string dailyId)
        {
            return dataAccess.Dailies.First(x => x.Id == dailyId);
        }
    }
}

