using System;
using System.Data.SqlClient;
using System.Text;
using Template.Services;

namespace Template.Models
{
    /// <summary>
    /// Course's student model
    /// </summary>
    class Student
    {
        private string _firstName;
        private string _secondName;
        private ulong _discordId;
        private DateTime _createdAt;
        private DateTime _interviewStart; // non field in db
        private DateTime _interviewEnd;  // non field in db

        public string FirstName { get => _firstName; set => _firstName = value; }
        public string SecondName { get => _secondName; set => _secondName = value; }
        public ulong DiscordId { get => _discordId; set => _discordId = value; }
        public DateTime CreatedAt { get => _createdAt; set => _createdAt = value; }
        public DateTime InterviewStart { get => _interviewStart; set => _interviewStart = value; }  // non field in db
        public DateTime InterviewEnd { get => _interviewEnd; set => _interviewEnd = value; }  // non field in db

        public Student(string firstName, string secondName, ulong discordId, DateTime interviewStart, DateTime interviewEnd)
        {
            FirstName = firstName;
            SecondName = secondName;
            DiscordId = discordId;
            InterviewStart = interviewStart;
            InterviewEnd = interviewEnd;
        }

        public Student(ulong discordId)
        {
            // Getting FIO fields.
            _discordId = discordId;
            FirstName = DatabaseHandler.RunCommand(BuildCommand("first_name"), "first_name").ToString();
            SecondName = DatabaseHandler.RunCommand(BuildCommand("second_name"), "second_name").ToString();
            DiscordId = ulong.Parse(DatabaseHandler.RunCommand(BuildCommand("discord_id"), "discord_id").ToString());

            // Getting created_at, interview_start, interview_end.
            string temp = DatabaseHandler.RunCommand(BuildCommand("created_at"), "created_at").ToString();
            if (!string.IsNullOrEmpty(temp))
                CreatedAt = DateTime.Parse(temp.ToString());

            string BuildCommand(string field)
            {
                StringBuilder cmd = new StringBuilder();
                cmd.Append("SELECT ");
                cmd.Append(field);
                cmd.Append(" FROM students WHERE student_id = ");
                cmd.Append(discordId.ToString());
                return cmd.ToString();
            }
        }

        public static void AddStudentToDB(ulong discord_id, string first_name, string second_name, DateTime created_at)
        {
            StringBuilder cmd = new StringBuilder();
            cmd.Append(String.Format("INSERT INTO students (discord_id, first_name, second_name, created_at) " +
                "VALUES ('{0}', N'{1}', N'{2}', '{3}')", discord_id.ToString(), first_name, second_name, created_at));

            DatabaseHandler.RunCommand(cmd.ToString());
        }
    }
}
