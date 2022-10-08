using Microsoft.EntityFrameworkCore;
using ClassAssistantBot.Models;

namespace ClassAssistantBot.Services
{
    public class DataAccess : DbContext
    {
        private string ConnectionString { get; set; }

        public DataAccess()
        {
            ConnectionString = "Host=db;Port=5432;Username=postgres;Password=biUrwej1obwok;Database=ClassAssitantBot;";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(ConnectionString);
        }

        public DbSet<DirectPending> DirectPendings { get; set; }

        public DbSet<Class> Classes { get; set; }

        public DbSet<Pending> Pendings { get; set; }

        public DbSet<Student> Students { get; set; }

        public DbSet<ClassRoom> ClassRooms { get; set; }

        public DbSet<ClassAssistantBot.Models.User> Users { get; set; }

        public DbSet<Teacher> Teachers { get; set; }

        public DbSet<ClassIntervention> ClassInterventions { get; set; }

        public DbSet<ClassTitle> ClassTitles { get; set; }

        public DbSet<Credits> Credits { get; set; }

        public DbSet<Daily> Dailies { get; set; }

        public DbSet<Joke> Jokes { get; set; }

        public DbSet<Meme> Memes { get; set; }

        public DbSet<RectificationToTheTeacher> RectificationToTheTeachers { get; set; }

        public DbSet<StatusPhrase> StatusPhrases { get; set; }

        public DbSet<TeacherByClassRoom> TeachersByClassRooms { get; set; }

        public DbSet<StudentByClassRoom> StudentsByClassRooms { get; set; }

        public DbSet<Miscellaneous> Miscellaneous { get; set; }

        public DbSet<PracticClass> PracticClasses { get; set; }

        public DbSet<Excercise> Excercises { get; set; }

        public DbSet<PracticClassPending> PracticClassPendings { get; set; }
    }
}
