using System.Collections.Concurrent;
using System.Text;
using scoring_counter_agent_bot.DB.Entity;
using scoring_counter_agent_bot.DB.Repository;
using scoring_counter_agent_bot.Models.Enums;
using scoring_counter_agent_bot.Parser;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace scoring_counter_agent_bot.Handlers;

/* Класс взаимодействий админ-пользователя
 * 
 * 
 */
internal class AdminHandler
{
    private readonly ConcurrentDictionary<long, LastCommand> _lastCommands = new();

    private readonly ReplyKeyboardMarkup _replyKeyboardMarkup =
        new(new[]
        {
            new KeyboardButton[] { "Проверить контрагента", "Проверить журнал" },
            new KeyboardButton[] { "Создать пользователя", "Создать администратора" }
        })
        {
            ResizeKeyboard = true
        };

    public async Task<CommandResponse> AuthorizeUser(long chatId, User user)
    {
        using var rep = new Repository();
        await rep.UpdateUserChatIdByUserIdAsync(chatId, user.Id);

        return new CommandResponse
        {
            TextMessage =
                $"Здравствуйте, {user.FirstName} {user.MidName}, Вы можете проверить контрагента, добавить пользователя или посмотреть чей-то журнал, для этого нажмите соответствующую кнопку.",
            ReplyKeyboardMarkup = _replyKeyboardMarkup
        };
        //1 - Вернуть сообщение
        //2 - Вернуть клавиатуру
        //3 - Установить ChatId для User 
    }

    public async Task<CommandResponse> HandleCommandAsync(string textMessage, User user)
    {
        if (textMessage == "/cancel")
        {
            _lastCommands.TryRemove(user.ChatId.Value, out _);
            var commandResponse = new CommandResponse
            {
                TextMessage = "Команда отменена",
                ReplyKeyboardMarkup = _replyKeyboardMarkup
            };
            return commandResponse;
        }

        ///Проверяем последнюю команду
        if (_lastCommands.TryGetValue(user.ChatId.Value, out var last))
        {
            var commandResponse = new CommandResponse();
            switch (last)
            {
                case LastCommand.CreateUser:
                    commandResponse = await CreateUser(textMessage);
                    _lastCommands.TryRemove(user.ChatId.Value, out _);
                    return commandResponse;
                    break;
                case LastCommand.CreateAdmin:
                    commandResponse = await CreateAdmin(textMessage);
                    _lastCommands.TryRemove(user.ChatId.Value, out _);
                    return commandResponse;
                    break;
                case LastCommand.CheckJournalByUser:
                    commandResponse = await CheckJournalByUser(textMessage);
                    _lastCommands.TryRemove(user.ChatId.Value, out _);
                    return commandResponse;
                    break;
                case LastCommand.CheckCounterAgent:
                    commandResponse = await CheckCounterAgent(textMessage);
                    _lastCommands.TryRemove(user.ChatId.Value, out _);
                    return commandResponse;
                    break;
            }
        }

        ///Обрабатываем default command
        switch (textMessage)
        {
            case "Создать пользователя":
                _lastCommands.TryAdd(user.ChatId.Value, LastCommand.CreateUser);
                return new CommandResponse
                {
                    TextMessage = "Введите полное ФИО пользователя"
                };
            case "Создать администратора":
                _lastCommands.TryAdd(user.ChatId.Value, LastCommand.CreateAdmin);
                return new CommandResponse
                {
                    TextMessage = "Введите полное ФИО пользователя"
                };
            case "Проверить журнал":
                _lastCommands.TryAdd(user.ChatId.Value, LastCommand.CheckJournalByUser);
                return new CommandResponse
                {
                    TextMessage = "Введите полное ФИО пользователя"
                };
            case "Проверить контрагента":
                _lastCommands.TryAdd(user.ChatId.Value, LastCommand.CheckCounterAgent);
                return new CommandResponse
                {
                    TextMessage = "Введите ИНН/ОГРН контрагента"
                };
            default:
                throw new Exception("Нет такой команды");
        }
    }

    //CreateUser -> HandleCommandAsync -> Program.cs
    private async Task<CommandResponse> CreateUser(string message)
    {
        var name = message.Split(' ');
        if (name.Length < 2) throw new Exception("Введите полное имя:");

        var midName = string.Empty;
        for (var i = 2; i < name.Length; i++)
            midName += name[i] + " ";

        using var rep = new Repository();
        var generatedToken = Guid.NewGuid().ToString("N");
        var user = new User
        {
            FirstName = name[1], LastName = name[0], MidName = midName.TrimEnd(' '), AdminRights = false,
            Token = generatedToken
        };
        await rep.InsertUsersAsync(user);

        var commandResponse = new CommandResponse();
        commandResponse.ParseMode = ParseMode.MarkdownV2;
        commandResponse.TextMessage = "||" + generatedToken + "||";
        commandResponse.ReplyKeyboardMarkup = _replyKeyboardMarkup;
        return commandResponse;
    }

    private async Task<CommandResponse> CreateAdmin(string message)
    {
        var name = message.Split(' ');
        if (name.Length < 2) throw new Exception("Введите полное имя");

        var midName = string.Empty;
        for (var i = 2; i < name.Length; i++)
            midName += name[i] + " ";

        using var rep = new Repository();
        var generatedToken = Guid.NewGuid().ToString("N");
        var user = new User
        {
            FirstName = name[1], LastName = name[0], MidName = midName.TrimEnd(' '), AdminRights = true,
            Token = generatedToken
        };
        await rep.InsertUsersAsync(user);

        var commandResponse = new CommandResponse();
        commandResponse.TextMessage = "|| " + generatedToken + " ||";
        commandResponse.ParseMode = ParseMode.MarkdownV2;
        commandResponse.ReplyKeyboardMarkup = _replyKeyboardMarkup;
        return commandResponse;
    }

    private async Task<CommandResponse> CheckJournalByUser(string message)
    {
        using var rep = new Repository();
        var name = message.Split(' ');
        if (name.Length < 2) throw new Exception("Введите полное имя");

        var midName = string.Empty;
        for (var i = 2; i < name.Length; i++)
            midName += name[i] + " ";
        var id = await rep.GetUserIdByNames(name[1], name[0], midName.Trim());

        if (id == 0) throw new Exception("Нет такого пользователя");

        var commandResponse = new CommandResponse();
        var result = await rep.GetJournalForUserById(id);
        if (result.Any())
        {
            var notesResulting = new StringBuilder();

            foreach (var note in result)
                notesResulting =
                    notesResulting.Append(
                        $"Название: {note.Name}, ИНН: {note.Inn}, ОГРН: {note.OGRN}, Результат: {note.CheckResult}, дата проверки: {note.CheckDate} \n");
            commandResponse.TextMessage = notesResulting.ToString();
            commandResponse.ReplyKeyboardMarkup = _replyKeyboardMarkup;
            return commandResponse;
        }

        commandResponse.TextMessage = "Пользователь не выполнил ни одной проверки";
        commandResponse.ReplyKeyboardMarkup = _replyKeyboardMarkup;
        return commandResponse;
    }

    private async Task<CommandResponse> CheckCounterAgent(string message)
    {
        var rep = new Reporter();
        var commandResponse = new CommandResponse();
        if (string.IsNullOrEmpty(message))
        {
            commandResponse.TextMessage = "Вы ничего не ввели";
            commandResponse.ReplyKeyboardMarkup = _replyKeyboardMarkup;
            return commandResponse;
        }

        if (message.Length < 10 || message.Length > 13)
        {
            commandResponse.TextMessage = "Вы ввели неправильный ИНН/ОГРН";
            commandResponse.ReplyKeyboardMarkup = _replyKeyboardMarkup;
            return commandResponse;
        }

        return await rep.GetResultChecking(message);
    }
}