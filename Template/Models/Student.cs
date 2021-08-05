using System;
using Template.Services;

namespace Template.Models
{
    /// <summary>
    /// Course's student model
    /// </summary>
    class Student
    {
        private int _id;
        private string _firstName;
        private string _secondName;
        private string _thirdName;
        private ulong _discordId;
        private DateTime _createdAt;
        private DateTime _interviewStart;
        private DateTime _interviewEnd;

        public int Id { get => _id; }
        public string FirstName { get => _firstName; set => _firstName = value; }
        public string SecondName { get => _secondName; set => _secondName = value; }
        public string ThirdName { get => _thirdName; set => _thirdName = value; }
        public ulong DiscordId { get => _discordId; set => _discordId = value; }
        public DateTime CreatedAt { get => _createdAt; set => _createdAt = value; }
        public DateTime InterviewStart { get => _interviewStart; set => _interviewStart = value; }
        public DateTime InterviewEnd { get => _interviewEnd; set => _interviewEnd = value; }

        public Student(string firstName, string secondName, ulong discordId, DateTime interviewStart, DateTime interviewEnd)
        {
            FirstName = firstName;
            SecondName = secondName;
            DiscordId = discordId;
            InterviewStart = interviewStart;
            InterviewEnd = interviewEnd;
        }
        public Student(int id)
        {
            _id = id;
            //FirstName = DatabaseHandler.RunCommand("SELECT first_name FROM students;", "first_name").ToString()
        }
    }
}
