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
            var pending = new Pending
            {
                Id = Guid.NewGuid().ToString(),
                Type = InteractionType.Meme,
                ClassRoomId = user.ClassRoomActiveId,
                ObjectId = meme.Id,
                StudentId = dataAccess.Students.Where(x => x.UserId == id).First().Id
            };
            dataAccess.Pendings.Add(pending);
            dataAccess.SaveChanges();
        }

        public void SendMeme(long id, Telegram.BotAPI.AvailableTypes.PhotoSize photo)
        {
            var user = dataAccess.Users.First(x => x.Id == id);
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            var meme = new Meme
            {
                DateTime = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                FileId = photo.FileId,
                FieldUniqueId = photo.FileUniqueId,
                UserId = user.Id,
                FileSize = photo.FileSize,
                MimeType = "",
                FileName = "",
                Height = photo.Height,
                Width = photo.Width,
            };
            dataAccess.Memes.Add(meme);
            var pending = new Pending
            {
                Id = Guid.NewGuid().ToString(),
                Type = InteractionType.Meme,
                ClassRoomId = user.ClassRoomActiveId,
                ObjectId = meme.Id,
                StudentId = dataAccess.Students.Where(x => x.UserId == id).First().Id
            };
            dataAccess.Pendings.Add(pending);
            dataAccess.SaveChanges();
        }
    }
}

