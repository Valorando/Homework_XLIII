using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;


//1. FromSqlRaw

//----------------------------------------------------------------------------------------------
//Пересоздание и заполнение таблиц
using (ApplicationContext db = new ApplicationContext())
{
    // пересоздаем базу данных
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
    Company microsoft = new Company { Name = "Microsoft" };
    Company google = new Company { Name = "Google" };
    db.Companies.AddRange(microsoft, google);
    User tom = new User
    {
        Name = "Tom",
        Age = 36,
        Company = microsoft
    };
    User bob = new User
    {
        Name = "Bob",
        Age = 39,
        Company = google
    };
    User alice = new User
    {
        Name = "Alice",
        Age = 28,
        Company = microsoft
    };
    User kate = new User
    {
        Name = "Kate",
        Age = 25,
        Company = google
    };
    User tomas = new User
    {
        Name = "Tomas",
        Age = 22,
        Company = microsoft
    };
    User tomek = new User
    {
        Name = "Tomek",
        Age = 42,
        Company = google
    };
    db.Users.AddRange(tom, bob, alice, kate, tomas, tomek);
    db.SaveChanges();
}

//SQL запрос с использованием FromSqlRaw
using (ApplicationContext db = new ApplicationContext())
{
    var comps = db.Companies.FromSqlRaw("SELECT * FROM Companies").ToList();
        foreach (var company in comps)
        Console.WriteLine(company.Name);
}

//Тоже самое, но с добавлением LINQ
using (ApplicationContext db = new ApplicationContext())
{
    var comps = db.Companies.FromSqlRaw("SELECT * FROM Companies").OrderBy(x=>x.Name).ToList();
        foreach (var company in comps)
        Console.WriteLine(company.Name);
}

//Тоже самое, но с использованием метода Include
using (ApplicationContext db = new ApplicationContext())
{
    var users = db.Users.FromSqlRaw("SELECT * FROM Users").Include(c => c.Company).ToList();
    foreach (var user in users)
        Console.WriteLine($"{user.Name} -{ user.Company?.Name}");
}





//2. FromSqlRaw с параметрами

//-------------------------------------------------------------------------------------------------------

//Конструктор SqlParameter, тут для SQLite
using (ApplicationContext db = new ApplicationContext())
{
    SqliteParameter param = new SqliteParameter("@name","%Tom%");
    var users = db.Users.FromSqlRaw("SELECT * FROM Users WHERE Name LIKE @name", param).ToList();
        foreach (var user in users)
        Console.WriteLine(user.Name);
}

// Аналогично для MS SQL Server
//using (ApplicationContext db = new ApplicationContext())
//{
//    SqlParameter param = new SqlParameter("@name", "%Tom%");
//    var users = db.Users.FromSqlRaw("SELECT * FROM Users WHERE Name LIKE @name", param).ToList();
//    foreach (var user in users)
//        Console.WriteLine(user.Name);
//}


//Параметры в виде переменных
using (ApplicationContext db = new ApplicationContext())
{
    var name = "%Tom%";
    var users = db.Users.FromSqlRaw("SELECT * FROM Users WHERE Name LIKE {0}", name).ToList();
        foreach (var user in users)
        Console.WriteLine(user.Name);

    var age = 30;
    users = db.Users.FromSqlRaw("SELECT * FROM Users WHERE Age > {0}", age).ToList();
        foreach (var user in users)
        Console.WriteLine(user.Name);
}


//3. ExecuteSqlRaw

//-------------------------------------------------------------------------------------------------------------


using (ApplicationContext db = new ApplicationContext())
{
    // вставка
    string newComp = "Apple";
    int numberOfRowInserted = db.Database.ExecuteSqlRaw("INSERT INTO Companies(Name) VALUES({0})", newComp);
   // асинхронная версия

   // int numberOfRowInserted2 = await
   db.Database.ExecuteSqlRawAsync("INSERT INTO Companies (Name) VALUES({0})", newComp);



    // обновление
    string appleInc = "Apple Inc.";
    string apple = "Apple";
    int numberOfRowUpdated = db.Database.ExecuteSqlRaw("UPDATE Companies SET Name ={0} WHERE Name = {1}", appleInc, apple);

    // удаление
    int numberOfRowDeleted = db.Database.ExecuteSqlRaw("DELETE FROM Companies WHERE Name ={0} ", appleInc);
}




//4. Интерполяция строк, методы FromSqlInterpolated() и ExecuteSqlInterpolated()

//------------------------------------------------------------------------------------------------------------------------


using (ApplicationContext db = new ApplicationContext())
{
    var name = "%Tom%";
    var age = 30;
    var users = db.Users.FromSqlInterpolated($"SELECT * FROM Users WHERE Name LIKE { name} AND Age > { age} ").ToList();
        foreach (var user in users)
        Console.WriteLine(user.Name);
}


//ExecuteSqlInterpolated() и FromSqlInterpolated()
using (ApplicationContext db = new ApplicationContext())
{
    string jetbrains = "JetBrains";
    db.Database.ExecuteSqlInterpolated($"INSERT INTO Companies(Name) VALUES({ jetbrains})");
// асинхронная версия
// await
db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO Companies(Name) VALUES({ jetbrains})");
foreach (var comp in db.Companies.ToList())
        Console.WriteLine(comp.Name);
}


//5. Хранимые функции. Обращение к функции в запросе SQL

//------------------------------------------------------------------------------------------------------------------------

//using (ApplicationContext db = new ApplicationContext())
//{
//    SqlParameter param = new SqlParameter("@age", 30);
//    var users = db.Users.FromSqlRaw("SELECT * FROM GetUsersByAge(@age)", param).ToList();
//        foreach (var u in users)
//        Console.WriteLine($"{u.Name} - {u.Age}");
//}

// Проецирование хранимой функции на метод класса

//public class ApplicationContext : DbContext
//{
//    public DbSet<Company> Companies { get; set; } = null!;
//    public DbSet<User> Users { get; set; } = null!;
//    public IQueryable<User> GetUsersByAge(int age) =>
//    FromExpression(() => GetUsersByAge(age));
//    protected override void
//    OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//    {
//        optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb; Database=helloappdb;Trusted_Connection=True;");
//    }
//    protected override void OnModelCreating(ModelBuilder
//    modelBuilder)
//    {
//        modelBuilder.HasDbFunction(() =>
//        GetUsersByAge(default));
//    }
//}

//using (ApplicationContext db = new ApplicationContext())
//{
//    var users = db.GetUsersByAge(30); // обращение к хранимой функции
//        foreach (var u in users)
//        Console.WriteLine($"{u.Name} - {u.Age}");
//}



//6. Хранимые процедуры. Обращение к проецедуре в запросе SQL

//---------------------------------------------------------------------------------------------------------------------------------------


//using (ApplicationContext db = new ApplicationContext())
//{
//    SqlParameter param = new("@name", "Microsoft");
//    var users = db.Users.FromSqlRaw("GetUsersByCompany @name", param).ToList();
//        foreach (var p in users)
//        Console.WriteLine($"{p.Name} - {p.Age}");
//}


//using (ApplicationContext db = new ApplicationContext())
//{
//    SqlParameter param = new()
//    {
//        ParameterName = "@userName",
//        SqlDbType = System.Data.SqlDbType.VarChar,
//        Direction = System.Data.ParameterDirection.Output,
//        Size = 50
//    };
//    db.Database.ExecuteSqlRaw("GetUserWithMaxAge @userName
//    OUT", param);
//    Console.WriteLine(param.Value);
//}

public class Company
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public List<User> Users { get; set; } = new();
}
public class User
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int Age { get; set; }
    public int CompanyId { get; set; }
    public Company? Company { get; set; }
}


public class ApplicationContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Company> Companies { get; set; } = null!;
    protected override void
    OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=helloapp.db");
    }
}

