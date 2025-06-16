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
            WindowState = WindowState.Maximized;
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
            var scheduleCards = FindName("ScheduleCards") as WrapPanel;
            if (scheduleCards != null)
            {
                scheduleCards.Children.Clear();
            }
            else
            {
                return;
            }

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
                        .Select(l => new
                        {
                            l.Id,
                            Time = l.Time,
                            Text = $"{l.Subject.Name} at {DateTime.Parse(l.Time).ToString("hh:mm")}" +
                                   (string.IsNullOrEmpty(l.Homework) ? "" : $"\nHomework: {l.Homework}") +
                                   (l.Teacher != null ? $"\nTeacher: {l.Teacher.FirstName} {l.Teacher.LastName}" : "")
                        })
                        .ToList()
                        .Select(l => new
                        {
                            l.Id,
                            l.Time,
                            ParsedTime = DateTime.TryParse(l.Time, out DateTime time) ? time : DateTime.MinValue,
                            l.Text
                        })
                        .OrderBy(l => l.ParsedTime)
                        .ToList();

                    string title = $"{days[i]} ({date:dd.MM.yyyy})";
                    var card = CreateDayCard(title, lessons, true);
                    scheduleCards.Children.Add(card);
                }
            }
        }

        private void LoadGradesSchedule(DateTime weekStart)
        {
            var gradesScheduleCards = FindName("GradesScheduleCards") as WrapPanel;
            if (gradesScheduleCards != null)
            {
                gradesScheduleCards.Children.Clear();
            }
            else
            {
                return;
            }

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
                        .Select(l => new
                        {
                            l.Id,
                            Text = $"{l.Subject.Name} at {DateTime.Parse(l.Time).ToString("hh:mm")}" +
                                   (l.Teacher != null ? $"\nTeacher: {l.Teacher.FirstName} {l.Teacher.LastName}" : "")
                        })
                        .ToList();

                    string title = $"{days[i]} ({date:dd.MM.yyyy})";
                    var card = CreateDayCard(title, lessons, true);
                    gradesScheduleCards.Children.Add(card);
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
                SubjectFilterComboBox.Items.Clear();
                SubjectFilterComboBox.Items.Add("All"); // Опція "Всі"
                if (subjects.Count == 0)
                {
                    MessageBox.Show("No subjects found in the database. Please ensure subjects are seeded.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                foreach (var subject in subjects)
                {
                    SubjectComboBox.Items.Add(subject);
                    SubjectComboBoxSchedule.Items.Add(subject);
                    SubjectFilterComboBox.Items.Add(subject);
                }
            }
        }

        private void LoadGradeHistory()
        {
            using (var context = new AppDbContext())
            {
                var query = context.Grades
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
                    });

                // Фільтрація за вибраним предметом
                if (SubjectFilterComboBox.SelectedItem != null && SubjectFilterComboBox.SelectedItem.ToString() != "All")
                {
                    string selectedSubject = SubjectFilterComboBox.SelectedItem.ToString();
                    query = query.Where(g => g.SubjectName == selectedSubject);
                }

                // Сортування за датою від минулих до нових
                var grades = query
                    .OrderBy(g => g.Date)
                    .ToList();

                GradesDataGrid.ItemsSource = grades;
            }
        }

        private void SubjectFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadGradeHistory();
        }

        private Border CreateDayCard(string title, dynamic items, bool isLessonCard = false)
        {
            var card = new Border
            {
                BorderThickness = new Thickness(2),
                BorderBrush = Brushes.LightGray,
                CornerRadius = new CornerRadius(15),
                Margin = new Thickness(15),
                Width = 300,
                Background = Brushes.White,
                Padding = new Thickness(15),
                Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 8, ShadowDepth = 3, Opacity = 0.3 }
            };

            var stack = new StackPanel();

            var dayTitle = new TextBlock
            {
                Text = title,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 15)
            };
            stack.Children.Add(dayTitle);

            if (items.Count == 0) // Перевірка, яка викликала помилку
            {
                stack.Children.Add(new TextBlock { Text = "No entries", Foreground = Brushes.Gray, FontSize = 14 });
            }

            foreach (var item in items)
            {
                var itemStack = new StackPanel { Orientation = Orientation.Horizontal };
                var text = new TextBlock
                {
                    Text = item is string ? item : item.Text,
                    FontSize = 15,
                    Margin = new Thickness(0, 5, 10, 5),
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 250
                };
                itemStack.Children.Add(text);

                if (isLessonCard)
                {
                    var deleteButton = new Button
                    {
                        Style = FindResource("DeleteButton") as Style,
                        Tag = item is string ? null : item.Id,
                        Margin = new Thickness(0, 5, 0, 5),
                        Width = 30,
                        Height = 30,
                        VerticalAlignment = VerticalAlignment.Top
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
            // Check if all required fields are filled
            if (StudentComboBox.SelectedItem == null || GradeDayComboBox.SelectedItem == null ||
                SubjectComboBox.SelectedItem == null || string.IsNullOrEmpty(GradeTextBox.Text))
            {
                MessageBox.Show("Please fill in all required fields (Student, Day, Subject, Grade).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate grade value as a string
            string gradeText = GradeTextBox.Text.Trim();
            if (string.IsNullOrEmpty(gradeText) || !int.TryParse(gradeText, out int gradeValue) || gradeValue < 1 || gradeValue > 12)
            {
                MessageBox.Show("Grade must be a valid number between 1 and 12.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string student = StudentComboBox.SelectedItem.ToString();
            string day = (GradeDayComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string subject = SubjectComboBox.SelectedItem.ToString();
            string description = DescriptionTextBox.Text;

            using (var context = new AppDbContext())
            {
                // Find the student entity
                var studentEntity = context.Users.FirstOrDefault(u => (u.FirstName + " " + u.LastName) == student);
                var subjectEntity = context.Subjects.FirstOrDefault(s => s.Name == subject);
                if (studentEntity == null || subjectEntity == null)
                {
                    MessageBox.Show("Student or subject not found in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Convert the selected day string to DayOfWeek enum
                if (!Enum.TryParse<DayOfWeek>(day, out var selectedDayOfWeek))
                {
                    MessageBox.Show("Invalid day selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Find the matching lesson using DayOfWeek
                var lesson = context.Lessons.FirstOrDefault(l =>
                    l.SubjectId == subjectEntity.Id &&
                    l.Date.DayOfWeek == selectedDayOfWeek &&
                    l.Date.Date >= currentWeekStart &&
                    l.Date.Date < currentWeekStart.AddDays(7));

                if (lesson == null)
                {
                    MessageBox.Show("No matching lesson found for the selected day and subject.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create and save the new grade
                var newGrade = new Grade
                {
                    StudentId = studentEntity.Id,
                    LessonId = lesson.Id,
                    Value = gradeText,
                    DateAdded = DateTime.Now,
                    Description = description
                };

                context.Grades.Add(newGrade);
                context.SaveChanges();
            }

            // Refresh the grade history
            LoadGradeHistory();

            // Clear input fields
            StudentComboBox.SelectedItem = null;
            GradeDayComboBox.SelectedItem = null;
            SubjectComboBox.SelectedItem = null;
            GradeTextBox.Clear();
            DescriptionTextBox.Clear();
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