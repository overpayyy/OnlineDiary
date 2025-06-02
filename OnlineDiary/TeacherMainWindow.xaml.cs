using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using OnlineDiary.Data;
using OnlineDiary;
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
                        .Where(l => l.Date.Date == date.Date)
                        .OrderBy(l => l.Time)
                        .Select(l => $"{l.Subject.Name} at {DateTime.Parse(l.Time).ToString("hh:mm")}" +
                                    (string.IsNullOrEmpty(l.Homework) ? "" : $"\nHomework: {l.Homework}"))
                        .ToList();

                    string title = $"{days[i]} ({date:dd.MM.yyyy})";
                    var card = CreateDayCard(title, lessons);
                    ScheduleCards.Children.Add(card);
                }

                var notes = context.Announcements
                    .Where(a => a.WeekStart == weekStart)
                    .Select(a => a.Text)
                    .ToList();

                var notesCard = CreateDayCard("Notes", notes);
                ScheduleCards.Children.Add(notesCard);
            }
        }

        private void LoadStudentsAndSubjects()
        {
            // Заповнення StudentComboBox імена та прізвища
            StudentComboBox.Items.Clear();
            StudentComboBox.Items.Add("John Doe");
            StudentComboBox.Items.Add("Jane Smith");
            StudentComboBox.Items.Add("Alex Brown");

            // Заповнення SubjectComboBox і SubjectComboBoxSchedule однаковими значеннями
            SubjectComboBox.Items.Clear();
            SubjectComboBoxSchedule.Items.Clear();
            SubjectComboBox.Items.Add("Math");
            SubjectComboBox.Items.Add("Physics");
            SubjectComboBox.Items.Add("English");
            SubjectComboBoxSchedule.Items.Add("Math");
            SubjectComboBoxSchedule.Items.Add("Physics");
            SubjectComboBoxSchedule.Items.Add("English");

            // (Опціонально) Завантаження з бази даних, якщо є таблиця Subjects
            using (var context = new AppDbContext())
            {
                var subjects = context.Subjects.Select(s => s.Name).ToList();
                SubjectComboBox.Items.Clear();
                SubjectComboBoxSchedule.Items.Clear();
                foreach (var subject in subjects)
                {
                    SubjectComboBox.Items.Add(subject);
                    SubjectComboBoxSchedule.Items.Add(subject);
                }
            }
        }

        private Border CreateDayCard(string title, List<string> items)
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
                var text = new TextBlock
                {
                    Text = "• " + item,
                    FontSize = 13,
                    Margin = new Thickness(0, 2, 0, 2),
                    TextWrapping = TextWrapping.Wrap
                };
                stack.Children.Add(text);
            }

            card.Child = stack;
            return card;
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
            if (dayOffset == -1) return;

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
                    subject = new Subject { Name = subjectName };
                    context.Subjects.Add(subject);
                    context.SaveChanges();
                }

                var lesson = new Lesson
                {
                    Date = currentWeekStart.AddDays(dayOffset),
                    Subject = subject,
                    Time = TimeTextBox.Text,
                    Homework = HomeworkTextBox.Text
                };
                context.Lessons.Add(lesson);
                context.SaveChanges();
            }

            LoadSchedule(currentWeekStart);
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
            ScheduleCards.Children.Add(gradeCard);

            StudentComboBox.SelectedItem = null;
            GradeDayComboBox.SelectedItem = null;
            SubjectComboBox.SelectedItem = null;
            GradeTextBox.Clear();
            DescriptionTextBox.Clear();
            MessageBox.Show("Grade added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddAnnouncement_Click(object sender, RoutedEventArgs e)
        {
            if (WeekStartDatePicker.SelectedDate == null || string.IsNullOrEmpty(AnnouncementTextBox.Text))
            {
                MessageBox.Show("Please select a week start date and enter announcement text.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var context = new AppDbContext())
            {
                var announcement = new Announcement
                {
                    WeekStart = WeekStartDatePicker.SelectedDate.Value.Date,
                    Text = AnnouncementTextBox.Text
                };
                context.Announcements.Add(announcement);
                context.SaveChanges();
            }

            if (WeekStartDatePicker.SelectedDate.Value.Date == currentWeekStart)
            {
                LoadSchedule(currentWeekStart);
            }

            AnnouncementTextBox.Clear();
            MessageBox.Show("Announcement added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
    }
}