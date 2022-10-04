using Telegram.BotAPI;
using Telegram.BotAPI.AvailableTypes;

namespace ClassAssistantBot.Models
{
    public class Meme
    {
        public string Id { get; set; }

        public DateTime DateTime { get; set; }

        public long UserId { get; set; }

        public User User { get; set; }

        public string FileId { get; set; }

        public string FieldUniqueId { get; set; }

        public ulong Width { get; set; }

        public ulong Height { get; set; }

        public string FileName { get; set; }

        public string MimeType { get; set; }

        public ulong FileSize { get; set; }

        public ClassRoom ClassRoom { get; set; }

        public long ClassRoomId { get; set; }
    }
}
