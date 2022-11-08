using System;
using System.Text;
using ClassAssistantBot.Models;
using Microsoft.EntityFrameworkCore;

namespace ClassAssistantBot.Services
{
    public class ClassRoomDataHandler
    {
        private DataAccess dataAccess { get; set; }

        public ClassRoomDataHandler(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public void CreateClassRoom(User user)
        {
            user.Status = UserStatus.TeacherCreatingClass;
            dataAccess.Users.Update(user);
             dataAccess.SaveChanges();
        }

        public string CreateClassRoom(long id, string name)
        {
            var teacher =  dataAccess.Teachers.Where(x => x.User.TelegramId == id).FirstOrDefault();

            var random = new Random();
            var studentAccessKey = random.Next(1000000, 9999999);
            while (dataAccess.ClassRooms.Where(x => x.StudentAccessKey == studentAccessKey).FirstOrDefault() != null)
            {
                studentAccessKey = random.Next(1000000, 9999999);
            }

            var teacherAccessKey = random.Next(1000000, 9999999);
            while (dataAccess.ClassRooms.Where(x => x.TeacherAccessKey == teacherAccessKey).FirstOrDefault() != null)
            {
                teacherAccessKey = random.Next(1000000, 9999999);
            }

            var classRoom = new ClassRoom
            {
                Name = name,
                StudentsByClassRooms = new List<StudentByClassRoom>(),
                TeachersByClassRooms = new List<TeacherByClassRoom>(),
                StudentAccessKey = studentAccessKey,
                TeacherAccessKey = teacherAccessKey,
                MemeChannel = "",
                ClassInterventionChannel = "",
                ClassTitleChannel = "",
                DiaryChannel = "",
                JokesChannel = "",
                MiscelaneousChannel = "",
                RectificationToTheTeacherChannel = "",
                StatusPhraseChannel = ""
            };

             dataAccess.Add(classRoom);
             dataAccess.SaveChanges();

            classRoom =  dataAccess.ClassRooms.Where(x => x.TeacherAccessKey == teacherAccessKey && x.StudentAccessKey == studentAccessKey).First();

            var user =  dataAccess.Users.Where(x => x.TelegramId == id).FirstOrDefault();

            if (teacher == null)
            {
                teacher = new Teacher
                {
                    UserId = user.Id,
                    TeachersByClassRooms = new List<TeacherByClassRoom>(),
                    Id = Guid.NewGuid().ToString()
                };
            }

            var teacherByClassRoom = new TeacherByClassRoom
            {
                ClassRoomId = classRoom.Id,
                Id = Guid.NewGuid().ToString(),
                Teacher = teacher
            };

            user.IsTecaher = true;
            user.Status = UserStatus.Ready;
            user.ClassRoomActiveId = classRoom.Id;

            dataAccess.Users.Update(user);

             dataAccess.Add(teacherByClassRoom);
             dataAccess.SaveChanges();

            return $"El código de acceso para sus estudiantes {classRoom.StudentAccessKey}\n\nEl código de acceso para los demás profesores es {classRoom.TeacherAccessKey}";
        }

        public string ChangeClassRoom(long id)
        {
            var user =  dataAccess.Users.Where(x => x.Id == id).First();
            user.Status = UserStatus.ChangeClassRoom;
            dataAccess.Update(user);
             dataAccess.SaveChanges();
            var list =  dataAccess.TeachersByClassRooms.Where(x => x.Teacher.UserId == id).Include(x => x.ClassRoom).ToList();
            var res = new StringBuilder();
            foreach (var item in list)
            {
                res.Append(item.ClassRoom.Name);
                res.Append(": ");
                res.Append(item.ClassRoom.Id);
                res.Append("\n");
            }
            return res.ToString();
        }

        public void ChangeClassRoom(long id, long classRoomID)
        {
            var user =  dataAccess.Users.Where(x => x.Id == id).First();
            var classRoom =  dataAccess.ClassRooms.Where(x => x.Id == classRoomID).FirstOrDefault();
            if (classRoom != null)
            {
                user.ClassRoomActiveId = classRoom.Id;
                user.Status = UserStatus.Ready;
                dataAccess.Users.Update(user);
                 dataAccess.SaveChanges();
            }
        }

        public string SeeClassRoomActive(User user)
        {
            var res =  dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).First();
            return res.Name;
        }

        public string GetClassesCreated(User user)
        {
            var classes =  dataAccess.Classes.Where(x => x.ClassRoomId == user.ClassRoomActiveId).ToList();

            StringBuilder stringBuilder = new StringBuilder();

            int k = 1;
            foreach (var @class in classes)
            {
                stringBuilder.Append($"{k}: {@class.Title}\n");
                k++;
            }

            return stringBuilder.ToString();
        }

        public void AssignMemeChannel(User user)
        {
            user.Status = UserStatus.AssignMemeChannel;
            dataAccess.Users.Update(user);
             dataAccess.SaveChanges();
        }

        public void AssignMemeChannel(User user, string chanelName)
        {
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            var classRoom =  dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).First();
            classRoom.MemeChannel = chanelName;
            dataAccess.ClassRooms.Update(classRoom);
             dataAccess.SaveChanges();
        }

        public string GetMemeChannel(User user)
        {
            var res =  dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).First();
            return res.MemeChannel;
        }

        public void AssignJokesChannel(User user)
        {
            user.Status = UserStatus.AssignJokeChannel;
            dataAccess.Users.Update(user);
             dataAccess.SaveChanges();
        }

        public void AssignJokesChannel(User user, string chanelName)
        {
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            var classRoom =  dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).First();
            classRoom.JokesChannel = chanelName;
            dataAccess.ClassRooms.Update(classRoom);
             dataAccess.SaveChanges();
        }

        public string GetJokesChannel(User user)
        {
            var res =  dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).First();

            return res.JokesChannel;
        }

        public void AssignClassInterventionChannel(User user)
        {
            user.Status = UserStatus.AssignClassInterventionChannel;
            dataAccess.Users.Update(user);
             dataAccess.SaveChanges();
        }

        public void AssignClassInterventionChannel(User user, string chanelName)
        {
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            var classRoom =  dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).First();
            classRoom.ClassInterventionChannel = chanelName;
            dataAccess.ClassRooms.Update(classRoom);
             dataAccess.SaveChanges();
        }

        public string GetClassInterventionChannel(User user)
        {
            var res =  dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).First();

            return res.ClassInterventionChannel;
        }

        public void AssignDiaryChannel(User user)
        {
            user.Status = UserStatus.AssignDiaryChannel;
            dataAccess.Users.Update(user);
             dataAccess.SaveChanges();
        }

        public void AssignDiaryChannel(User user, string chanelName)
        {
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            var classRoom =  dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).First();
            classRoom.DiaryChannel = chanelName;
            dataAccess.ClassRooms.Update(classRoom);
             dataAccess.SaveChanges();
        }

        public string GetDiaryChannel(User user)
        {
            var res =  dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).First();

            return res.DiaryChannel;
        }

        public void AssignStatusPhraseChannel(User user)
        {
            user.Status = UserStatus.AssignStatusPhraseChannel;
            dataAccess.Users.Update(user);
             dataAccess.SaveChanges();
        }

        public void AssignStatusPhraseChannel(User user, string chanelName)
        {
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            var classRoom =  dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).First();
            classRoom.StatusPhraseChannel = chanelName;
            dataAccess.ClassRooms.Update(classRoom);
             dataAccess.SaveChanges();
        }

        public string GetStatusPhraseChannel(User user)
        {
            var res =  dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).First();

            return res.StatusPhraseChannel;
        }

        public void AssignClassTitleChannel(User user)
        {
            user.Status = UserStatus.AssignClassTitleChannel;
            dataAccess.Users.Update(user);
             dataAccess.SaveChanges();
        }

        public void AssignClassTitleChannel(User user, string chanelName)
        {
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            var classRoom =  dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).First();
            classRoom.ClassTitleChannel = chanelName;
            dataAccess.ClassRooms.Update(classRoom);
             dataAccess.SaveChanges();
        }

        public string GetClassTitleChannel(User user)
        {
            var res =  dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).First();

            return res.ClassTitleChannel;
        }

        public void AssignRectificationToTheTeacherChannel(User user)
        {
            user.Status = UserStatus.AssignRectificationToTheTeacherChannel;
            dataAccess.Users.Update(user);
             dataAccess.SaveChanges();
        }

        public void AssignRectificationToTheTeacherChannel(User user, string chanelName)
        {
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            var classRoom =  dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).First();
            classRoom.RectificationToTheTeacherChannel = chanelName;
            dataAccess.ClassRooms.Update(classRoom);
             dataAccess.SaveChanges();
        }

        public string GetRectificationToTheTeacherChannel(User user)
        {
            var res =  dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).First();

            return res.RectificationToTheTeacherChannel;
        }

        public void SendInformationToTheStudents(User user)
        {
            user.Status = UserStatus.SendInformation;
            dataAccess.Users.Update(user);
             dataAccess.SaveChanges();
        }

        public List<StudentByClassRoom> GetStudentsOnClassRoom(User user)
        {
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
             dataAccess.SaveChanges();
            return  dataAccess.StudentsByClassRooms
                .Where(x => x.ClassRoomId == user.ClassRoomActiveId)
                .Include(x => x.Student)
                .Include(x => x.Student.User)
                .ToList();
        }

        public List<TeacherByClassRoom> GetTeachersOnClassRoom(User user)
        {
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
             dataAccess.SaveChanges();
            return  dataAccess.TeachersByClassRooms
                .Where(x => x.ClassRoomId == user.ClassRoomActiveId && x.Teacher.UserId != user.Id)
                .Include(x => x.Teacher)
                .Include(x => x.Teacher.User)
                .ToList();
        }

        public void AssignMiscellaneousChannel(User user)
        {
            user.Status = UserStatus.AssignMiscellaneousChannel;
            dataAccess.Users.Update(user);
             dataAccess.SaveChanges();
        }

        public void AssignMiscellaneousChannel(User user, string chanelName)
        {
            user.Status = UserStatus.Ready;
            dataAccess.Users.Update(user);
            var classRoom =  dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).First();
            classRoom.MiscelaneousChannel = chanelName;
            dataAccess.ClassRooms.Update(classRoom);
             dataAccess.SaveChanges();
        }

        public string GetMiscellaneousChannel(User user)
        {
            var res =  dataAccess.ClassRooms.Where(x => x.Id == user.ClassRoomActiveId).First();

            return res.MiscelaneousChannel;
        }
    }
}

