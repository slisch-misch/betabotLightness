using betabotLightness.Models.Enums;
using Dapper.Contrib.Extensions;
namespace betabotLightness.DB.Entity;

[Table("Users")]
public class User
{
    public int Id { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MidName { get; set; }
    public Role Role { get; set; }
    public DateTime Birthday { get; set; }
    public int Weight { get; set; }
    public int Height { get; set; }
    public Tariff Tariff { get; set; }
    public string Token { get; set; }    
    public long? ChatId { get; set; }
}

