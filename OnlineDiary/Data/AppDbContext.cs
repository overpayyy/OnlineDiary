using Microsoft.EntityFrameworkCore;
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=diary.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, FirstName = "Maksym", LastName = "Slipchyshyn", Email = "slipchyshyn@gmail.com", Password = "1234", Role = "Student" },
                new User { Id = 2, FirstName = "Oksana", LastName = "Petrenko", Email = "petrenko@gmail.com", Password = "admin", Role = "Teacher" },
                new User { Id = 3, FirstName = "Oleh", LastName = "Gorodysko", Email = "gorodysko@gmail.com", Password = "4321", Role = "Student" }
            );

            modelBuilder.Entity<Subject>().HasData(
                new Subject { Id = 1, Name = "Mathematics" },
                new Subject { Id = 2, Name = "Physics" },
                new Subject { Id = 3, Name = "English" },
                new Subject { Id = 4, Name = "Chemistry" },
                new Subject { Id = 5, Name = "History" }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}