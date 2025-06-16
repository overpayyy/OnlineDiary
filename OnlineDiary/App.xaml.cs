using System.Windows;
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

            var roleSelectionWindow = new RoleSelectionWindow();
            roleSelectionWindow.Show();
        }
    }
}