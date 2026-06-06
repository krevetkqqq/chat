using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DbsClasses;

public class Chat
{
    public Chat(string owner, string name)
    {
        Owner = owner;
        Members = new string[0];
        Name = name;
    }
    public Chat() { }
    [Key]
    public int Id { get; set; }
    [JsonPropertyName("owner")]
    public string Owner { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    public string[] Members { get; set; } = [];
    public string[] Messages { get; set; } = [];
}