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

    private Border CreateDayCard(string title, List<(string SubjectTime, string Homework, List<string> Grades)> items)
    {
        var card = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.LightGray,
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(10),
            Width = 300,
            Height = 250,
            Background = Brushes.White,
            Padding = new Thickness(15),
        };

        var stack = new StackPanel();

        var dayTitle = new TextBlock
        {
            Text = title,
            FontSize = 18,
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
                    FontSize = 14,
                    Margin = new Thickness(0, 2, 0, 2)
                };
                stack.Children.Add(subjectText);

                if (!string.IsNullOrEmpty(item.Homework) && item.Homework != "No homework")
                {
                    var homeworkText = new TextBlock
                    {
                        Text = "  - Homework: " + item.Homework,
                        FontSize = 13,
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
                            FontSize = 13,
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
        LoadSchedule(currentWeekStart);
    }

    private void LastWeek_Click(object sender, RoutedEventArgs e)
    {
        currentWeekStart = currentWeekStart.AddDays(-7);
        LoadSchedule(currentWeekStart);
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
}