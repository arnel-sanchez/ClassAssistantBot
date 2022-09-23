using System;
using System.Text;
using ClassAssistantBot.Models;
using Microsoft.EntityFrameworkCore;

namespace ClassAssistantBot.Services
{
    public class ClassRoomDataHandler
    {
        private DataAccess dataAccess { get; set; }

        public ClassRoomDataHandler(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public void CreateClassRoom(User user)
        {
            user.Status = UserStatus.TeacherCreatingClass;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
        }

        public string CreateClassRoom(long id, string name)
        {
            var teacher = dataAccess.Teachers.Where(x => x.User.TelegramId == id).FirstOrDefault();

            var random = new Random();
            var studentAccessKey = random.Next(1000000, 9999999);
            while (dataAccess.ClassRooms.Where(x => x.StudentAccessKey == studentAccessKey).FirstOrDefault() != null)
            {
                studentAccessKey = random.Next(1000000, 9999999);
            }

            var teacherAccessKey = random.Next(1000000, 9999999);
            while (dataAccess.ClassRooms.Where(x => x.TeacherAccessKey == teacherAccessKey).FirstOrDefault() != null)
            {
                teacherAccessKey = random.Next(1000000, 9999999);
            }

            var classRoom = new ClassRoom
            {
                Name = name,
                StudentsByClassRooms = new List<StudentByClassRoom>(),
                TeachersByClassRooms = new List<TeacherByClassRoom>(),
                StudentAccessKey = studentAccessKey,
                TeacherAccessKey = teacherAccessKey
            };

            dataAccess.Add(classRoom);
            dataAccess.SaveChanges();

            classRoom = dataAccess.ClassRooms.Where(x => x.TeacherAccessKey == teacherAccessKey && x.StudentAccessKey == studentAccessKey).FirstOrDefault();

            var user = dataAccess.Users.Where(x => x.TelegramId == id).FirstOrDefault();

            if (teacher == null)
            {
                teacher = new Teacher
                {
                    UserId = user.Id,
                    TeachersByClassRooms = new List<TeacherByClassRoom>(),
                    Id = Guid.NewGuid().ToString()
                };
            }

            var teacherByClassRoom = new TeacherByClassRoom
            {
                ClassRoomId = classRoom.Id,
                Id = Guid.NewGuid().ToString(),
                Teacher = teacher
            };

            user.IsTecaher = true;
            user.Status = UserStatus.Ready;
            user.ClassRoomActiveId = classRoom.Id;

            dataAccess.Users.Update(user);

            dataAccess.Add(teacherByClassRoom);
            dataAccess.SaveChanges();

            return $"El código de acceso para sus estudiantes {classRoom.StudentAccessKey}\n\nEl código de acceso para los demás profesores es {classRoom.TeacherAccessKey}";
        }

        public string ChangeClassRoom(long id)
        {
            var user = dataAccess.Users.Where(x => x.Id == id).First();
            user.Status = UserStatus.ChangeClassRoom;
            dataAccess.Update(user);
            dataAccess.SaveChanges();
            var list = dataAccess.TeachersByClassRooms.Where(x => x.Teacher.UserId == id).Include(x => x.ClassRoom).ToList();
            var res = new StringBuilder();
            foreach (var item in list)
            {
                res.Append(item.ClassRoom.Name);
                res.Append(": ");
                res.Append(item.ClassRoom.Id);
                res.Append("\n");
            }
            return res.ToString();
        }

        public void ChangeClassRoom(long id, long classRoomID)
        {
            var user = dataAccess.Users.Where(x => x.Id == id).First();
            var classRoom = dataAccess.ClassRooms.Where(x => x.Id == classRoomID).FirstOrDefault();
            if (classRoom != null)
            {
                user.ClassRoomActiveId = classRoom.Id;
                user.Status = UserStatus.Ready;
                dataAccess.Users.Update(user);
                dataAccess.SaveChanges();
            }
        }

        public string SeeClassRoomActive(User user)
        {
            return dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).First().Name;
        }

        public string GetClassesCreated(User user)
        {
            var classes = dataAccess.Classes.Where(x => x.ClassRoomId == user.ClassRoomActiveId).ToList();

            StringBuilder stringBuilder = new StringBuilder();

            int k = 1;
            foreach (var @class in classes)
            {
                stringBuilder.Append($"{k}: {@class.Title}\n");
                k++;
            }

            return stringBuilder.ToString();
        }
    }
}

