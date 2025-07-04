﻿using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using OnlineDiary.Data;
using OnlineDiary.Models;

namespace OnlineDiary
{
    public partial class LoginWindowStudent : Window
    {
        public LoginWindowStudent()
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

            using (var context = new AppDbContext())
            {
                var user = context.Users
                    .FirstOrDefault(u => u.Email == email && u.Password == password);

                if (user != null)
                {
                    if (user.Role == "Student")
                    {
                        StudentMainWindow studentWindow = new StudentMainWindow(user.Id);
                        studentWindow.Show();
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