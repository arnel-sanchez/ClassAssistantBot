using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassAssistantBot.Models
{
    public class RectificationToTheTeacher
    {
        public string Id { get; set; }

        public string Text { get; set; }

        public DateTime DateTime { get; set; }

        public long UserId { get; set; }

        public User User { get; set; }

        public Teacher Teacher { get; set; }

        public string TeacherId { get; set; }
    }
}
