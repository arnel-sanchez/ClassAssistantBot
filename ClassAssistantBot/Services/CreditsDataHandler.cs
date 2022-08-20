using System;
using System.Text;
using ClassAssistantBot.Models;
using Microsoft.EntityFrameworkCore;

namespace ClassAssistantBot.Services
{
    public class CreditsDataHandler
    {
        private DataAccess dataAccess { get; set; }

        public CreditsDataHandler(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public string GetCreditsByUserName(long id, string userName)
        {
            var teacher = dataAccess.Teachers.Where(x => x.UserId == id).First();
            var student = dataAccess.StudentsByClassRooms
                .Where(x => x.ClassRoomId == teacher.User.ClassRoomActiveId && x.Student.User.Username == userName.Substring(1))
                .Include(x => x.Student)
                .ThenInclude(x => x.User)
                .FirstOrDefault();
            var user = dataAccess.Users.Where(x => x.Id == id).First();
            user.Status = UserStatus.Ready;
            dataAccess.Update(user);
            dataAccess.SaveChanges();
            if (student == null)
                return $"No existe usuario con el username {userName}";
            var res = new StringBuilder($"{student.Student.User.FirstName} {student.Student.User.LastName}(@{student.Student.User.Username}):\n");
            var credits = dataAccess.Credits.Where(x => x.UserId == student.Student.UserId && x.ClassRoomId == student.Student.User.ClassRoomActiveId).ToList();
            for (int i = 0; i < credits.Count; i++)
            {
                res.Append(i + 1);
                res.Append(": ");
                res.Append(credits[i].Text);
                res.Append(" -> ");
                res.Append(credits[i].Value);
                res.Append("\n");
            }
            if (credits.Count == 0)
                res.Append("No tiene créditos registrados todavía.");
            return res.ToString();
        }

        public void CreditByTeacher(User user)
        {
            user.Status = UserStatus.Credits;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
        }

        public string GetCreditsById(long id)
        {
            var student = dataAccess.Students.Where(x => x.UserId == id).First();
            var classRoom = dataAccess.ClassRooms.Where(x => x.Id == student.User.ClassRoomActiveId).First();
            var credits = dataAccess.Credits.Where(x => x.UserId == student.UserId && x.ClassRoomId == student.User.ClassRoomActiveId).Include(x => x.ClassRoom).ToList();

            var user = dataAccess.Users.Where(x => x.Id == id).First();
            user.Status = UserStatus.Ready;
            dataAccess.Update(user);
            dataAccess.SaveChanges();
            var res = new StringBuilder($"Créditos en el aula {classRoom.Name}:\n");

            for (int i = 0; i < credits.Count; i++)
            {
                res.Append(i + 1);
                res.Append(": ");
                res.Append(credits[i].Text);
                res.Append(" -> ");
                res.Append(credits[i].Value);
                res.Append("\n");
            }
            if (credits.Count == 0)
                res.Append("No tiene créditos registrados todavía.");
            else
            {
                res.Append("Total: ");
                res.Append(credits.Sum(x => x.Value).ToString());
            }
            return res.ToString();
        }
    }
}

