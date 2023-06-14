using System.Data.SQLite;
using Dapper;
using Dapper.Contrib.Extensions;
using betabotLightness.DB.Entity;
using betabotLightness.Models.Enums;

namespace betabotLightness.DB.Repository;

/* Методы взаимодействия с таблицами 
 * по-хорошему, для каждой таблицы - свой репозиторий
 * но проект маленький и методов немного
 */
internal class UserRepository : IDisposable
{
    private readonly SQLiteConnection _connection;

    public UserRepository()
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
        return Environment.CurrentDirectory + "\\SimpleDimpleLite.sqlite";
    }

    public async Task InsertUsersAsync(User user)
    {
        await _connection.InsertAsync(user);
    }

    public async Task InsertUsersAsync(IEnumerable<User> user)
    {
        await _connection.InsertAsync(user);
    }

    public async Task<IEnumerable<User>> GetUsers()
    {
        return await _connection.QueryAsync<User>("SELECT * FROM Users");
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

    public async Task<User> GetUserByNamesAsync(string firstName, string lastName, string midName)
    {
        midName = midName.Trim();
        const string query =
            "SELECT * FROM Users WHERE (FirstName = @firstName) AND (LastName = @lastName) AND (MidName = @midName) AND (Role = 1)";
        return await _connection.QueryFirstOrDefaultAsync<User>(query, new { firstName, lastName, midName });
    }
    
    public async Task<int> GetUserIdByChatId(long chatId)
    {
        const string query = "SELECT Id FROM Users WHERE ChatId = @chatId";
        return await _connection.QueryFirstOrDefaultAsync<int>(query, new { chatId });
    }

    public async Task<IEnumerable<long>> GetChatIdsByTariff(Tariff tariff)
    {
        var tariffClause = tariff == Tariff.All ? string.Empty : "AND Tariff = @tariff";

        var query = $"SELECT ChatId FROM Users WHERE Role = 1 {tariffClause}";
        return await _connection.QueryAsync<long>(query, new { tariff });   
    }
}