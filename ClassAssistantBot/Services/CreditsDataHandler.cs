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

        public async Task<string> GetCreditsByUserName(long id, string userName, bool usercode = false, bool showTeacher = false)
        {
            var teacher = dataAccess.Teachers.Where(x => x.UserId == id).First();
            var student = dataAccess.StudentsByClassRooms
                .Where(x => x.ClassRoomId == teacher.User.ClassRoomActiveId && (x.Student.User.Username == userName.Substring(1) || x.Student.User.Username == userName))
                .Include(x => x.Student)
                .ThenInclude(x => x.User)
                .FirstOrDefault();
            var user = dataAccess.Users.Where(x => x.Id == id).First();
            user.Status = UserStatus.Ready;
            dataAccess.Update(user);
            await dataAccess.SaveChangesAsync();
            if (usercode && student == null)
                return "";
            if (student == null)
                return $"No existe usuario con el username {userName}";
            var res = new StringBuilder();
            if (!string.IsNullOrEmpty(student.Student.User.FirstName))
                res = new StringBuilder($"{student.Student.User.FirstName} {student.Student.User.LastName}(@{student.Student.User.Username}):\n");
            else
                res = new StringBuilder($"{student.Student.User.Name}(@{student.Student.User.Username}):\n");
            var credits = dataAccess.Credits
                .Include(x => x.Teacher)
                .Where(x => x.UserId == student.Student.UserId && x.ClassRoomId == student.Student.User.ClassRoomActiveId)
                .OrderByDescending(x => x.DateTime.Date)
                .ToList()
                .GroupBy(x => x.DateTime.Date);

            foreach (var item in credits)
            {
                if (item.Count() == 1 && item.First().Value == 0)
                    continue;
                int i = 1;
                res.Append("\n");
                res.Append(item.Key.Day);
                res.Append("/");
                res.Append(item.Key.Month);
                res.Append("/");
                res.Append(item.Key.Year);
                res.Append(":\n");
                foreach (var credit in item)
                {
                    if (credit.Value != 0)
                    {
                        string text = "";
                        if (dataAccess.ClassInterventions.Where(x => x.Id == credit.ObjectId).FirstOrDefault() != null)
                        {
                            text = dataAccess.ClassInterventions.Where(x => x.Id == credit.ObjectId).First().Text;
                        }
                        else if (dataAccess.ClassTitles.Where(x => x.Id == credit.ObjectId).FirstOrDefault() != null)
                        {
                            text = dataAccess.ClassTitles.Where(x => x.Id == credit.ObjectId).First().Title;
                        }
                        else if (dataAccess.Dailies.Where(x => x.Id == credit.ObjectId).FirstOrDefault() != null)
                        {
                            text = dataAccess.Dailies.Where(x => x.Id == credit.ObjectId).First().Text;
                        }
                        else if (dataAccess.Jokes.Where(x => x.Id == credit.ObjectId).FirstOrDefault() != null)
                        {
                            text = dataAccess.Jokes.Where(x => x.Id == credit.ObjectId).First().Text;
                        }
                        else if (dataAccess.RectificationToTheTeachers.Where(x => x.Id == credit.ObjectId).FirstOrDefault() != null)
                        {
                            text = dataAccess.RectificationToTheTeachers.Where(x => x.Id == credit.ObjectId).First().Text;
                        }
                        else if (dataAccess.StatusPhrases.Where(x => x.Id == credit.ObjectId).FirstOrDefault() != null)
                        {
                            text = dataAccess.StatusPhrases.Where(x => x.Id == credit.ObjectId).First().Phrase;
                        }
                        else if (dataAccess.Miscellaneous.Where(x => x.Id == credit.ObjectId).FirstOrDefault() != null)
                        {
                            text = dataAccess.Miscellaneous.Where(x => x.Id == credit.ObjectId).First().Text;
                        }
                        else if (dataAccess.Memes.Where(x => x.Id == credit.ObjectId).FirstOrDefault() != null)
                        {
                            text = "Meme";
                        }
                        res.Append("    ");
                        res.Append(i);
                        res.Append(": ");
                        res.Append(credit.Value);
                        if (!string.IsNullOrEmpty(text))
                        {
                            res.Append(" -> ");
                            res.Append(text);
                        }
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
            }
            if (credits.Count() == 0)
                res.Append("No tiene créditos registrados todavía.");
            return res.ToString();
        }

        public async Task CreditByTeacher(User user)
        {
            user.Status = UserStatus.Credits;
            dataAccess.Users.Update(user);
            await dataAccess.SaveChangesAsync();
        }

        public async Task AssignCredit(User user)
        {
            user.Status = UserStatus.AssignCreditsStudent;
            dataAccess.Users.Update(user);
            await dataAccess.SaveChangesAsync();
        }

        public async Task<bool> AssignCredit(User user, string username)
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
                await dataAccess.SaveChangesAsync();
                return false;
            }

            var random = new Random();
            var misc = new Miscellaneous
            {
                ClassRoomId = user.ClassRoomActiveId,
                DateTime = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                UserId = student.UserId,
                Text = ""
            };
            var credit = new Credits
            {
                ClassRoomId = user.ClassRoomActiveId,
                DateTime = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                TeacherId = user.Id,
                UserId = student.UserId,
                Value = 0,
                Text = String.Empty,
                ObjectId = misc.Id,
                Code = random.Next(10000, 99999)
            };
            dataAccess.Miscellaneous.Add(misc);
            dataAccess.Credits.Add(credit);
            await dataAccess.SaveChangesAsync();
            return true;
        }

        public async Task<(string, long, long)> AssignCreditMessage(User user, string message)
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
            await dataAccess.SaveChangesAsync();

            return (text, credit.User.ChatId, credit.Value);
        }

        public async Task<string> GetCreditsById(long id)
        {
            var student = dataAccess.Students.Where(x => x.UserId == id).First();
            var classRoom = dataAccess.ClassRooms.Where(x => x.Id == student.User.ClassRoomActiveId).First();
            var credits = dataAccess.Credits.Where(x => x.UserId == student.UserId && x.ClassRoomId == student.User.ClassRoomActiveId).Include(x => x.ClassRoom).ToList();

            var user = dataAccess.Users.Where(x => x.Id == id).First();
            user.Status = UserStatus.Ready;
            dataAccess.Update(user);
            await dataAccess.SaveChangesAsync();
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

        public async Task AddCredits(long value, long teacherId, string objectId, long userId, long classRoomId, string recomendation)
        {
            var random = new Random();
            var credit = new Credits
            {
                DateTime = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                Value = value,
                UserId = userId,
                ObjectId = objectId,
                ClassRoomId = classRoomId,
                Text = recomendation,
                TeacherId = teacherId,
                Code = random.Next(10000, 99999)
            };
            dataAccess.Credits.Add(credit);
            await dataAccess.SaveChangesAsync();
        }

        public string GetCreditListOfUser(User user)
        {
            var credits = dataAccess.Credits
                .Where(x => x.ClassRoomId == user.ClassRoomActiveId)
                .OrderByDescending(x => x.DateTime.Date)
                .ToList()
                .GroupBy(x => x.UserId);

            var tuple = new List<(long, long)>();
            foreach (var credit in credits)
            {
                tuple.Add((credit.Key, credit.Sum(x => x.Value)));
            }

            tuple = tuple.OrderByDescending(x => x.Item2).ToList();
            var me = tuple.Where(x => x.Item1 == user.Id).First();
            var indexMe = tuple.IndexOf(me);
            var res = new StringBuilder();
            if(indexMe - 1 >= 0)
            {
                (long, long) previous = tuple[indexMe - 1];
                res.Append(indexMe);
                res.Append(": ");
                res.Append(previous.Item2);
                res.Append(" -> **********************\n");
            }
            res.Append(indexMe + 1);
            res.Append(": ");
            res.Append(me.Item2);
            res.Append(" -> ");
            res.Append(string.IsNullOrEmpty(user.Name) ? user.FirstName + " " + user.LastName : user.Name);
            res.Append("\n");
            if (indexMe + 1 < tuple.Count())
            {
                (long, long) next = tuple[indexMe + 1];
                res.Append(indexMe + 2);
                res.Append(": ");
                res.Append(next.Item2);
                res.Append(" -> **********************\n");
            }

            return res.ToString();
        }
    }
}