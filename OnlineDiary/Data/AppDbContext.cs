using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineDiary.Models;

namespace OnlineDiary.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<ScheduleEntry> ScheduleEntries { get; set; }

        public static void SeedData(AppDbContext context)
        {
            if (!context.Users.Any())
            {
                var teacher = new User
                {
                    FirstName = "Maksym",
                    LastName = "Slipchyshyn",
                    Email = "slipcisin@gmail.com",
                    Password = "admin",
                    Role = "Teacher"
                };

                var student = new User
                {
                    FirstName = "Viktor",
                    LastName = "Butenko",
                    Email = "butenko@gmail.com",
                    Password = "1234",
                    Role = "Student"
                };

                context.Users.AddRange(teacher, student);
                context.SaveChanges();
            }
        }

    }
}
