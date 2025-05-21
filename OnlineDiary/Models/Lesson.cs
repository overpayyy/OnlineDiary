using OnlineDiary.Models;

public class Lesson
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Time { get; set; }
    public string Description { get; set; }

    public int SubjectId { get; set; }
    public Subject Subject { get; set; }

    public int TeacherId { get; set; }
    public User Teacher { get; set; }

    public List<Grade> Grades { get; set; }
}