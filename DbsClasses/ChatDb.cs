using Microsoft.EntityFrameworkCore;
using DbsClasses;
using System.Text.RegularExpressions;
using Microservices;

public class ChatDb : DbContext
{
    private DbSet<Chat> Chats => Set<Chat>();
    public ChatDb() => Database.EnsureCreated();
    private readonly AESClass _aes = new();
    private readonly UserDb _user = new();
 
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=chat.db");
    }
    public List<Chat> GetChats()
    {
        return Chats.ToList();
    }
    public Chat GetChat(string name)
    {
        return Chats.FirstOrDefault(x => x.Name == name);
    }
    public Chat GetChat(int id)
    {
        return Chats.FirstOrDefault(x => x.Id == id);
    }
    public string AddChat(Chat chat)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(chat.Name))
                return "Name cannot be empty";
            if (Chats.Any(u => u.Name == chat.Name))
                return "Chat with this name already exists";
            if (int.TryParse(chat.Name, out _))
                return "Chat name cannot be a number";
            Chats.Add(chat);
            SaveChanges();
            JoinChat(chat, chat.Owner);
            LoggingService.LogAsync($"{chat} added");
            return $"Chat added, id: {GetChat(chat.Name).Id}";
        }
        catch (Exception ex)
        {
            LoggingService.ErrorAsync(ex.Message);
            return ex.Message;
        }
    }
    public string DeleteChat(Chat chat)
    {
        try
        {
            if (!Chats.Any(u => u.Name == chat.Name)) 
                return "Chat not found";
            Chats.Remove(chat);
            SaveChanges();
            LoggingService.LogAsync($"{chat} deleted");
            return "Chat deleted";
        }
        catch (Exception ex)
        {
            LoggingService.ErrorAsync(ex.Message);
            return ex.Message;
        }
    }
    public string JoinChat(Chat chat, string user)
    {
        try
        {
            if (chat.Members.Any(u => u == user))
                return "User already in chat";
            chat.Members = chat.Members.Append(user).ToArray();
            SaveChanges();
            LoggingService.LogAsync($"{user} joined {chat}");
            SendMessage(chat, user, "User joined chat");
            return "User joined chat";
        }
        catch (Exception ex)
        {
            LoggingService.ErrorAsync(ex.Message);
            return ex.Message;
        }
    }
    public string LeaveChat(Chat chat, string user)
    {
        try
        {
            if (!chat.Members.Any(u => u == user))
                return "User not in chat";
            if (chat.Owner == user)
                return "Chat owner cannot leave the chat";
            SendMessage(chat, user, "User left chat");
            chat.Members = chat.Members.Where(u => u != user).ToArray();
            SaveChanges();
            LoggingService.LogAsync($"{user} left {chat}");
            return "User left chat";
        }
        catch (Exception ex)
        {
            LoggingService.ErrorAsync(ex.Message);
            return ex.Message;
        }
    }
    public string TransferOwnership(Chat chat, string user, string newOwner)
    {
        try
        {
            if (chat.Owner != user)
                return "User is not chat owner";
            if (!chat.Members.Any(u => u == newOwner))
                return "New owner must be a member of the chat";
            chat.Owner = newOwner;
            SaveChanges();
            LoggingService.LogAsync($"{user} transferred ownership of {chat}");
            SendMessage(chat, user, "Chat ownership transferred");
            return "Chat ownership transferred";
        }
        catch (Exception ex)
        {
            LoggingService.ErrorAsync(ex.Message);
            return ex.Message;
        }
    }
    public string SendMessage(Chat chat, string user, string message)
    {
        try
        {
            if (!chat.Members.Any(u => u == user))
                return "User not in chat";
            if (_user.GetTag(user) != "None")
                user += _user.GetTag(user) + user;

            chat.Messages = [.. chat.Messages, $"{user}: {message}"];
            
            SaveChanges();
            LoggingService.LogAsync($"{user} sent message to {chat}");
            return "Message sent";
        }
        catch (Exception ex)
        {
            LoggingService.ErrorAsync(ex.Message);
            return ex.Message;
        }
    }
}