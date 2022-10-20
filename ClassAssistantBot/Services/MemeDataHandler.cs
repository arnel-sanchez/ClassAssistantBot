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
                ClassRoomId = user.ClassRoomActiveId
            };
            dataAccess.Memes.Add(meme);
            var random = new Random();
            string code = random.Next(100000, 999999).ToString();
            while (dataAccess.Pendings.Where(x => x.Code == code).Count() != 0)
                code = random.Next(100000, 999999).ToString();
            var pending = new Pending
            {
                Id = Guid.NewGuid().ToString(),
                Type = InteractionType.Meme,
                ClassRoomId = user.ClassRoomActiveId,
                ObjectId = meme.Id,
                StudentId = dataAccess.Students.Where(x => x.UserId == id).First().Id,
                Code = code
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
                ClassRoomId = user.ClassRoomActiveId
            };
            dataAccess.Memes.Add(meme);

            var random = new Random();
            string code = random.Next(100000, 999999).ToString();
            while (dataAccess.Pendings.Where(x => x.Code == code).Count() != 0)
                code = random.Next(100000, 999999).ToString();
            var pending = new Pending
            {
                Id = Guid.NewGuid().ToString(),
                Type = InteractionType.Meme,
                ClassRoomId = user.ClassRoomActiveId,
                ObjectId = meme.Id,
                StudentId = dataAccess.Students.Where(x => x.UserId == id).First().Id,
                Code = code
            };
            dataAccess.Pendings.Add(pending);
            dataAccess.SaveChanges();
        }

        public Meme GetMeme(string memeId)
        {
            return dataAccess.Memes.Where(x => x.Id == memeId).First();
        }
    }
}

