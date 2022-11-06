using System;
namespace ClassAssistantBot.Models
{
    public class Guild
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public List<Student> Students { get; set; }

        public ClassRoom ClassRoom { get; set; }

        public long ClassRoomId { get; set; }
    }
}

