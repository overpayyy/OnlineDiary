using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using OnlineDiary.Data;
using OnlineDiary;

public partial class StudentMainWindow : Window
{
    private DateTime currentWeekStart;

    public StudentMainWindow()
    {
        InitializeComponent();
        currentWeekStart = GetStartOfWeek(DateTime.Today);
        LoadSchedule(currentWeekStart);
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
                    .Select(l => l.Subject.Name + " at " + DateTime.Parse(l.Time).ToString("hh:mm", System.Globalization.CultureInfo.InvariantCulture))
                    .ToList();

                string title = $"{days[i]} ({date:dd.MM.yyyy})";
                var card = CreateDayCard(title, lessons);
                ScheduleCards.Children.Add(card);
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
                Margin = new Thickness(0, 2, 0, 2)
            };
            stack.Children.Add(text);
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
}