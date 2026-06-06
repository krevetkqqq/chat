using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DbsClasses;

public class Chat
{
    public Chat(User owner)
    {
        Owner = owner;
        Members = new User[0];
    }
    public Chat() { }
    [Key]
    public int Id { get; set; }
    [JsonPropertyName("user")]
    public User Owner { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    public User[] Members { get; set; } = [];
    public string[] Messages { get; set; } = [];
}