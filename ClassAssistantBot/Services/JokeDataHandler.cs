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

        public void DoJoke(User user)
        {
            user.Status = UserStatus.Joke;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
            Console.WriteLine($"The student {user.Username} is redy to do a Joke");
        }

        public void DoJoke(long id, string message)
        {
            var user = dataAccess.Users.First(x => x.Id == id);
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            var joke = new Joke
            {
                DateTime = DateTime.Now,
                Id = Guid.NewGuid().ToString(),
                Text = message,
                UserId = user.Id
            };
            dataAccess.Jokes.Add(joke);
            dataAccess.SaveChanges();
            Console.WriteLine($"The student {user.Username} did a Joke");
        }
    }
}

