using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineDiary.Models;

namespace OnlineDiary.Data
{
    public class AppDbContext : DbContext
    {
        

        public DbSet<User> Users { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<ScheduleEntry> ScheduleEntries { get; set; }
        public DbSet<Announcement> Announcements { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=diary.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, FirstName = "Maksym", LastName = "Slipchyshyn", Email = "slipchyshyn@gmail.com", Password = "1234", Role = "Student" },
                new User { Id = 2, FirstName = "Oksana", LastName = "Petrenko", Email = "petrenko@gmail.com", Password = "admin", Role = "Teacher" }
            );
        }

       
    }
}
