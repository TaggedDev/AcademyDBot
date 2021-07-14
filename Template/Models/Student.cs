using System;

namespace Template.Models
{
    class Student
    {
        private string _firstName;
        private string _secondName;
        private ulong _discordId;
        private DateTime _interviewStart;
        private DateTime _interviewEnd;

        public string FirstName { get => _firstName; set => _firstName = value; }
        public string SecondName { get => _secondName; set => _secondName = value; }
        public ulong DiscordId { get => _discordId; set => _discordId = value; }
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
    }
}
