using System;
using ClassAssistantBot.Models;

namespace ClassAssistantBot.Services
{
    public class UserDataHandler
    {
        private DataAccess dataAccess { get; set; }

        public UserDataHandler(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public void CreateUser(Telegram.BotAPI.AvailableTypes.User appUser, long chatId)
        {
            var user = new ClassAssistantBot.Models.User
            {
                TelegramId = appUser.Id,
                Created = DateTime.UtcNow,
                FirstName = "",
                LastName = "",
                Name="",
                IsBot = appUser.IsBot,
                LanguageCode = appUser.LanguageCode,
                Username = appUser.Username,
                ChatId = chatId,
                Role = Roles.Student,
                Status = UserStatus.Created
            };

            dataAccess.Users.Add(user);
            dataAccess.SaveChanges();
            Console.WriteLine($"Inserting the new user {appUser.Username}");
        }

        public User GetUser(long id)
        {
            return dataAccess.Users.Where(x => x.TelegramId == id).FirstOrDefault();
        }

        public void VerifyUser(User user)
        {
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
        }

        public void CancelAction(User user)
        {
            if(user.Status == UserStatus.AssignCredits)
            {
                var credit = dataAccess.Credits.Where(x => x.TeacherId == user.Id && string.IsNullOrEmpty(x.Text)).First();
                dataAccess.Remove(credit);
            }
            else if(user.Status == UserStatus.ClassTitleSelect)
            {
                var classTitle = dataAccess.ClassTitles.Where(x => x.UserId == user.Id && string.IsNullOrEmpty(x.Title)).First();
                dataAccess.Remove(classTitle);
            }
            else if (user.Status == UserStatus.ClassInterventionSelect)
            {
                var classIntervention = dataAccess.ClassInterventions
                .Where(x => x.UserId == user.Id && !x.Finished)
                .First();
                dataAccess.ClassInterventions.Remove(classIntervention);
            }
            else if(user.Status == UserStatus.RectificationToTheTeacherUserName)
            {
                var rectification = dataAccess.RectificationToTheTeachers
                    .Where(x => x.UserId == user.Id)
                    .OrderByDescending(x => x.DateTime)
                    .First();
                dataAccess.RectificationToTheTeachers.Remove(rectification);
            }
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
        }
    }
}

