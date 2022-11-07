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

        public async Task CreateTeacher(User user)
        {
            user.Status = UserStatus.CreatingTecaher;
            dataAccess.Users.Update(user);
            await dataAccess.SaveChangesAsync();
            Console.WriteLine($"The user {user.Username} is creating a teacher");
        }

        public async Task TeacherEnterClass(User user)
        {
            user.Status = UserStatus.TeacherEnteringClass;
            dataAccess.Users.Update(user);
            await dataAccess.SaveChangesAsync();
            Console.WriteLine($"The teacher {user.Username} is entering class");
        }

        public async Task<(string, bool)> AssignTeacherAtClass(long id, string codeText, bool success)
        {
            int code = 0;
            success = false;
            bool canParse = int.TryParse(codeText, out code);

            if (!canParse)
                return ($"No hay aula creada con el código de acceso {codeText}", success);
            var user = await dataAccess.Users.Where(x => x.TelegramId == id).FirstOrDefaultAsync();
            var classRoom = await dataAccess.ClassRooms.Where(x => x.TeacherAccessKey == code).FirstOrDefaultAsync();
            if (classRoom == null)
                return ($"No hay aula creada con el código de acceso {code}", success);
            else
            {
                var teacher = await dataAccess.Teachers.Where(x => x.UserId == user.Id).FirstOrDefaultAsync();

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
                await dataAccess.SaveChangesAsync();
                Console.WriteLine($"The teacher {user.Username} has entered class");
                success = true;
                return ($"Ha entrado en el aula satiscatoriamente", success);
            }
        }

        public async Task<int> GetStudentAccessKey(long id)
        {
            var user = await dataAccess.Users.Where(x => x.Id == id).FirstAsync();
            var res = await dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).FirstAsync();

            return res.StudentAccessKey;
        }

        public async Task<int> GetTeacherAccessKey(long id)
        {
            var user = await dataAccess.Users.Where(x => x.Id == id).FirstAsync();
            var res = await dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).FirstAsync();
            return res.TeacherAccessKey;
        }

        public async Task<List<Teacher>> GetTeachers(User user)
        {
            var teacherByClassRoom = await dataAccess.TeachersByClassRooms
                .Where(x => x.ClassRoomId == user.ClassRoomActiveId)
                .Include(x => x.ClassRoom)
                .Include(x => x.Teacher)
                .ThenInclude(x => x.User)
                .ToListAsync();
            var res = new List<Teacher>();
            foreach (var item in teacherByClassRoom)
            {
                res.Add(item.Teacher);
            }

            return res;
        }

        public async Task<bool> ExistTeacher(string username)
        {
            var teacher = await dataAccess.Teachers.Where(x => x.User.Username == username || x.User.Username == username.Substring(1)).FirstOrDefaultAsync();

            return teacher != null;
        }
    }
}
