using System;

namespace OnlineDiary.Models
{
    public class ScheduleEntry
    {
        public int Id { get; set; }

        public string DayOfWeek { get; set; }

        public int LessonNumber { get; set; }

        public int TeacherId { get; set; }
        public User Teacher { get; set; }

        public int StudentId { get; set; }
        public User Student { get; set; }

        public int SubjectId { get; set; }
        public Subject Subject { get; set; }

        public string? Homework { get; set; }

        public string? Note { get; set; }
    }
}
