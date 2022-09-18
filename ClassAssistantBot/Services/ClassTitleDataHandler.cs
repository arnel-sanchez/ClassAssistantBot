using System;
using ClassAssistantBot.Models;

namespace ClassAssistantBot.Services
{
    public class ClassTitleDataHandler
    {
        private DataAccess dataAccess { get; set; }

        public ClassTitleDataHandler(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public List<Class> GetClasses(User user)
        {
            var res = dataAccess.Classes.Where(x => x.ClassRoomId == user.ClassRoomActiveId).ToList();
            if(res.Count == 0)
            {
                user.Status = UserStatus.Ready;
                dataAccess.Users.Update(user);
                dataAccess.SaveChanges();
            }
            return res;
        }

        public ClassTitle GetClassTitle(string classTitleId)
        {
            return dataAccess.ClassTitles.First(x => x.Id == classTitleId);
        }

        public void ChangeClassTitle(User user)
        {
            user.Status = UserStatus.ClassTitle;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
        }

        public void ChangeClassTitle(User user, long classID)
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
            dataAccess.SaveChanges();
        }

        public void ChangeClassTitle(User user, string newTitle)
        {
            user.Status = UserStatus.Ready;

            var classTitle = dataAccess.ClassTitles.OrderByDescending(x => x.DateTime).First();

            classTitle.Title = newTitle;

            var random = new Random();
            var pending = new Pending
            {
                Id = Guid.NewGuid().ToString(),
                Type = InteractionType.ClassTitle,
                ClassRoomId = user.ClassRoomActiveId,
                ObjectId = classTitle.Id,
                StudentId = dataAccess.Students.Where(x => x.UserId == user.Id).First().Id,
                Code = random.Next(1000, 9999).ToString()
            };
            dataAccess.Pendings.Add(pending);

            dataAccess.Users.Update(user);
            dataAccess.ClassTitles.Update(classTitle);
            dataAccess.SaveChanges();
        }

        public void CreateClass(User user)
        {
            user.Status = UserStatus.CreateClass;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
        }

        public void CreateClass(User user, string title)
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
            dataAccess.SaveChanges();
        }
    }
}