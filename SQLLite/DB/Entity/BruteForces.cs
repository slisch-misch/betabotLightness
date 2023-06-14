using Dapper.Contrib.Extensions;

namespace scoring_counter_agent_bot.DB.Entity;

/*
 * Таблица для счётчика на количество неудачных входов
 * по ней в program.cs банятся пользователи
 */
[Table("BruteForcers")]
internal class BruteForces
{
    public int Id { get; set; }
    public long? ChatId { get; set; }
    public int Counter { get; set; }
}