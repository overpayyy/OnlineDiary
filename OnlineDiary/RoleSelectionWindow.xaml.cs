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
            var studentLogin = new LoginWindowStudent();
            NavigationService.GetNavigationService(this)?.Navigate(studentLogin);
            this.Close();
        }

        private void TeacherButton_Click(object sender, RoutedEventArgs e)
        {
            var teacherLogin = new LoginWindowTeacher();
            NavigationService.GetNavigationService(this)?.Navigate(teacherLogin);
            this.Close();
        }
    }
}
