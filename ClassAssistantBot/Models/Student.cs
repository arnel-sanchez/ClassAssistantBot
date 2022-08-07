using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassAssistantBot.Models
{
    public class Student
    {
        public string Id { get; set; }

        public User User { get; set; }

        public long UserId { get; set; }

        public List<StudentByClassRoom> StudentsClassRooms { get; set; }
    }
}
