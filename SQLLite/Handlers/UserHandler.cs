using System.Collections.Concurrent;
using System.Text;
using scoring_counter_agent_bot.DB.Entity;
using scoring_counter_agent_bot.DB.Repository;
using scoring_counter_agent_bot.Models.Enums;
using scoring_counter_agent_bot.Parser;
using Telegram.Bot.Types.ReplyMarkups;

namespace scoring_counter_agent_bot.Handlers;

/* Класс взаимодействий обычного пользователя
 * 
 * 
 */
internal class UserHandler
{
    private readonly ConcurrentDictionary<long, LastCommand> _lastCommands = new();

    private readonly ReplyKeyboardMarkup replyKeyboardMarkup =
        new(new[]
        {
            new KeyboardButton[] { "Проверить контрагента", "Проверить журнал" }
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
                $"Здравствуйте, {user.FirstName} {user.MidName}, вы можете проверить контрагента или просмотреть свой журнал, для этого нажмите соответствующую кнопку.",
            ReplyKeyboardMarkup = replyKeyboardMarkup
        };
        //1 - Вернуть сообщение
        //2 - Вернуть клавиатуру
        //3 - Установить ChatId для User 
    }

    public async Task<CommandResponse> HandleCommandAsync(string textMessage, User user)
    {
        ///Проверяем последнюю команду
        if (_lastCommands.TryGetValue(user.ChatId.Value, out var last))
        {
            var commandResponse = new CommandResponse();
            switch (last)
            {
                case LastCommand.CheckJournalByUser:
                    commandResponse = await CheckJournalByUser(user);
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
            case "Проверить журнал":
                return await CheckJournalByUser(user);
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

    private async Task<CommandResponse> CheckJournalByUser(User user)
    {
        using var rep = new Repository();
        var commandResponse = new CommandResponse();
        var result = await rep.GetJournalForUserById(user.Id);
        if (result.Any())
        {
            var notesResulting = new StringBuilder();

            foreach (var note in result)
                notesResulting =
                    notesResulting.Append(
                        $"Название: {note.Name}, ИНН: {note.Inn}, ОГРН: {note.OGRN}, Результат: {note.CheckResult}, дата проверки: {note.CheckDate} \n");
            commandResponse.TextMessage = notesResulting.ToString();
            commandResponse.ReplyKeyboardMarkup = replyKeyboardMarkup;
            return commandResponse;
        }

        commandResponse.TextMessage = "Журнал пуст";
        return commandResponse;
    }

    private async Task<CommandResponse> CheckCounterAgent(string message)
    {
        var rep = new Reporter();
        var commandResponse = new CommandResponse();
        commandResponse.ReplyKeyboardMarkup = replyKeyboardMarkup;
        if (string.IsNullOrEmpty(message))
        {
            commandResponse.TextMessage = "Вы ничего не ввели";
            return commandResponse;
        }

        if (message.Length < 10 || message.Length > 13)
        {
            commandResponse.TextMessage = "Вы ввели неправильный ИНН/ОГРН";
            return commandResponse;
        }

        return await rep.GetResultChecking(message);
    }
}