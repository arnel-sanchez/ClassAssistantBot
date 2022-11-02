using System;
using ClassAssistantBot.Models;

namespace ClassAssistantBot.Services
{
    public class JokeDataHandler
    {
        private DataAccess dataAccess { get; set; }

        public JokeDataHandler(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public async Task DoJoke(User user)
        {
            user.Status = UserStatus.Joke;
            dataAccess.Users.Update(user);
            await dataAccess.SaveChangesAsync();
        }

        public async Task DoJoke(long id, string message)
        {
            var user = dataAccess.Users.First(x => x.Id == id);
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            var joke = new Joke
            {
                DateTime = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                Text = message,
                UserId = user.Id,
                ClassRoomId = user.ClassRoomActiveId,
            };
            dataAccess.Jokes.Add(joke);
            var random = new Random();
            string code = random.Next(100000, 999999).ToString();
            while (dataAccess.Pendings.Where(x => x.Code == code).Count() != 0)
                code = random.Next(100000, 999999).ToString();
            var pending = new Pending
            {
                Id = Guid.NewGuid().ToString(),
                Type = InteractionType.Joke,
                ClassRoomId = user.ClassRoomActiveId,
                ObjectId = joke.Id,
                StudentId = dataAccess.Students.Where(x => x.UserId == id).First().Id,
                Code = code
            };
            dataAccess.Pendings.Add(pending);
            await dataAccess.SaveChangesAsync();
        }

        public Joke GetJoke(string id)
        {
            return dataAccess.Jokes.First(x => x.Id == id);
        }
    }
}

