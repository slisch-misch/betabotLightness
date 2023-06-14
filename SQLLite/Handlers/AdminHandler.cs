using System.Collections.Concurrent;
using betabotLightness.DB.Entity;
using betabotLightness.DB.Repository;
using betabotLightness.Extensions;
using betabotLightness.Models;
using betabotLightness.Models.Enums;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace betabotLightness.Handlers;

/* Класс взаимодействий админ-пользователя
 * 
 * 
 */
internal class AdminHandler
{
    private const int MinNumberLinePersonCard = 5;

    private readonly ConcurrentDictionary<long, LastCommand> _lastCommands = new();

    private readonly ReplyKeyboardMarkup _replyKeyboardMarkup =
        new(new[]
        {
            new KeyboardButton[] { "Создать пользователя", "Создать администратора", "Показать карточку пользователя" }
        })
        {
            ResizeKeyboard = true
        };

    public async Task<CommandResponse> AuthorizeUser(long chatId, User user)
    {
        using var rep = new UserRepository();
        await rep.UpdateUserChatIdByUserIdAsync(chatId, user.Id);

        return new CommandResponse
        {
            TextMessage =
                $"Здравствуйте, {user.FirstName} {user.MidName}, вы можете добавить пользователя, для этого нажмите соответствующую кнопку. Чтобы отправить сообщение всем введите /message и введите сообщение, чтобы оно было с отложенной отправкой выберите \"Отправить позже\" и выберите дату+время😎",
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

        if (textMessage.StartsWith("/message"))
        {
            return await HandleMessageCommandAsync(textMessage, user);
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
                case LastCommand.CreateAdmin:
                    commandResponse = await CreateAdmin(textMessage);
                    _lastCommands.TryRemove(user.ChatId.Value, out _);
                    return commandResponse;       
                case LastCommand.ShowClientCard:
                    commandResponse = await ShowClientCard(textMessage);
                    _lastCommands.TryRemove(user.ChatId.Value, out _);
                    return commandResponse;
            }
        }

        ///Обрабатываем default command
        switch (textMessage)
        {
            case "Создать пользователя":
                _lastCommands.TryAdd(user.ChatId.Value, LastCommand.CreateUser);
                return new CommandResponse
                {
                    TextMessage = "Введите данные клиента в формате: \nБыков Никита Андреевич\n65кг\n172см\n27.06.2000\n1\nТарифы записываются в формате 1, 2 или 3 по повышению стоимости"
                };
            case "Создать администратора":
                _lastCommands.TryAdd(user.ChatId.Value, LastCommand.CreateAdmin);
                return new CommandResponse
                {
                    TextMessage = "Введите данные администратора в формате: \nБыков Никита Андреевич"
                };
            case "Показать карточку пользователя":
                _lastCommands.TryAdd(user.ChatId.Value, LastCommand.ShowClientCard);
                return new CommandResponse
                {
                    TextMessage = "Введите ФИО пользователя в формате: \nБыков Никита Андреевич"
                };
            default:
                throw new Exception("Нет такой команды");
        }
    }

    //CreateUser -> HandleCommandAsync -> Program.cs
    private async Task<CommandResponse> CreateUser(string message)
    {
        var personCard = message.Split("\n");
        if (personCard.Length < MinNumberLinePersonCard) 
            throw new Exception("Введены некорректные данные");
        var name = personCard[0].Split(" ");

        var midName = string.Empty;
        for (var i = 2; i < name.Length; i++)
            midName += name[i] + " ";


        if (!int.TryParse(personCard[1][..^2], out var weight))
            throw new Exception("Введен некорректный вес");
        if (!int.TryParse(personCard[2][..^2], out var height))
            throw new Exception("Введен некорректные рост");
        if (!DateTime.TryParse(personCard[3], out var dateBirth))
            throw new Exception("Введена некорректная дата рождения");
        if (!int.TryParse(personCard[4], out var tariff))
            throw new Exception("Введен некорректный тариф");

        using var rep = new UserRepository();
        var generatedToken = Guid.NewGuid().ToString("N");
        var user = new User
        {
            FirstName = name[1],
            LastName = name[0],
            MidName = midName.TrimEnd(' '),
            Weight = weight,
            Height = height,
            Birthday = dateBirth,
            Role = Role.Client,
            Tariff = (Tariff)tariff,
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
        var personCard = message.Split(' ');
        if (personCard.Length < 2)
            throw new Exception("Введены некорректные данные");

        var midName = string.Empty;
        for (var i = 2; i < personCard.Length; i++)
            midName += personCard[i] + " ";

        using var rep = new UserRepository();
        var generatedToken = Guid.NewGuid().ToString("N");
        var user = new User
        {
            FirstName = personCard[1],
            LastName = personCard[0],
            MidName = midName.TrimEnd(' '),
            Role = Role.Admin,
            Token = generatedToken
        };

        await rep.InsertUsersAsync(user);
        var commandResponse = new CommandResponse
        {
            TextMessage = "|| " + generatedToken + " ||",
            ParseMode = ParseMode.MarkdownV2,
            ReplyKeyboardMarkup = _replyKeyboardMarkup
        };
        return commandResponse;
    }

    private async Task<CommandResponse> ShowClientCard(string message)
    {
        var fullName = message.Split(' ');
        if (fullName.Length < 2)
            throw new Exception("Введены некорректные данные");

        var midName = string.Empty;
        for (var i = 2; i < fullName.Length; i++)
            midName += fullName[i] + " ";

        using var userRep = new UserRepository();

        var user = await userRep.GetUserByNamesAsync(fullName[1], fullName[0], midName);
        

        var years = (DateTime.Now.Year - user.Birthday.Year);

        var mess = $"🥷*ФИО*: {user.LastName} {user.FirstName} {user.MidName}\n";
        mess+= $"💰*Тариф*: {user.Tariff.GetDisplayName()}";              
        mess += $"\n📆*День рождения*: {user.Birthday:dd.MM.yyyy}\n";
        mess += $"🍰*Возраст*: {years}\n";
        mess += $"🍕*Вес*: {user.Weight} кг\n";
        mess += $"🚏*Рост*: {user.Height} см\n";
        

        var commandResponse = new CommandResponse
        {
            TextMessage = mess,
            ParseMode = ParseMode.Markdown,
            ReplyKeyboardMarkup = _replyKeyboardMarkup
        };
        return commandResponse;
    }


    private async Task<CommandResponse> HandleMessageCommandAsync(string textMessage, User user)
    {
        _lastCommands.TryRemove(user.ChatId.Value, out _);
        using var userRep = new UserRepository();

        var tariff = textMessage switch
        {
            _ when textMessage.Contains("Lite") => Tariff.Light,
            _ when textMessage.Contains("Standart") => Tariff.Standart,
            _ when textMessage.Contains("Max") => Tariff.Max,
            _ when textMessage.Contains("All") => Tariff.All,
            _ => Tariff.None
        };
        var chatIds = await userRep.GetChatIdsByTariff(tariff);

        return new CommandResponse
        {
            TextMessage = string.Join(" ",textMessage.Split(" ").Skip(1)),
            ReplyKeyboardMarkup = _replyKeyboardMarkup,
            ChatIds = chatIds
        };
    }

}