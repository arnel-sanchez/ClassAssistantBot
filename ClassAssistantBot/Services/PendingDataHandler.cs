using ClassAssistantBot.Models;

namespace ClassAssistantBot.Services
{
    public class PendingDataHandler
    {
        private DataAccess dataAccess { get; set; }

        public PendingDataHandler(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public string GetPendings(User user)
        {
            var pendings = dataAccess.Pendings.Where(x => x.ClassRoomId == user.ClassRoomActiveId).ToList();

            var res = "";

            foreach (var item in pendings)
            {
                //pendiente a analizar la estructura
            }

            return res;
        }
    }
}

