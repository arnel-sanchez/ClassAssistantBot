using ClassAssistantBot.Models;
using Microsoft.EntityFrameworkCore;

namespace ClassAssistantBot.Services
{
    public class ClassInterventionDataHandler
    {
        private DataAccess dataAccess { get; set; }

        public ClassInterventionDataHandler(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public void CreateIntervention(User user)
        {
            user.Status = UserStatus.ClassIntervention;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
        }

        public void CreateIntervention(User user, long classId)
        {
            user.Status = UserStatus.ClassInterventionSelect;
            dataAccess.Users.Update(user);
            var classIntervention = new ClassIntervention
            {
                Id = Guid.NewGuid().ToString(),
                Text = "",
                ClassId = classId,
                UserId = user.Id,
                DateTime = DateTime.UtcNow,
                Finished = false
            };
             dataAccess.ClassInterventions.Add(classIntervention);
             dataAccess.SaveChanges();
        }

        public void CreateIntervention(User user, string intervention)
        {
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            var classIntervention = dataAccess.ClassInterventions
                .Where(x => x.UserId == user.Id && !x.Finished)
                .First();
            classIntervention.Finished = true;
            classIntervention.Text = intervention;
            dataAccess.ClassInterventions.Update(classIntervention);
            var random = new Random();
            string code = random.Next(100000, 999999).ToString();
            while(dataAccess.Pendings.Where(x => x.Code == code).Count()!=0)
                code = random.Next(100000, 999999).ToString();
            var pending = new Pending
            {
                Id = Guid.NewGuid().ToString(),
                Type = InteractionType.ClassIntervention,
                ClassRoomId = user.ClassRoomActiveId,
                ObjectId = classIntervention.Id,
                StudentId = dataAccess.Students.Where(x => x.UserId == user.Id).First().Id,
                Code = code
            };
             dataAccess.Pendings.Add(pending);
             dataAccess.SaveChanges();
        }

        public ClassIntervention GetClassIntenvention(string id)
        {
            return dataAccess.ClassInterventions.Where(x => x.Id == id).First();
        }
    }
}

