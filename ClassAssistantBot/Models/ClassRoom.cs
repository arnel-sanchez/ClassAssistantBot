using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ClassAssistantBot.Models
{
    public class ClassRoom
    {
        [Required]
        public long Id { get; set; }

        public string Name { get; set; }

        public List<TeacherByClassRoom> TeachersByClassRooms { get; set; }

        public List<StudentByClassRoom> StudentsByClassRooms { get; set; }

        public int StudentAccessKey { get; set; }
        
        public int TeacherAccessKey { get; set; }

        public string MemeChannel { get; set; }
    }
}
