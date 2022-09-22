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

        public string GetCreditsByUserName(long id, string userName, bool usercode = false, bool showTeacher = false)
        {
            var teacher = dataAccess.Teachers.Where(x => x.UserId == id).First();
            var student = dataAccess.StudentsByClassRooms
                .Where(x => x.ClassRoomId == teacher.User.ClassRoomActiveId && (x.Student.User.Username == userName.Substring(1) || x.Student.User.Username == userName.Substring(1)))
                .Include(x => x.Student)
                .ThenInclude(x => x.User)
                .FirstOrDefault();
            var user = dataAccess.Users.Where(x => x.Id == id).First();
            user.Status = UserStatus.Ready;
            dataAccess.Update(user);
            dataAccess.SaveChanges();
            if (usercode && student == null)
                return "";
            if (student == null)
                return $"No existe usuario con el username {userName}";
            var res = new StringBuilder();
            if (string.IsNullOrEmpty(user.Name))
                res = new StringBuilder($"{student.Student.User.FirstName} {student.Student.User.LastName}(@{student.Student.User.Username}):\n");
            else
                res = new StringBuilder($"{student.Student.User.Name}(@{student.Student.User.Username}):\n");
            var credits = dataAccess.Credits
                .Include(x => x.Teacher)
                .Where(x => x.UserId == student.Student.UserId && x.ClassRoomId == student.Student.User.ClassRoomActiveId)
                .GroupBy(x => x.DateTime.Date);

            foreach (var item in credits)
            {
                int i = 1;
                res.Append(item.Key.Date);
                foreach (var credit in item)
                {
                    res.Append("  ");
                    res.Append(i);
                    res.Append(": ");
                    res.Append(credit.Value);
                    res.Append(" -> ");
                    res.Append(credit.Text);
                    if (showTeacher)
                    {
                        res.Append(" -> ");
                        res.Append($"@{credit.Teacher.Username}");
                    }
                    res.Append(":\n");
                    i++;
                }
            }
            if (credits.Count() == 0)
                res.Append("No tiene créditos registrados todavía.");
            return res.ToString();
        }

        public void CreditByTeacher(User user)
        {
            user.Status = UserStatus.Credits;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
        }

        public void AssignCredit(User user)
        {
            user.Status = UserStatus.AssignCreditsStudent;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
        }

        public bool AssignCredit(User user, string username)
        {
            user.Status = UserStatus.AssignCredits;
            dataAccess.Users.Update(user);

            var student = dataAccess.Students
                .Where(x => x.User.Username == username || x.User.Username == username.Substring(1))
                .FirstOrDefault();

            if (student == null)
            {
                user.Status = UserStatus.Ready;
                dataAccess.Users.Update(user);
                dataAccess.SaveChanges();
                return false;
            }

            var random = new Random();

            var credit = new Credits
            {
                ClassRoomId = user.ClassRoomActiveId,
                DateTime = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                TeacherId = user.Id,
                UserId = student.UserId,
                Value = 0,
                Text = String.Empty,
                Code = random.Next(10000, 99999)
            };
            dataAccess.Credits.Add(credit);
            dataAccess.SaveChanges();
            return true;
        }

        public (string, long, long) AssignCreditMessage(User user, string message)
        {
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);

            var credit = dataAccess.Credits.Include(x=>x.User).Where(x => x.TeacherId == user.Id && string.IsNullOrEmpty(x.Text)).First();

            var words = message.Split(" ");

            credit.Value = int.Parse(words[0]);

            string text = "";
            for (int i = 1; i < words.Length; i++)
            {
                text += words[i] + " ";
            }
            credit.Text = text;

            dataAccess.Credits.Update(credit);
            dataAccess.SaveChanges();

            return (text, credit.User.ChatId, credit.Value);
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
                res.Append(credits[i].Value);
                res.Append(" -> ");
                res.Append(credits[i].Text);
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

        public void AddCredits(long value, long teacherId, long userId, long classRoomId, string recomendation)
        {
            var random = new Random();
            var credit = new Credits
            {
                DateTime = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                Value = value,
                UserId = userId,
                ClassRoomId = classRoomId,
                Text = recomendation,
                TeacherId = teacherId,
                Code = random.Next(10000, 99999)
            };
            dataAccess.Credits.Add(credit);
            dataAccess.SaveChanges();
        }
    }
}

