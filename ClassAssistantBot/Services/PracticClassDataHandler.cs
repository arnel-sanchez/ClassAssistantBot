using System;
using ClassAssistantBot.Models;
using Microsoft.EntityFrameworkCore;

namespace ClassAssistantBot.Services
{
    public class PracticClassDataHandler
    {
        private DataAccess dataAccess { get; set; }

        public PracticClassDataHandler(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public void CreatePracticClass(User user)
        {
            user.Status = UserStatus.CreatePracticClass;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
        }

        public bool CreatePracticClass(User user, string cpFormat)
        {
            try
            {
                var data = cpFormat.Split(" ");
                if (data.Length % 2 == 0)
                {
                    return false;
                }
                var cpName = data[0];
                var random = new Random();
                string code = random.Next(100000, 999999).ToString();
                while (dataAccess.PracticClasses.Where(x => x.Code == code).Count() != 0)
                    code = random.Next(100000, 999999).ToString();
                var cp = new PracticClass
                {
                    ClassRoomId = user.ClassRoomActiveId,
                    Id = Guid.NewGuid().ToString(),
                    Name = cpName,
                    Code = code
                };
                var exercises = new List<Excercise>();

                for (int i = 1; i < data.Length; i += 2)
                {
                    exercises.Add(new Excercise
                    {
                        Id = Guid.NewGuid().ToString(),
                        Code = data[i],
                        PracticClassId = cp.Id,
                        Value = long.Parse(data[i + 1])
                    });
                }
                cp.Excercises = exercises;
                dataAccess.PracticClasses.Add(cp);
                dataAccess.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<PracticClass> ReviewPracticClass(User user)
        {
            user.Status = UserStatus.ReviewPracticClassSelect;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
            return dataAccess.PracticClasses
                .Where(x => x.ClassRoomId == user.ClassRoomActiveId)
                .ToList();
        }

        public List<Student> ReviewPracticClassSelect(User user)
        {
            user.Status = UserStatus.ReviewPracticClassSelectStudent;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
            return dataAccess.Students
                .Include(x => x.User)
                .Where(x => x.User.ClassRoomActiveId == user.ClassRoomActiveId)
                .ToList();
        }

        public void ReviewPracticClassPending(User user, long userId, string practicClassId)
        {
            user.Status = UserStatus.ReviewPracticClass;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();

            dataAccess.PracticClassPendings.Add(new PracticClassPending
            {
                Id = Guid.NewGuid().ToString(),
                PracticClassId = practicClassId,
                StudentId = userId,
                UserId = user.Id
            });
            dataAccess.SaveChanges();
        }

        public (bool, string, string, string) ReviewPrecticalClass(User user, string format)
        {
            var data = format.Split(" ");
            var practicClassPending = dataAccess.PracticClassPendings.Where(x => x.UserId == user.Id).FirstOrDefault();
            if (practicClassPending == null)
            {
                return (false, $"No existe la configuración para la revisión de la clase práctica, cometió un error en la selección anterior.", "", "");
            }
            var student = dataAccess.Students.Where(x => x.UserId == practicClassPending.StudentId).Include(x => x.User).FirstOrDefault();
            if(student == null)
            {
                return (false, $"No seleccionó correctamente el estudiante.", "", "");
            }
            var practicalClass = dataAccess.PracticClasses.Where(x => x.Id == practicClassPending.PracticClassId).FirstOrDefault();
            if (practicalClass == null)
            {
                return (false, $"No seleccionó correctamente la Clase Práctica", "", "");
            }
            var credits = new List<Credits>();
            try
            {
                bool @double = false;
                bool.TryParse(data[data.Length - 1], out @double);
                for (int i = 0; i < data.Length-1; i++)
                {
                    var excercise = dataAccess.Excercises.Where(x => x.PracticClassId == practicalClass.Id && x.Code == data[i]).First();
                    var random = new Random();
                    if(dataAccess.Credits.Where(x => x.ObjectId == excercise.Id).Count() == 0)
                        credits.Add(new Credits {
                            ClassRoomId = user.ClassRoomActiveId,
                            Id = Guid.NewGuid().ToString(),
                            ObjectId = excercise.Id,
                            Value = ( @double ? excercise.Value*2 : excercise.Value),
                            UserId = student.UserId,
                            TeacherId = user.Id,
                            Text = $"Ejecicio {excercise.Code} de la Clase Práctica {practicalClass.Name}",
                            DateTime = DateTime.UtcNow,
                            Code = random.Next(10000, 99999)
                        });
                }
                if(credits.Count()==0)
                {
                    return (false, $"Los ejercicios revisado ya recibieron sus créditos.", "", "");
                }
                dataAccess.Credits.AddRange(credits);
                dataAccess.PracticClassPendings.Remove(practicClassPending);
                user.Status = UserStatus.Ready;
                dataAccess.Users.Update(user);
                dataAccess.SaveChanges();
                return (true, "Se insertó la clase práctica correctamente.", practicalClass.Name, student.User.ChatId.ToString());
            }
            catch
            {
                return (false, $"No insertó correctamente la revisión.", "", "");
            }
        }
    }
}