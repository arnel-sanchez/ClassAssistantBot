using System;
namespace ClassAssistantBot.Models
{
    public class DirectPending
    {
        public string Id { get; set; }

        public long UserId { get; set; }

        public User User { get; set; }

        public string PendingId { get; set; }

        public Pending Pending { get; set; }
    }
}

