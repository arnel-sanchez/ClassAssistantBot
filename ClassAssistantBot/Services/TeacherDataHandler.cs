using ClassAssistantBot.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassAssistantBot.Services
{
    public class TeacherDataHandler
    {
        private DataAccess dataAccess { get; set; }
        
        public TeacherDataHandler(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public void CreateTeacher(User user)
        {
            user.Status = UserStatus.CreatingTecaher;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
            Console.WriteLine($"The user {user.Username} is creating a teacher");
        }

        public void TeacherEnterClass(User user)
        {
            user.Status = UserStatus.TeacherEnteringClass;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
            Console.WriteLine($"The teacher {user.Username} is entering class");
        }

        public string AssignTeacherAtClass(long id, string codeText)
        {
            int code = 0;
            bool canParse = int.TryParse(codeText, out code);

            if (!canParse)
                return $"No hay aula creada con el código de acceso {codeText}";
            var user = dataAccess.Users.Where(x => x.TelegramId == id).FirstOrDefault();
            var classRoom = dataAccess.ClassRooms.Where(x => x.TeacherAccessKey == code).FirstOrDefault();
            if (classRoom == null)
                return $"No hay aula creada con el código de acceso {code}";
            else
            {
                var teacher = dataAccess.Teachers.Where(x => x.UserId == user.Id).FirstOrDefault();

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
                    ClassRoom = classRoom,
                    Id = Guid.NewGuid().ToString(),
                    Teacher = teacher
                };

                user.IsTecaher = true;
                user.Status = UserStatus.Ready;
                user.ClassRoomActiveId = classRoom.Id;

                dataAccess.Users.Update(user);
                dataAccess.Add(teacherByClassRoom);
                dataAccess.SaveChanges();
                Console.WriteLine($"The teacher {user.Username} has entered class");
                return $"Ha entrado en el aula satiscatoriamente";
            }
        }

        public int GetStudentAccessKey(long id)
        {
            var user = dataAccess.Users.Where(x => x.Id == id).First();
            return dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).First().StudentAccessKey;
        }

        public int GetTeacherAccessKey(long id)
        {
            var user = dataAccess.Users.Where(x => x.Id == id).First();
            return dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).First().TeacherAccessKey;
        }

        public List<Teacher> GetTeachers(User user)
        {
            var teacherByClassRoom = dataAccess.TeachersByClassRooms
                .Where(x => x.ClassRoomId == user.ClassRoomActiveId)
                .Include(x => x.ClassRoom)
                .Include(x => x.Teacher)
                .ThenInclude(x => x.User)
                .ToList();
            var res = new List<Teacher>();
            foreach (var item in teacherByClassRoom)
            {
                res.Add(item.Teacher);
            }

            return res;
        }
    }
}
