using Microsoft.EntityFrameworkCore;
using DbsClasses;
using System.Text.RegularExpressions;
using Microservices;

public class ChatDb : DbContext
{
    private DbSet<Chat> Chats => Set<Chat>();
    public ChatDb() => Database.EnsureCreated();
    private readonly AESClass _aes = new();
 
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=chatdb.db");
    }
    public List<Chat> GetChats()
    {
        return Chats.ToList();
    }
    public string AddUser(Chat chat)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(chat.Name))
                return "Name cannot be empty";
            if (Chats.Any(u => u.Name == chat.Name))
                return "Chat with this name already exists";
            if (Regex.IsMatch(chat.Name, @"[^a-zA-Z0-9]"))
                return "Name must contain only letters and numbers";
            Chats.Add(chat);
            SaveChanges();
            LoggingService.LogAsync($"{chat} added");
            return "Chat added";
        }
        catch (Exception ex)
        {
            LoggingService.ErrorAsync(ex.Message);
            return ex.Message;
        }
    }
}