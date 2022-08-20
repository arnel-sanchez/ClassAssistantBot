using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassAssistantBot.Models
{
    public class ClassIntervention
    {
        public string Id { get; set; }

        public bool Finished { get; set; }

        public DateTime DateTime { get; set; }

        public string Text { get; set; }

        public long UserId { get; set; }

        public User User { get; set; }

        public Class Class { get; set; }

        public long ClassId { get; set; }
    }
}
