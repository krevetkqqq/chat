using Microsoft.EntityFrameworkCore;
using DbsClasses;
using Microservices;
using System.Text.RegularExpressions;

public class ApplicationContext : DbContext
{
    private DbSet<User> Users => Set<User>();
    public ApplicationContext() => Database.EnsureCreated();
    private readonly AESClass _aes = new();
 
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=helloapp.db");
    }
    public string AddUser(User user)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(user.Name))
                return "Name cannot be empty";
            if (Users.Any(u => u.Name == user.Name))
                return "User with this name already exists";
            if (user.Password.Length < 6)
                return "Password must be at least 6 characters long";
            if (Regex.IsMatch(user.Password+user.Name, @"[^a-zA-Z0-9]"))
                return "Password or login must contain only letters and numbers";
            Users.Add(user);
            SaveChanges();
            LoggingService.LogAsync($"{user} added");
            return "User added";
        }
        catch (Exception ex)
        {
            LoggingService.ErrorAsync(ex.Message);
            return ex.Message;
        }
    }
    public bool DeleteUser(User user)
    {
        try
        {
            if (!Users.Any(u => u.Name == user.Name)) 
                return false;
            Users.Remove(user);
            SaveChanges();
            LoggingService.LogAsync($"{user} deleted");
            return true;
        }
        catch (Exception ex)
        {
            LoggingService.ErrorAsync(ex.Message);
            return false;
        }
    }
    public (string, string?) LoginUser(User notUser)
    {
        try
        {
            var user = Users.FirstOrDefault(u => u.Name == notUser.Name && u.Password == notUser.Password);
            if (user == null)
            {
                LoggingService.ErrorAsync("User not found");
                return ("User not found", "");
            }
            return ("Logged in", _aes.Encrypt(user.Name));

        }
        catch (Exception ex)
        {
            LoggingService.ErrorAsync(ex.Message);
            return ("Failed to login", null);
        }
    }
    public List<object> GetUsers()
    {
        return Users.Select(x => new { Id = x.Id, Name = x.Name, Admin = x.AdminStatus}).ToList<object>();
    }
}