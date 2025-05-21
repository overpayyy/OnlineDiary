using System.Windows;
using System.Windows.Navigation;

namespace OnlineDiary
{
    public partial class RoleSelectionWindow : Window
    {
        public RoleSelectionWindow()
        {
            InitializeComponent();
        }

        private void StudentButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new LoginWindowStudent();
            window.Show();
            this.Close();
        }

        private void TeacherButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new LoginWindowTeacher();
            window.Show();
            this.Close();
        }
    }
}
