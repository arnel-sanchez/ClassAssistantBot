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
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
        }
    }
}

