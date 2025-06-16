using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using OnlineDiary.Data;
using OnlineDiary.Models;

namespace OnlineDiary
{
    public partial class TeacherMainWindow : Window
    {
        private DateTime currentWeekStart;
        private List<(string Student, string Day, string Subject, string Grade, string Description)> grades;

        public TeacherMainWindow()
        {
            InitializeComponent();
            currentWeekStart = GetStartOfWeek(DateTime.Today);
            grades = new List<(string, string, string, string, string)>();
            LoadSchedule(currentWeekStart);
            LoadStudentsAndSubjects();
            LoadGradesSchedule(currentWeekStart);
            LoadGradeHistory();
        }

        private DateTime GetStartOfWeek(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private void LoadSchedule(DateTime weekStart)
        {
            ScheduleCards.Children.Clear();

            List<string> days = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };

            using (var context = new AppDbContext())
            {
                for (int i = 0; i < days.Count; i++)
                {
                    DateTime date = weekStart.AddDays(i);
                    var lessons = context.Lessons
                        .Include(l => l.Subject)
                        .Include(l => l.Teacher)
                        .Where(l => l.Date.Date == date.Date)
                        .OrderBy(l => l.Time)
                        .Select(l => new {
                            l.Id,
                            Text = $"{l.Subject.Name} at {DateTime.Parse(l.Time).ToString("hh:mm")}" +
                                    (string.IsNullOrEmpty(l.Homework) ? "" : $"\nHomework: {l.Homework}") +
                                    (l.Teacher != null ? $"\nTeacher: {l.Teacher.FirstName} {l.Teacher.LastName}" : "")
                        })
                        .ToList();

                    string title = $"{days[i]} ({date:dd.MM.yyyy})";
                    var card = CreateDayCard(title, lessons, true);
                    ScheduleCards.Children.Add(card);
                }
            }
        }

        private void LoadGradesSchedule(DateTime weekStart)
        {
            GradesScheduleCards.Children.Clear();

            List<string> days = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };

            using (var context = new AppDbContext())
            {
                for (int i = 0; i < days.Count; i++)
                {
                    DateTime date = weekStart.AddDays(i);
                    var lessons = context.Lessons
                        .Include(l => l.Subject)
                        .Include(l => l.Teacher)
                        .Where(l => l.Date.Date == date.Date)
                        .OrderBy(l => l.Time)
                        .Select(l => new {
                            l.Id,
                            Text = $"{l.Subject.Name} at {DateTime.Parse(l.Time).ToString("hh:mm")}" +
                                    (l.Teacher != null ? $"\nTeacher: {l.Teacher.FirstName} {l.Teacher.LastName}" : "")
                        })
                        .ToList();

                    string title = $"{days[i]} ({date:dd.MM.yyyy})";
                    var card = CreateDayCard(title, lessons);
                    GradesScheduleCards.Children.Add(card);
                }
            }
        }

        private void LoadStudentsAndSubjects()
        {
            using (var context = new AppDbContext())
            {
                // Load students
                var students = context.Users
                    .Where(u => u.Role == "Student")
                    .Select(u => $"{u.FirstName} {u.LastName}")
                    .ToList();
                StudentComboBox.Items.Clear();
                if (students.Count == 0)
                {
                    MessageBox.Show("No students found in the database. Please ensure users are seeded.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                foreach (var student in students)
                {
                    StudentComboBox.Items.Add(student);
                }

                // Load subjects
                var subjects = context.Subjects.Select(s => s.Name).ToList();
                SubjectComboBox.Items.Clear();
                SubjectComboBoxSchedule.Items.Clear();
                if (subjects.Count == 0)
                {
                    MessageBox.Show("No subjects found in the database. Please ensure subjects are seeded.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                foreach (var subject in subjects)
                {
                    SubjectComboBox.Items.Add(subject);
                    SubjectComboBoxSchedule.Items.Add(subject);
                }
            }
        }

        private void LoadGradeHistory()
        {
            using (var context = new AppDbContext())
            {
                var grades = context.Grades
                    .Include(g => g.Student)
                    .Include(g => g.Lesson).ThenInclude(l => l.Subject)
                    .Include(g => g.Lesson).ThenInclude(l => l.Teacher)
                    .Select(g => new
                    {
                        g.Id,
                        StudentName = $"{g.Student.FirstName} {g.Student.LastName}",
                        Date = g.Lesson.Date,
                        SubjectName = g.Lesson.Subject.Name,
                        g.Value,
                        TeacherName = g.Lesson.Teacher != null ? $"{g.Lesson.Teacher.FirstName} {g.Lesson.Teacher.LastName}" : "N/A"
                    })
                    .ToList();
                GradesDataGrid.ItemsSource = grades;
            }
        }

        private Border CreateDayCard(string title, dynamic items, bool isLessonCard = false)
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
                Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 5, ShadowDepth = 2, Opacity = 0.2 }
            };

            var stack = new StackPanel();

            var dayTitle = new TextBlock
            {
                Text = title,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            stack.Children.Add(dayTitle);

            if (items.Count == 0)
            {
                stack.Children.Add(new TextBlock { Text = "No entries", Foreground = Brushes.Gray });
            }

            foreach (var item in items)
            {
                var itemStack = new StackPanel { Orientation = Orientation.Horizontal };
                var text = new TextBlock
                {
                    Text = "• " + item.Text,
                    FontSize = 13,
                    Margin = new Thickness(0, 2, 0, 2),
                    TextWrapping = TextWrapping.Wrap,
                    Width = 180
                };
                itemStack.Children.Add(text);

                if (isLessonCard)
                {
                    var deleteButton = new Button
                    {
                        Content = "Delete",
                        Style = FindResource("DeleteButton") as Style,
                        Tag = item.Id,
                        Margin = new Thickness(5, 2, 0, 2),
                        Padding = new Thickness(5, 2, 5, 2)
                    };
                    deleteButton.Click += DeleteLesson_Click;
                    itemStack.Children.Add(deleteButton);
                }

                stack.Children.Add(itemStack);
            }

            card.Child = stack;
            return card;
        }

        private void DeleteLesson_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int lessonId)
            {
                var result = MessageBox.Show("Are you sure you want to delete this lesson?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    using (var context = new AppDbContext())
                    {
                        var lesson = context.Lessons.Find(lessonId);
                        if (lesson != null)
                        {
                            context.Lessons.Remove(lesson);
                            context.SaveChanges();
                            LoadSchedule(currentWeekStart);
                            LoadGradesSchedule(currentWeekStart);
                            MessageBox.Show("Lesson deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
        }

        private void AddLesson_Click(object sender, RoutedEventArgs e)
        {
            if (DayComboBox.SelectedItem == null || SubjectComboBoxSchedule.SelectedItem == null || string.IsNullOrEmpty(TimeTextBox.Text))
            {
                MessageBox.Show("Please fill in all required fields (Day, Subject, Time).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string selectedDay = (DayComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            int dayOffset = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" }.IndexOf(selectedDay);
            if (dayOffset == -1)
            {
                MessageBox.Show("Invalid day selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!DateTime.TryParse(TimeTextBox.Text, out DateTime time))
            {
                MessageBox.Show("Invalid time format. Use HH:mm (e.g., 09:30).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var context = new AppDbContext())
            {
                string subjectName = SubjectComboBoxSchedule.SelectedItem.ToString().Trim();
                var subject = context.Subjects.FirstOrDefault(s => s.Name == subjectName);
                if (subject == null)
                {
                    MessageBox.Show($"Selected subject '{subjectName}' does not exist in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Set TeacherId to the current teacher (e.g., Oksana Petrenko with Id = 2)
                int teacherId = 2; // Hardcoded for now, adjust based on logged-in teacher

                // Debug output to check SubjectId and TeacherId
                MessageBox.Show($"Adding lesson with SubjectId: {subject.Id}, SubjectName: {subject.Name}, TeacherId: {teacherId}", "Debug Info", MessageBoxButton.OK, MessageBoxImage.Information);

                var lesson = new Lesson
                {
                    Date = currentWeekStart.AddDays(dayOffset),
                    SubjectId = subject.Id,
                    TeacherId = teacherId,
                    Time = TimeTextBox.Text,
                    Homework = HomeworkTextBox.Text
                };

                try
                {
                    context.Lessons.Add(lesson);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to add lesson. Error: {ex.Message}\nInner Exception: {ex.InnerException?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            LoadSchedule(currentWeekStart);
            LoadGradesSchedule(currentWeekStart);
            SubjectComboBoxSchedule.SelectedItem = null;
            TimeTextBox.Clear();
            HomeworkTextBox.Clear();
            MessageBox.Show("Lesson added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddGrade_Click(object sender, RoutedEventArgs e)
        {
            if (StudentComboBox.SelectedItem == null || GradeDayComboBox.SelectedItem == null || SubjectComboBox.SelectedItem == null || string.IsNullOrEmpty(GradeTextBox.Text))
            {
                MessageBox.Show("Please fill in all required fields (Student, Day, Subject, Grade).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(GradeTextBox.Text, out int grade) || grade < 1 || grade > 12)
            {
                MessageBox.Show("Grade must be a number between 1 and 12.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string student = StudentComboBox.SelectedItem.ToString();
            string day = (GradeDayComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string subject = SubjectComboBox.SelectedItem.ToString();
            string description = DescriptionTextBox.Text;

            grades.Add((student, day, subject, grade.ToString(), description));

            var gradeCard = CreateDayCard($"Grade for {student} ({day})", new List<string> { $"{subject}: {grade} - {description}" });
            GradesScheduleCards.Children.Add(gradeCard);

            StudentComboBox.SelectedItem = null;
            GradeDayComboBox.SelectedItem = null;
            SubjectComboBox.SelectedItem = null;
            GradeTextBox.Clear();
            DescriptionTextBox.Clear();
            LoadGradeHistory();
            MessageBox.Show("Grade added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void NextWeek_Click(object sender, RoutedEventArgs e)
        {
            currentWeekStart = currentWeekStart.AddDays(7);
            LoadSchedule(currentWeekStart);
            LoadGradesSchedule(currentWeekStart);
        }

        private void LastWeek_Click(object sender, RoutedEventArgs e)
        {
            currentWeekStart = currentWeekStart.AddDays(-7);
            LoadSchedule(currentWeekStart);
            LoadGradesSchedule(currentWeekStart);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var roleWindow = new RoleSelectionWindow();
            roleWindow.Show();
            this.Close();
        }
    }
}