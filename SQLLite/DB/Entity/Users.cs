using Dapper.Contrib.Extensions;

namespace scoring_counter_agent_bot.DB.Entity;

/* Таблица для хранения юзеров
 * думаю, тут всё понятно
 * 
 */
[Table("Users")]
public class User
{
    public int Id { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MidName { get; set; }
    public string Token { get; set; }
    public bool AdminRights { get; set; }
    public long? ChatId { get; set; }
}