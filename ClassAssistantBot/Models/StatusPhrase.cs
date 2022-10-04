using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassAssistantBot.Models
{
    public class StatusPhrase
    {
        public string Id { get; set; }

        public string Phrase { get; set; }

        public DateTime DateTime { get; set; }

        public long UserId { get; set; }

        public User User { get; set; }

        public ClassRoom ClassRoom { get; set; }

        public long ClassRoomId { get; set; }
    }
}
