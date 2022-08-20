using System;
namespace ClassAssistantBot.Models
{
    public class Class
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public ClassRoom ClassRoom { get; set; }

        public long ClassRoomId { get; set; }

        public DateTime Date { get; set; } 
    }
}

