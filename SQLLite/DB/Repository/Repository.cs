using System.Data.SQLite;
using Dapper;
using Dapper.Contrib.Extensions;
using scoring_counter_agent_bot.DB.Entity;

namespace scoring_counter_agent_bot.DB.Repository;

/* Методы взаимодействия с таблицами 
 * по-хорошему, для каждой таблицы - свой репозиторий
 * но проект маленький и методов немного
 */
internal class Repository : IDisposable
{
    private readonly SQLiteConnection _connection;

    public Repository()
    {
        _connection = new SQLiteConnection("Data Source=" + GetDatabaseFile());
    }


    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    public static string GetDatabaseFile()
    {
        return Environment.CurrentDirectory + "\\SimpleDb.sqlite";
    }

    public async Task InsertUsersAsync(User user)
    {
        await _connection.InsertAsync(user);
    }

    public async Task InsertNoteAsync(Journal journal)
    {
        await _connection.InsertAsync(journal);
    }

    public async Task InsertUsersAsync(IEnumerable<User> user)
    {
        await _connection.InsertAsync(user);
    }

    public async Task<IEnumerable<User>> GetUsers()
    {
        return await _connection.QueryAsync<User>("SELECT * FROM Users");
    }

    public async Task<IEnumerable<Journal>> GetJournal()
    {
        return await _connection.QueryAsync<Journal>("SELECT * FROM Journal");
    }

    public async Task<int> GetUserId(string token)
    {
        const string query = "SELECT Id FROM Users WHERE Token = @token";
        return await _connection.QueryFirstOrDefaultAsync<int>(query, new { token });
    }

    public async Task<User> GetUserByChatIdAsync(long chatId)
    {
        const string query = "SELECT * FROM Users WHERE ChatId = @chatId";
        return await _connection.QueryFirstOrDefaultAsync<User>(query, new { chatId });
    }

    public async Task<User> GetUserByTokenAsync(string token)
    {
        const string query = "SELECT * FROM Users WHERE Token = @token";
        return await _connection.QueryFirstOrDefaultAsync<User>(query, new { token });
    }

    public async Task UpdateUserChatIdByUserIdAsync(long chatId, int userId)
    {
        const string query = "UPDATE Users SET ChatId = @chatId WHERE Id = @userId";
        await _connection.ExecuteAsync(query, new { chatId, userId });
    }

    public async Task<int> GetUserIdByNames(string firstName, string lastName, string midName)
    {
        const string query =
            "SELECT Id FROM Users WHERE (FirstName = @firstName) AND (LastName = @lastName) AND (MidName = @midName)";
        return await _connection.QueryFirstOrDefaultAsync<int>(query, new { firstName, lastName, midName });
    }

    public async Task<int> GetUserIdByChatId(long chatId)
    {
        const string query = "SELECT Id FROM Users WHERE ChatId = @chatId";
        return await _connection.QueryFirstOrDefaultAsync<int>(query, new { chatId });
    }

    public async Task<IEnumerable<Journal>> GetJournalForUserById(int userId)
    {
        const string query = @"SELECT j.* FROM Users u 
                        JOIN Journal j ON j.UserId = u.Id 
                        WHERE u.Id  = @userId";
        return await _connection.QueryAsync<Journal>(query, new { userId });
    }

    public async Task InsertHackerAsync(BruteForces hacker)
    {
        await _connection.InsertAsync(hacker);
    }

    public async Task<BruteForces> GetHackerByChatIdAsync(long chatId)
    {
        const string query = "SELECT * FROM BruteForcers WHERE ChatId = @chatId";
        return await _connection.QueryFirstOrDefaultAsync<BruteForces>(query, new { chatId });
    }

    public async Task<int> GetCounterByChatId(long chatId)
    {
        const string query = "SELECT Counter FROM BruteForcers WHERE ChatId = @chatId";
        return await _connection.QueryFirstOrDefaultAsync<int>(query, new { chatId });
    }

    public async Task IncreaseByOneCounterByChatId(long chatId, int counter)
    {
        const string query = "UPDATE BruteForcers SET Counter = @counter WHERE ChatId = @chatId";
        await _connection.ExecuteAsync(query, new { counter, chatId });
    }

    public async Task SetCounterToZeroByChatId(long chatId)
    {
        var counter = 0;
        const string query = "UPDATE Users SET Counter = @counter WHERE ChatId = @chatId";
        await _connection.ExecuteAsync(query, new { counter, chatId });
    }
}