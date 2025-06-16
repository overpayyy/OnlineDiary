using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using OnlineDiary;
using OnlineDiary.Data;
using OnlineDiary.Models;

public partial class StudentMainWindow : Window
{
    private DateTime currentWeekStart;
    private int currentStudentId;
    private bool isScheduleView = true;

    public StudentMainWindow(int studentId)
    {
        InitializeComponent();
        currentStudentId = studentId;
        currentWeekStart = GetStartOfWeek(DateTime.Today);
        LoadStudentName();
        LoadSchedule(currentWeekStart);
    }

    private DateTime GetStartOfWeek(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    private void LoadStudentName()
    {
        using (var context = new AppDbContext())
        {
            var student = context.Users.FirstOrDefault(u => u.Id == currentStudentId);
            if (student != null)
            {
                StudentNameTextBlock.Text = $"{student.FirstName} {student.LastName}";
            }
        }
    }

    private void LoadSchedule(DateTime weekStart)
    {
        if (!isScheduleView) return;

        ScheduleCards.Children.Clear();

        List<string> days = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };

        using (var context = new AppDbContext())
        {
            for (int i = 0; i < days.Count; i++)
            {
                DateTime date = weekStart.AddDays(i);
                var lessons = context.Lessons
                    .Where(l => l.Date.Date == date.Date)
                    .Include(l => l.Grades)
                    .ThenInclude(g => g.Student)
                    .ToList()
                    .OrderBy(l => DateTime.Parse(l.Time ?? "00:00"))
                    .ToList();

                var cardItems = new List<(string SubjectTime, string Homework, List<string> Grades)>();
                foreach (var lesson in lessons)
                {
                    var subject = context.Subjects.FirstOrDefault(s => s.Id == lesson.SubjectId)?.Name ?? "Unknown";
                    var homework = lesson.Homework ?? "No homework";
                    var grades = lesson.Grades
                        .Where(g => g.StudentId == currentStudentId)
                        .Select(g => $"Grade: {g.Value}" + (string.IsNullOrEmpty(g.Description) ? "" : $" ({g.Description})"))
                        .ToList() ?? new List<string>();

                    string subjectTime = $"{subject} at {lesson.Time ?? "N/A"}";
                    cardItems.Add((subjectTime, homework, grades));
                }

                if (!cardItems.Any())
                {
                    cardItems.Add(("No entries", "No homework", new List<string>()));
                }

                string title = $"{days[i]} ({date:dd.MM.yyyy})";
                var card = CreateDayCard(title, cardItems);
                ScheduleCards.Children.Add(card);
            }
        }
    }

    private void LoadGrades(DateTime weekStart)
    {
        if (isScheduleView) return;

        var grid = new DataGrid
        {
            Margin = new Thickness(10),
            AutoGenerateColumns = false,
            IsReadOnly = true
        };

        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Subject",
            Binding = new System.Windows.Data.Binding("SubjectName"),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });

        List<DateTime> dates = new List<DateTime>();
        for (int i = 0; i < 5; i++)
        {
            dates.Add(weekStart.AddDays(i).Date);
        }
        string dateHeaders = string.Join(", ", dates.Select(d => d.ToString("dd.MM.yyyy")));
        Console.WriteLine($"Dates for week: {dateHeaders}");

        using (var context = new AppDbContext())
        {
            try
            {
                var lessonsWithGrades = context.Lessons
                    .Include(l => l.Subject)
                    .Include(l => l.Grades)
                    .ThenInclude(g => g.Student)
                    .Where(l => dates.Contains(l.Date.Date))
                    .Select(l => new
                    {
                        l.Subject.Name,
                        l.Date,
                        Grades = l.Grades.Where(g => g.StudentId == currentStudentId)
                                       .Select(g => new { g.Value, LessonDate = g.Lesson.Date.Date })
                                       .ToList()
                    })
                    .ToList();

                Console.WriteLine($"Found {lessonsWithGrades.Count} lessons with grades for student ID {currentStudentId}:");
                if (lessonsWithGrades.Count == 0)
                {
                    Console.WriteLine("No lessons found for the given dates.");
                }
                else
                {
                    foreach (var lesson in lessonsWithGrades)
                    {
                        Console.WriteLine($"Subject: {lesson.Name}, Date: {lesson.Date:dd.MM.yyyy}, Grades: {string.Join(", ", lesson.Grades.Select(g => g.Value ?? "null"))}");
                    }
                }

                foreach (var date in dates)
                {
                    var dateStr = date.ToString("dd.MM.yyyy");
                    var column = new DataGridTextColumn
                    {
                        Header = dateStr,
                        Binding = new System.Windows.Data.Binding($"Grades[{dateStr}]") { Converter = new GradeConverter() },
                        Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                    };
                    grid.Columns.Add(column);
                }

                var subjects = lessonsWithGrades.Select(l => l.Name).Distinct().ToList();
                var gradeData = subjects.Select(subject => new
                {
                    SubjectName = subject,
                    Grades = lessonsWithGrades
                        .Where(l => l.Name == subject)
                        .SelectMany(l => l.Grades)
                        .GroupBy(g => g.LessonDate.ToString("dd.MM.yyyy"))
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(x => x.Value).FirstOrDefault() ?? "-"
                        )
                }).ToList();

                Console.WriteLine("GradeData:");
                foreach (var item in gradeData)
                {
                    Console.WriteLine($"Subject: {item.SubjectName}, Grades: {string.Join(", ", item.Grades.Select(kv => $"{kv.Key}: {kv.Value}"))}");
                }

                grid.ItemsSource = gradeData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LoadGrades: {ex.Message}");
            }
        }

        MainContent.Content = grid;
        Console.WriteLine("LoadGrades completed.");
    }

    private Border CreateDayCard(string title, List<(string SubjectTime, string Homework, List<string> Grades)> items)
    {
        var card = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.LightGray,
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(10),
            Width = 220,
            Background = Brushes.White,
            Padding = new Thickness(10),
        };

        var stack = new StackPanel();

        var dayTitle = new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 2, 0, 10)
        };
        stack.Children.Add(dayTitle);

        if (items.Count == 0 || items.All(i => i.SubjectTime == "No entries"))
        {
            stack.Children.Add(new TextBlock { Text = "No entries", Foreground = Brushes.Gray });
        }
        else
        {
            foreach (var item in items)
            {
                var subjectText = new TextBlock
                {
                    Text = "• " + item.SubjectTime,
                    FontSize = 13,
                    Margin = new Thickness(0, 2, 0, 2)
                };
                stack.Children.Add(subjectText);

                if (!string.IsNullOrEmpty(item.Homework) && item.Homework != "No homework")
                {
                    var homeworkText = new TextBlock
                    {
                        Text = "  - Homework: " + item.Homework,
                        FontSize = 12,
                        Margin = new Thickness(0, 2, 0, 2),
                        Foreground = Brushes.DarkGreen
                    };
                    stack.Children.Add(homeworkText);
                }

                if (item.Grades.Any())
                {
                    foreach (var grade in item.Grades)
                    {
                        var gradeText = new TextBlock
                        {
                            Text = $"  - {grade}",
                            FontSize = 12,
                            Margin = new Thickness(0, 2, 0, 2),
                            Foreground = Brushes.DarkBlue
                        };
                        stack.Children.Add(gradeText);
                    }
                }
            }
        }

        card.Child = stack;
        return card;
    }

    private void NextWeek_Click(object sender, RoutedEventArgs e)
    {
        currentWeekStart = currentWeekStart.AddDays(7);
        if (isScheduleView) LoadSchedule(currentWeekStart);
        else LoadGrades(currentWeekStart);
    }

    private void LastWeek_Click(object sender, RoutedEventArgs e)
    {
        currentWeekStart = currentWeekStart.AddDays(-7);
        if (isScheduleView) LoadSchedule(currentWeekStart);
        else LoadGrades(currentWeekStart);
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        var roleWindow = new RoleSelectionWindow();
        roleWindow.Show();
        this.Close();
    }

    private void ScheduleButton_Click(object sender, RoutedEventArgs e)
    {
        isScheduleView = true;
        MainContent.Content = ScheduleCards;
        LoadSchedule(currentWeekStart);
    }

    private void GradesButton_Click(object sender, RoutedEventArgs e)
    {
        isScheduleView = false;
        LoadGrades(currentWeekStart);
    }
}

public class GradeConverter : System.Windows.Data.IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is Dictionary<string, string> grades && parameter is string date)
        {
            return grades.ContainsKey(date) ? grades[date] : "-";
        }
        return "-";
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}