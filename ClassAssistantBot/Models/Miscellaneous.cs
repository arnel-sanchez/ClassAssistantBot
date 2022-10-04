using System;
namespace ClassAssistantBot.Models
{
    public class Miscellaneous
    {
        public string Id { get; set; }

        public string Text { get; set; }

        public DateTime DateTime { get; set; }

        public long UserId { get; set; }

        public User User { get; set; }

        public ClassRoom ClassRoom { get; set; }

        public long ClassRoomId { get; set; }
    }
}

