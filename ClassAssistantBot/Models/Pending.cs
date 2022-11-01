using System;
namespace ClassAssistantBot.Models
{
    public enum InteractionType
    {
        ClassTitle,
        Diary,
        Joke,
        Meme,
        RectificationToTheTeacher,
        StatusPhrase,
        ClassIntervention,
        None,
        Miscellaneous
    }

    public class Pending
    {
        public string Id { get; set; }

        public string StudentId { get; set; }

        public Student Student { get; set; }

        public long ClassRoomId { get; set; }

        public ClassRoom ClassRoom { get; set; }

        public InteractionType Type { get; set; }

        public string ObjectId { get; set; }

        public string Code { get; set; }

        public bool GiveMeAnExplication { get; set; }
    }
}

