using System;
namespace ClassAssistantBot.Models
{
    public class PracticClassPending
    {
        public string Id { get; set; }

        public long UserId { get; set; }

        public long StudentId { get; set; }

        public string PracticClassId { get; set; }
    }
}

