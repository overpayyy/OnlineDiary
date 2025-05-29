namespace OnlineDiary.Models
{
    public class Announcement
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }

        public int TeacherId { get; set; }
        public User Teacher { get; set; }
    }
}
