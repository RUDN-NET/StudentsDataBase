using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace StudentsDataBase
{
    class Program
    {
        static (Group[], Student[]) GetStudents()
        {
            var groups = new Group[10];
            for (var i = 0; i < groups.Length; i++)
                groups[i] = new Group
                {
                    Name = $"Группа - {i + 1}"
                };

            var rnd = new Random();
            var students = new Student[100];
            for (var i = 0; i < students.Length; i++)
            {
                students[i] = new Student
                {
                    Name = $"Имя - {i + 1}",
                    LastName = $"Фамилия - {i + 1}",
                    Patronymic = $"Отчество - {i + 1}",

                    Age = rnd.Next(17, 31),
                    Rating = rnd.NextDouble() * 100,
                    Group = groups[rnd.Next(groups.Length)],
                };
                students[i].Group.Students.Add(students[i]);
            }

            return (groups, students);
        }

        static void Main(string[] args)
        {
            var db_opt = new DbContextOptionsBuilder()
               .UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=StudentsDB")
               .LogTo(str => Console.WriteLine(str));


            using (var db = new StudentsDB(db_opt.Options))
            {
                // Инициализация

                db.Database.EnsureCreated();

                var students_count = db.Students.Count();

                Console.WriteLine("Students count " + students_count);

                if (students_count == 0)
                {
                    var (groups, students) = GetStudents();
                    db.Students.AddRange(students);
                    //db.Groups.AddRange(groups); // Не обязательно!
                    db.SaveChanges();
                }
            }

            using (var db = new StudentsDB(db_opt.Options))
            {
                var students_with_rating_greater_75 = db.Students.Where(s => s.Rating > 75);

                var top_students_count = students_with_rating_greater_75.Count();
                Console.WriteLine("Число лучших студентов " + top_students_count);

                foreach (var student in students_with_rating_greater_75)
                    Console.WriteLine("{0} {1} {2:f2}", 
                        student.LastName,
                        student.Name,
                        student.Rating);

            }
        }
    }

    class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
    }

    class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }
        public int Age { get; set; }
        public double Rating { get; set; }
        public Group Group { get; set; }
    }

    class StudentsDB : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Group> Groups { get; set; }

        public StudentsDB(DbContextOptions opt) : base(opt) { }
    }
}
