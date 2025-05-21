using System.Windows;
using Microsoft.EntityFrameworkCore;
using OnlineDiary.Data;

namespace OnlineDiary
{
    public partial class App : Application
    {
        public static AppDbContext DbContext { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=diary.db")
                .Options;

            DbContext = new AppDbContext(options);
            DbContext.Database.Migrate();

            base.OnStartup(e);

            using (var context = new AppDbContext(options))
            {
                AppDbContext.SeedData(context);
            }

            var roleSelectionWindow = new RoleSelectionWindow();
            roleSelectionWindow.Show();
        }
    }
}
