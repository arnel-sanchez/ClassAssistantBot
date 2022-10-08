using System;
namespace ClassAssistantBot.Models
{
    public class Excercise
    {
        public string Id { get; set; }

        public string Code { get; set; }

        public long Value { get; set; }

        public string PracticClassId { get; set; }

        public PracticClass PracticClass { get; set; }
    }
}

