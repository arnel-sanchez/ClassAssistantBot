using System;
using ClassAssistantBot.Models;
using Microsoft.EntityFrameworkCore;

namespace ClassAssistantBot.Services
{
    public class DiaryDataHandler
    {
        private DataAccess dataAccess { get; set; }

        public DiaryDataHandler(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public void UpdateDiary(User user)
        {
            user.Status = UserStatus.Diary;
            dataAccess.Users.Update(user);
            dataAccess.SaveChanges();
            Console.WriteLine($"The student {user.Username} is ready to update Diary");
        }

        public void UpdateDiary(long id, string message)
        {
            var user = dataAccess.Users.First(x => x.Id == id);
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            var diary = new Diary
            {
                DateTime = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                Text = message,
                UserId = user.Id,
                ClassRoomId = user.ClassRoomActiveId
            };
            dataAccess.Dailies.Add(diary);

            var random = new Random();
            string code = random.Next(100000, 999999).ToString();
            while (dataAccess.Pendings.Where(x => x.Code == code).Count() != 0)
                code = random.Next(100000, 999999).ToString();
            var pending = new Pending
            {
                Id = Guid.NewGuid().ToString(),
                Type = InteractionType.Diary,
                ClassRoomId = user.ClassRoomActiveId,
                ObjectId = diary.Id,
                StudentId = dataAccess.Students.Where(x => x.UserId == user.Id).First().Id,
                Code = code
            };
            dataAccess.Pendings.Add(pending);

            dataAccess.SaveChanges();
        }

        public void AcceptDiary(User user, long studentId, string diaryId)
        {
            var dailies = dataAccess.Dailies.Where(x => x.UserId == studentId).OrderByDescending(x => x.DateTime).ToList();
            int count = 0;
            DateTime dateTime = DateTime.UtcNow;
            foreach (var item in dailies)
            {
                if (item.DateTime.Date == dateTime.Date)
                {
                    count++;
                    dateTime = dateTime.AddDays(-1);
                }
                else
                {
                    break;
                }
            }
            if (dailies.Where(x => x.DateTime == DateTime.UtcNow.Date).Count() == 1)
            {
                var credit = new Credits
                {
                    Id = Guid.NewGuid().ToString(),
                    DateTime = DateTime.UtcNow,
                    UserId = user.Id,
                    ClassRoomId = user.ClassRoomActiveId,
                    Value = count * 10000,
                    Text = $"Has recibido {count * 10000} créditos por haber actualizado tu diario {count} días seguidos.",
                    TeacherId = dataAccess.TeachersByClassRooms.Include(x => x.Teacher).Where(x => x.ClassRoomId == user.ClassRoomActiveId).First().Teacher.UserId,
                    ObjectId = diaryId
                };
                dataAccess.Credits.Add(credit);
            }

            var pending = dataAccess.Pendings.Where(x => x.ObjectId == diaryId).First();
            dataAccess.Remove(pending);
            dataAccess.SaveChanges();
        }

        public Diary GetDiary(string diaryId)
        {
            return dataAccess.Dailies.First(x => x.Id == diaryId);
        }
    }
}

