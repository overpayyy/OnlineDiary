using System.Windows;
using Microsoft.EntityFrameworkCore;
using OnlineDiary.Data;

namespace OnlineDiary
{
    public partial class App : Application
    {
        public static class DbInitializer
        {
            public static void Initialize()
            {
                using (var context = new AppDbContext())
                {
                    context.Database.EnsureCreated();
                }
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DbInitializer.Initialize();

            using (var context = new AppDbContext())
            {
                context.Database.EnsureCreated();
            }

            var roleSelectionWindow = new RoleSelectionWindow();
            roleSelectionWindow.Show();
        }
    }
}
