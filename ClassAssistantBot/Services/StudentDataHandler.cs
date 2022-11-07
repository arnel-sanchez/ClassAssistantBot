using System;
using System.Text;
using ClassAssistantBot.Models;
using Microsoft.EntityFrameworkCore;

namespace ClassAssistantBot.Services
{
    public class StudentResult
    {
        public long Credits { get; set; }

        public string Username { get; set; }

        public string Name { get; set; }
    }

    public class StudentDataHandler
    {
        private DataAccess dataAccess { get; set; }

        public StudentDataHandler(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public async Task<(string, string)> RemoveStudentFromClassRoom(long id, string userName)
        {
            var teacher = dataAccess.Users.First(x => x.Id == id);
            var user = dataAccess.Users.FirstOrDefault(x => x.Username == userName.Substring(1) || x.Username == userName);

            if (user == null)
                return ("No existe extudiante con ese nombre de usuario, por favor atienda lo que hace.", "");
            else
            {
                teacher.Status = UserStatus.Ready;
                var userByClassRoom = dataAccess.StudentsByClassRooms.Where(x => x.Student.UserId == user.Id && teacher.ClassRoomActiveId == x.ClassRoomId);
                user.ClassRoomActiveId = 0;
                user.Status = UserStatus.Ready;
                dataAccess.Users.Update(user);
                dataAccess.StudentsByClassRooms.RemoveRange(userByClassRoom);
                dataAccess.Users.Update(teacher);
                await dataAccess.SaveChangesAsync();
                return ("Estudiante sacado del aula con éxito", user.ChatId.ToString());
            }
        }

        public async Task<string> RemoveStudentFromClassRoom(long id)
        {
            var user = dataAccess.Users.Where(x => x.Id == id).First();
            user.Status = UserStatus.RemoveStudentFromClassRoom;
            dataAccess.Update(user);
            await dataAccess.SaveChangesAsync();
            var list = dataAccess.StudentsByClassRooms
                .Where(x => x.ClassRoomId == user.ClassRoomActiveId)
                .Include(x => x.Student.User).ToList();
            var res = new StringBuilder();
            foreach (var item in list)
            {
                if (string.IsNullOrEmpty(item.Student.User.Name))
                {
                    res.Append(item.Student.User.FirstName);
                    res.Append(" ");
                    res.Append(item.Student.User.LastName);
                }
                else
                {
                    res.Append(item.Student.User.Name);
                }
                res.Append(": @");
                res.Append(item.Student.User.Username);
                res.Append("\n");
            }
            return res.ToString();
        }

        public async Task StudentEnterClass(User user)
        {
            user.Status = UserStatus.StudentEnteringClass;
            dataAccess.Users.Update(user);
            await dataAccess.SaveChangesAsync();
            Console.WriteLine($"The student {user.Username} is entering class");
        }

        public async Task<(string, bool)> AssignStudentAtClass(long id, string codeText, bool success)
        {
            success = false;
            int code = 0;
            bool canParse = int.TryParse(codeText, out code);

            if (!canParse)
                return ($"No hay aula creada con el código de acceso {codeText}", success);

            var user = await dataAccess.Users.Where(x => x.TelegramId == id).FirstOrDefaultAsync();
            var classRoom = await dataAccess.ClassRooms.Where(x => x.StudentAccessKey == code).FirstOrDefaultAsync();
            if (classRoom == null)
                return ($"No hay aula creada con el código de acceso {code}", success);
            else
            {
                var student = await dataAccess.Students.Where(x => x.UserId == user.Id).FirstOrDefaultAsync();

                if (student == null)
                {
                    student = new Student
                    {
                        UserId = user.Id,
                        StudentsClassRooms = new List<StudentByClassRoom>(),
                        Id = Guid.NewGuid().ToString()
                    };
                }

                var studentByClassRoom = new StudentByClassRoom
                {
                    ClassRoomId = classRoom.Id,
                    Id = Guid.NewGuid().ToString(),
                    Student = student
                };

                user.Status = UserStatus.Ready;
                user.IsTecaher = false;
                user.ClassRoomActiveId = classRoom.Id;

                dataAccess.Users.Update(user);
                dataAccess.StudentsByClassRooms.Add(studentByClassRoom);
                await dataAccess.SaveChangesAsync();
                Console.WriteLine($"The student {user.Username} has entered class");
                success = true;
                return ($"Ha entrado en el aula satisfactoriamente", success);
            }
        }

        public async Task<string> GetStudentsOnClassByTeacherId(long id)
        {
            var teacher = await dataAccess.Teachers.Where(x => x.UserId == id).Include(x => x.User).FirstAsync();
            var classRoom = await dataAccess.ClassRooms.Where(x => x.Id == teacher.User.ClassRoomActiveId).FirstAsync();
            var students = await dataAccess.StudentsByClassRooms
                .Where(x => x.ClassRoomId == teacher.User.ClassRoomActiveId)
                .Include(x => x.ClassRoom)
                .Include(x => x.Student)
                .ThenInclude(x => x.User)
                .ToListAsync();

            List<StudentResult> studentResults = new List<StudentResult>();

            foreach (var student in students)
            {
                studentResults.Add(new StudentResult
                {
                    Credits = await dataAccess.Credits.Where(x => x.UserId == student.Student.UserId && x.ClassRoomId == student.Student.User.ClassRoomActiveId).SumAsync(x => x.Value),
                    Username = student.Student.User.Username,
                    Name = string.IsNullOrEmpty(student.Student.User.Name) ? student.Student.User.FirstName + " " + student.Student.User.LastName : student.Student.User.Name
                });
            }

            var res = new StringBuilder($"Estudantes inscritos en el aula {classRoom.Name}:\n");
            studentResults = studentResults.OrderByDescending(x => x.Credits).ToList();
            for (int i = 0; i < studentResults.Count; i++)
            {
                res.Append(i + 1);
                res.Append(": ");
                res.Append(studentResults[i].Credits);
                res.Append(" -> ");
                res.Append(studentResults[i].Name);
                res.Append(" (/");
                res.Append(studentResults[i].Username);
                res.Append(")\n");
            }
            return res.ToString();
        }

        public async Task<List<Student>> GetStudents(User teacher)
        {
            var classRoom = await dataAccess.ClassRooms.Where(x => x.Id == teacher.ClassRoomActiveId).FirstAsync();
            var students = await dataAccess.StudentsByClassRooms
                .Where(x => x.ClassRoomId == teacher.ClassRoomActiveId)
                .Include(x => x.ClassRoom)
                .Include(x => x.Student)
                .ThenInclude(x => x.User)
                .ToListAsync();

            var list = new List<Student>();

            foreach (var student in students)
            {
                list.Add(student.Student);
            }

            return list;
        }
    }
}

