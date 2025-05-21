using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineDiary.Data;
using OnlineDiary.Models;

namespace OnlineDiary
{
    public partial class LoginWindowTeacher : Window
    {
        public LoginWindowTeacher()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(email) || email == "E-mail" || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both email and password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=diary.db")
                .Options;

            using (var context = new AppDbContext(options))
            {
                var user = context.Users
                    .FirstOrDefault(u => u.Email == email && u.Password == password);

                if (user != null)
                {
                    if (user.Role == "Teacher")
                    {
                        TeacherMainWindow teacherWindow = new TeacherMainWindow(user);
                        teacherWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Unknown user role.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid email or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
