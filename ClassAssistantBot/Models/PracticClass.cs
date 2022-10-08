using System;
namespace ClassAssistantBot.Models
{
    public class PracticClass
    {
        public string Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public List<Excercise> Excercises { get; set; }

        public long ClassRoomId { get; set; }

        public ClassRoom ClassRoom { get; set; }
    }
}

