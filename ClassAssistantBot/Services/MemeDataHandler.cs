using System;
using ClassAssistantBot.Models;

namespace ClassAssistantBot.Services
{
    public class MemeDataHandler
    {
        private DataAccess dataAccess { get; set; }

        public MemeDataHandler(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public void SendMeme(User user)
        {
            user.Status = UserStatus.Meme;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
            Console.WriteLine($"The student {user.Username} is redy to send a Meme");
        }

        public void SendMeme(long id, Telegram.BotAPI.AvailableTypes.Document document)
        {
            var user = dataAccess.Users.First(x => x.Id == id);
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            var meme = new Meme
            {
                DateTime = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                FileId = document.FileId,
                FieldUniqueId = document.FileUniqueId,
                UserId = user.Id,
                FileSize = document.FileSize,
                MimeType = document.MimeType,
                FileName = document.FileName,
                Height = document.Thumb.Height,
                Width = document.Thumb.Width,
            };
            dataAccess.Memes.Add(meme);
            dataAccess.SaveChanges();
            Console.WriteLine($"The student {user.Username} sent a Meme");
        }
    }
}

