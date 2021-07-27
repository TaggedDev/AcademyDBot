using System;
using System.Collections.Generic;
using System.Text;

namespace Template.Models
{
    class Lesson
    {
        private byte _number;
        private string _topic;
        private DateTime _date;
        private string _homeworkDescription;

        public byte Number { get => _number; set => _number = value; }
        public string Topic { get => _topic; set => _topic = value; }
        public DateTime Date { get => _date; set => _date = value; }
        public string HomeworkDescription { get => _homeworkDescription; set => _homeworkDescription = value; }
    }
}
