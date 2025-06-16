namespace OnlineDiary.Models
{
    public class Grade
    {
        public int Id { get; set; }
        public string Value { get; set; }

        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }

        public int StudentId { get; set; }
        public User Student { get; set; }

        public DateTime DateAdded { get; set; }
        public string Description { get; set; }
    }
}
