using System;
using ClassAssistantBot.Models;
using Microsoft.EntityFrameworkCore;

namespace ClassAssistantBot.Services
{
    public class ClassTitleDataHandler
    {
        private DataAccess dataAccess { get; set; }

        public ClassTitleDataHandler(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public async Task<List<Class>> GetClasses(User user)
        {
            var res = dataAccess.Classes.Where(x => x.ClassRoomId == user.ClassRoomActiveId).ToList();
            if(res.Count == 0)
            {
                user.Status = UserStatus.Ready;
                dataAccess.Users.Update(user);
                await dataAccess.SaveChangesAsync();
            }
            return res;
        }

        public async Task<ClassTitle> GetClassTitle(string classTitleId)
        {
            return await dataAccess.ClassTitles.FirstAsync(x => x.Id == classTitleId);
        }

        public async Task ChangeClassTitle(User user)
        {
            user.Status = UserStatus.ClassTitle;
            dataAccess.Users.Update(user);
            await dataAccess.SaveChangesAsync();
        }

        public async Task ChangeClassTitle(User user, long classID)
        {
            user.Status = UserStatus.ClassTitleSelect;

            var classTitle = new ClassTitle
            {
                ClassId = classID,
                DateTime = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                Title = String.Empty,
                UserId = user.Id
            };

            dataAccess.Users.Update(user);
            dataAccess.ClassTitles.Add(classTitle);
            await dataAccess.SaveChangesAsync();
        }

        public async Task ChangeClassTitle(User user, string newTitle)
        {
            user.Status = UserStatus.Ready;

            var classTitle = dataAccess.ClassTitles.OrderByDescending(x => x.DateTime).First();

            classTitle.Title = newTitle;

            var random = new Random();
            string code = random.Next(100000, 999999).ToString();
            while (dataAccess.Pendings.Where(x => x.Code == code).Count() != 0)
                code = random.Next(100000, 999999).ToString();
            var pending = new Pending
            {
                Id = Guid.NewGuid().ToString(),
                Type = InteractionType.ClassTitle,
                ClassRoomId = user.ClassRoomActiveId,
                ObjectId = classTitle.Id,
                StudentId = dataAccess.Students.Where(x => x.UserId == user.Id).First().Id,
                Code = code
            };
            dataAccess.Pendings.Add(pending);

            dataAccess.Users.Update(user);
            dataAccess.ClassTitles.Update(classTitle);
            await dataAccess.SaveChangesAsync();
        }

        public async Task CreateClass(User user)
        {
            user.Status = UserStatus.CreateClass;
            dataAccess.Users.Update(user);
            await dataAccess.SaveChangesAsync();
        }

        public async Task CreateClass(User user, string title)
        {
            user.Status = UserStatus.Ready;

            var class1 = new Class
            {
                ClassRoomId = user.ClassRoomActiveId,
                Date = DateTime.UtcNow,
                Title = title
            };

            dataAccess.Users.Update(user);
            dataAccess.Classes.Add(class1);
            await dataAccess.SaveChangesAsync();
        }
    }
}