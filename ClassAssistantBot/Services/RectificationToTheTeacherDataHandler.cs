﻿using ClassAssistantBot.Models;

namespace ClassAssistantBot.Services
{
    public class RectificationToTheTeacherDataHandler
    {
        private DataAccess dataAccess { get; set; }

        public RectificationToTheTeacherDataHandler(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public async Task DoRectificationToTheTaecher(User user)
        {
            user.Status = UserStatus.RectificationAtTeacher;
            dataAccess.Users.Update(user);
            await dataAccess.SaveChangesAsync();
        }

        public async Task DoRectificationToTheTaecherUserName(User user, string teacherUserName)
        {
            user.Status = UserStatus.RectificationToTheTeacherUserName;
            var teacher = dataAccess.Teachers.Where(x => x.User.Username == teacherUserName).First();
            var rectification = new RectificationToTheTeacher
            {
                Id = Guid.NewGuid().ToString(),
                DateTime = DateTime.UtcNow,
                UserId = user.Id,
                Text = String.Empty,
                TeacherId = teacher.Id,
                User = user,
                Teacher = teacher,
                ClassRoomId = user.ClassRoomActiveId,
            };

            dataAccess.Users.Update(user);
            dataAccess.RectificationToTheTeachers.Add(rectification);
            await dataAccess.SaveChangesAsync();
        }

        public async Task<string> DoRectificationToTheTaecherText(User user, string text)
        {
            user.Status = UserStatus.Ready;
            var rectification = dataAccess.RectificationToTheTeachers.Where(x => x.UserId == user.Id).OrderByDescending(x => x.DateTime).First();
            rectification.Text = text;

            var random = new Random();
            string code = random.Next(100000, 999999).ToString();
            while (dataAccess.Pendings.Where(x => x.Code == code).Count() != 0)
                code = random.Next(100000, 999999).ToString();
            var pending = new Pending
            {
                Id = Guid.NewGuid().ToString(),
                Type = InteractionType.RectificationToTheTeacher,
                ClassRoomId = user.ClassRoomActiveId,
                ObjectId = rectification.Id,
                StudentId = dataAccess.Students.Where(x => x.UserId == user.Id).First().Id,
                Code = code
            };
            dataAccess.Pendings.Add(pending);

            dataAccess.Users.Update(user);
            dataAccess.RectificationToTheTeachers.Update(rectification);
            await dataAccess.SaveChangesAsync();
            return "Rectificación al profesor hecha satisfactoriamente.";
        }

        public RectificationToTheTeacher GetRectificationToTheTeacher(string id)
        {
            return dataAccess.RectificationToTheTeachers.First(x => x.Id == id);
        }
    }
}

