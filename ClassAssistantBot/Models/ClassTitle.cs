using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassAssistantBot.Models
{
    public class ClassTitle
    {
        public string Id { get; set; }

        public long ClassId { get; set; }

        public Class Class { get; set; }

        public string Title { get; set; }

        public DateTime DateTime { get; set; }

        public long UserId { get; set; }

        public User User { get; set; }
    }
}
