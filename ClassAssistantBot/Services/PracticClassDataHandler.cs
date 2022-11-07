using System;
using System.Text;
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

        public async Task CreatePracticClass(User user)
        {
            user.Status = UserStatus.CreatePracticClass;
            dataAccess.Users.Update(user);
            await dataAccess.SaveChangesAsync();
        }

        public async Task<bool> CreatePracticClass(User user, string cpFormat)
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
                await dataAccess.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<PracticClass>> GetPracticClasses(User user)
        {
            return await dataAccess.PracticClasses.Where(x => x.ClassRoomId == user.ClassRoomActiveId).ToListAsync();
        }

        public async Task<List<PracticClass>> DeletePracticClasses(User user)
        {
            user.Status = UserStatus.DeletePracticalClass;
            dataAccess.Users.Update(user);
            await dataAccess.SaveChangesAsync();
            return await dataAccess.PracticClasses.Where(x => x.ClassRoomId == user.ClassRoomActiveId).ToListAsync();
        }

        public async Task DeletePracticClasses(User user, string code)
        {
            var practicalClass = await dataAccess.PracticClasses.Where(x => x.Code == code).FirstAsync();
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            dataAccess.PracticClasses.Remove(practicalClass);
            await dataAccess.SaveChangesAsync();
        }

        public async Task<List<Excercise>> GetExcercises(User user, string studentUserName, string practicaClassCode)
        {
            var excersices = await dataAccess.Excercises.Where(x => x.PracticClass.Code == practicaClassCode).ToListAsync();
            var credits = await dataAccess.Credits.Where(x => x.User.Username == studentUserName && x.ClassRoomId == user.ClassRoomActiveId).ToListAsync();

            var res = new List<Excercise>();

            foreach (var excercise in excersices)
            {
                if(credits.Where(x => x.ObjectId == excercise.Id).Count() == 0)
                {
                    res.Add(excercise);
                }
            }
            res = res.OrderBy(x => x.Code).ToList();
            return res;
        }

        public async Task<string> EditPracticalClassName(User user)
        {
            user.Status = UserStatus.EditPracticalClasss;
            dataAccess.Users.Update(user);

            StringBuilder res = new StringBuilder("Listado de Clases Prácticas:\n");

            var practicalClasses = dataAccess.PracticClasses.Where(x => x.ClassRoomId == user.ClassRoomActiveId).ToList();
            int k = 1;
            foreach (var practicaClass in practicalClasses)
            {
                res.Append(k);
                res.Append(":");
                res.Append(practicaClass.Name);
                res.Append(" (");
                res.Append(practicaClass.Code);
                res.Append(")\n");
            }

            res.Append("\n\n**Para editar el nombre de la Clase Práctica debe escribir el código entre paréntesis seguido de un espacio y luego el nuevo nombre**");
            await dataAccess.SaveChangesAsync();
            return res.ToString();
        }

        public async Task<string> EditPracticalClassName(User user, string text)
        {
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);

            var data = text.Split(" ");

            var code = data[0];

            var name = data[1];

            for (int i = 2; i < data.Length; i++)
            {
                name += " ";
                name += data[i];
            }

            var practicalClass = dataAccess.PracticClasses.Where(x => x.Code == code).FirstOrDefault();

            if (practicalClass == null)
                return "No existe una Clase Práctica con ese código.";

            practicalClass.Name = name;
            dataAccess.PracticClasses.Update(practicalClass);
            await dataAccess.SaveChangesAsync();
            return "Clase Práctica Editada Satisfactoriamente";
        }

        public async Task<(bool, string, string, string)> ReviewPrecticalClass(User user, string excerciseCode, string studentUserName, string practicalClassCode, bool @double)
        {
            var student = dataAccess.Students.Where(x => x.User.Username == studentUserName).Include(x => x.User).FirstOrDefault();
            if (student == null)
            {
                return (false, $"No seleccionó correctamente el estudiante.", "", "");
            }
            var practicalClass = dataAccess.PracticClasses.Where(x => x.Code == practicalClassCode).FirstOrDefault();
            if (practicalClass == null)
            {
                return (false, $"No seleccionó correctamente la Clase Práctica", "", "");
            }
            var excercise = dataAccess.Excercises.Where(x => x.PracticClassId == practicalClass.Id && x.Code == excerciseCode).First();
            if (excercise == null)
            {
                return (false, $"No seleccionó correctamente el ejercicio", "", "");
            }
            if (dataAccess.Credits.Where(x => x.ObjectId == excercise.Id && x.UserId == student.UserId).Count()!=0)
            {
                return (false, $"El ejercicio ya fue revisado", "", "");
            }
            try
            {
                var random = new Random();
                long code = random.Next(100000, 999999);
                while (dataAccess.Credits.Where(x => x.Code == code).Count() != 0)
                    code = random.Next(100000, 999999);
                var credit = new Credits
                {
                    ClassRoomId = user.ClassRoomActiveId,
                    Id = Guid.NewGuid().ToString(),
                    ObjectId = excercise.Id,
                    Value = (@double ? excercise.Value * 2 : excercise.Value),
                    UserId = student.UserId,
                    TeacherId = user.Id,
                    Text = $"Ejecicio {excercise.Code} de la Clase Práctica {practicalClass.Name}",
                    DateTime = DateTime.UtcNow,
                    Code = code
                };
                user.Status = UserStatus.Ready;
                dataAccess.Credits.Add(credit);
                dataAccess.Users.Update(user);
                await dataAccess.SaveChangesAsync();
                return (true, $"Le han revisado el ejercicio {excercise.Code} en la Clase Práctica {practicalClass.Name}.", practicalClass.Name, student.User.ChatId.ToString());
            }
            catch
            {
                return (false, $"No insertó correctamente la revisión.", "", "");
            }
        }
    }
}