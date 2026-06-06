using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DbsClasses;

public class User
{
    public User(string name, string password)
    {
        Name = name;
        Password = password;
    }
    public User() { }
    [Key]
    public int Id { get; set; }
    [JsonPropertyName("user")]
    public string Name { get; set; }
    [JsonPropertyName("password")]
    public string Password { get; set; }
    [JsonPropertyName("admin")]
    public string AdminStatus = "None";
    [JsonPropertyName("utcplus")]
    public int HoursAdd = 0;
}