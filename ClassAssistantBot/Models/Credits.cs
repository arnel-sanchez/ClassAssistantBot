using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassAssistantBot.Models
{
    public class Credits
    {
        public string Id { get; set; }

        public DateTime DateTime { get; set; }

        public long Value { get; set; }

        public string Text { get; set; }

        public long UserId { get; set; }

        public User User { get; set; }

        public User Teacher { get; set; }

        public long TeacherId { get; set; }

        public long ClassRoomId { get; set; }

        public ClassRoom ClassRoom { get; set; }

        public long Code { get; set; }

        public string ObjectId { get; set; }
    }
}
