using System.ComponentModel.DataAnnotations;

namespace ClassAssistantBot.Models
{
    public enum UserStatus
    {
        None,
        Created,
        Verified,
        CreatingTecaher,
        TeacherCreatingClass,
        StudentEnteringClass,
        TeacherEnteringClass,
        Ready,
        Credits,
        ClassIntervention,
        ClassInterventionSelect,
        RectificationAtTeacher,
        RectificationToTheTeacherUserName,
        Diary,
        StatusPhrase,
        Meme,
        Joke,
        ClassTitle,
        ClassTitleSelect,
        CreateClass,
        ChangeClassRoom,
        RemoveStudentFromClassRoom,
        Pending,
        AssignCreditsStudent,
        AssignCredits,
        AssignMemeChannel,
        AssignJokeChannel,
        AssignClassInterventionChannel,
        AssignDiaryChannel,
        AssignStatusPhraseChannel,
        AssignClassTitleChannel,
        AssignRectificationToTheTeacherChannel,
        SendInformation,
        Miscellaneous,
        AssignMiscellaneousChannel,
        CreatePracticClass,
        ReviewPracticClass,
        EditPracticalClasss,
        CreateGuild,
        DeleteGuild,
        AssignStudentAtGuild,
        AssignCreditsAtGuild,
        DeleteStudentFromGuild,
        DetailsGuild
    }

    public class User : Telegram.BotAPI.AvailableTypes.ITelegramUser
    {
        [Required]
        [Key]
        public new long Id { get; set; }
        
        public long TelegramId { get; set; }

        public DateTime Created { get; set; }

        public long ChatId { get; set; }

        public Roles Role { get; set; }

        public UserStatus Status { get; set; }

        public bool IsBot { get; set; }

        public string Name { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
        
        public string Username { get; set; }
        
        public string LanguageCode { get; set; }

        public bool IsTecaher { get; set; }

        public long ClassRoomActiveId { get; set; }
    }
}
