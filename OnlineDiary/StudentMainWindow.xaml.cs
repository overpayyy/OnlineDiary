using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

public partial class StudentMainWindow : Window
{
    public StudentMainWindow()
    {
        InitializeComponent();
        LoadSchedule();
    }

    private void LoadSchedule()
    {
        List<string> days = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };

        foreach (string day in days)
        {
            var card = CreateDayCard(day, new List<string>
            {
                "Math", "English", "Biology", "History", "PE", "Art"
            });

            ScheduleCards.Children.Add(card);
        }

        // Sixth card — Notes/Announcements
        var notesCard = CreateDayCard("Notes", new List<string>
        {
            "Math test on Wednesday.",
            "Bring PE clothes on Friday.",
            "Art homework due Thursday."
        });

        ScheduleCards.Children.Add(notesCard);
    }

    private Border CreateDayCard(string title, List<string> lessons)
    {
        var card = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.LightGray,
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(10),
            Width = 300,
            Background = Brushes.White,
            Padding = new Thickness(10),
        };

        var stack = new StackPanel();

        var dayTitle = new TextBlock
        {
            Text = title,
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        stack.Children.Add(dayTitle);

        foreach (var lesson in lessons)
        {
            var text = new TextBlock
            {
                Text = "• " + lesson,
                FontSize = 14,
                Margin = new Thickness(0, 2, 0, 2)
            };
            stack.Children.Add(text);
        }

        card.Child = stack;
        return card;
    }
}
