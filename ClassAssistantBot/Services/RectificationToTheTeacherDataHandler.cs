using ClassAssistantBot.Models;
using Microsoft.EntityFrameworkCore;

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
            var teacher = await dataAccess.Teachers.Where(x => x.User.Username == teacherUserName).FirstAsync();
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
            var rectification = await dataAccess.RectificationToTheTeachers.Where(x => x.UserId == user.Id).OrderByDescending(x => x.DateTime).FirstAsync();
            rectification.Text = text;

            var random = new Random();
            string code = random.Next(100000, 999999).ToString();
            while (dataAccess.Pendings.Where(x => x.Code == code).Count() != 0)
                code = random.Next(100000, 999999).ToString();
            var student = await dataAccess.Students.Where(x => x.UserId == user.Id).FirstAsync();
            var pending = new Pending
            {
                Id = Guid.NewGuid().ToString(),
                Type = InteractionType.RectificationToTheTeacher,
                ClassRoomId = user.ClassRoomActiveId,
                ObjectId = rectification.Id,
                StudentId = student.Id,
                Code = code
            };
            dataAccess.Pendings.Add(pending);

            dataAccess.Users.Update(user);
            dataAccess.RectificationToTheTeachers.Update(rectification);
            await dataAccess.SaveChangesAsync();
            return "Rectificación al profesor hecha satisfactoriamente.";
        }

        public async Task<RectificationToTheTeacher> GetRectificationToTheTeacher(string id)
        {
            return await dataAccess.RectificationToTheTeachers.FirstAsync(x => x.Id == id);
        }
    }
}

