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

        public string JokesChannel { get; set; }

        public string RectificationToTheTeacherChannel { get; set; }

        public string ClassInterventionChannel { get; set; }

        public string ClassTitleChannel { get; set; }

        public string DiaryChannel { get; set; }

        public string StatusPhraseChannel { get; set; }

        public string MiscelaneousChannel { get; set; }
    }
}
