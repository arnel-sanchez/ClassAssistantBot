using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassAssistantBot.Models
{
    public class TeacherByClassRoom
    {
        public string Id { get; set; }

        public string TeacherId { get; set; }

        public Teacher Teacher { get; set; }

        public ClassRoom ClassRoom { get; set; }

        public long ClassRoomId { get; set; }
    }
}
