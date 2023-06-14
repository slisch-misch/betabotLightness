using scoring_counter_agent_bot;
using scoring_counter_agent_bot.DB.Entity;
using scoring_counter_agent_bot.DB.Repository;
using scoring_counter_agent_bot.Handlers;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using User = scoring_counter_agent_bot.DB.Entity.User;

/*
 * Реализация телеграмм-бота
 * с обработкой авторизации и бана
 */
var botClient = new TelegramBotClient("5692922554:AAGBumgwJ9N0ODByxcwo_4fuz3y-rCb_RPc");

using var cts = new CancellationTokenSource();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};

var adminHandler = new AdminHandler();
var userHandler = new UserHandler();

botClient.StartReceiving(
    HandleUpdateAsync,
    HandlePollingErrorAsync,
    receiverOptions,
    cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Message is not { } message)
        return;

    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;

    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

    using var rep = new Repository();

    var user = await rep.GetUserByChatIdAsync(chatId);


    if (user != null)
    {
        try
        {
            var commandResponse = await HandleAuthorizeUser(message.Text, user);
            await SendMessage(chatId, commandResponse, cancellationToken);
        }
        catch (Exception ex)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                ex.Message,
                cancellationToken: cancellationToken);
        }
    }
    else
    {
        if (message.Text.ToLower() == "/start")
        {
            await botClient.SendTextMessageAsync(message.Chat,
                "Здравствуйте! Это бот для проверки контрагентов. Введите свой токен для авторизации");
            return;
        }

        try
        {
            user = await rep.GetUserByTokenAsync(messageText);
            if (user != null)
            {
                var commandResponse = await HandleUnauthorizeUser(user, chatId);
                await SendMessage(chatId, commandResponse, cancellationToken);
            }
            else
            {
                var hacker = await rep.GetHackerByChatIdAsync(chatId);
                if (hacker == null)
                {
                    hacker = new BruteForces { ChatId = chatId, Counter = 0 };
                    await rep.InsertHackerAsync(hacker);
                }

                hacker.Counter++;
                await rep.IncreaseByOneCounterByChatId(chatId, hacker.Counter);
                if (hacker.Counter < 10)
                {
                    var commandResponse = new CommandResponse
                    {
                        TextMessage =
                            "Извините, Вас ещё нет в списке пользователей. Свяжитесь с администратором и пришлите Ваш токен"
                    };
                    await SendMessage(chatId, commandResponse, cancellationToken);
                }
                else
                {
                    var commandResponse = new CommandResponse
                    {
                        TextMessage = "Вы забанены"
                    };
                    await SendMessage(chatId, commandResponse, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                ex.Message,
                cancellationToken: cancellationToken);
        }
    }
}


Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}

async Task<CommandResponse> HandleAuthorizeUser(string messageText, User user)
{
    switch (user.AdminRights)
    {
        case false:
            //throw new NotImplementedException();
            return await userHandler.HandleCommandAsync(messageText, user);
            break;
        case true:
            return await adminHandler.HandleCommandAsync(messageText, user);
            break;
    }
}

async Task<CommandResponse> HandleUnauthorizeUser(User user, long chatId)
{
    switch (user.AdminRights)
    {
        case false:
            return await userHandler.AuthorizeUser(chatId, user);
            //throw new NotImplementedException();
            break;
        case true:
            return await adminHandler.AuthorizeUser(chatId, user);
            break;
    }
}


async Task SendMessage(long chatId, CommandResponse commandResponse, CancellationToken cancellationToken)
{
    if (commandResponse.Payload != null && commandResponse.Payload.Any())
    {
        using var stream = new MemoryStream(commandResponse.Payload);
        var doc = new InputOnlineFile(stream, "GonReport.html");
        await botClient.SendDocumentAsync(
            chatId,
            doc,
            parseMode: commandResponse.ParseMode,
            cancellationToken: cancellationToken
        );
    }

    if (commandResponse.ReplyKeyboardMarkup != null)
        await botClient.SendTextMessageAsync(
            chatId,
            parseMode: commandResponse.ParseMode,
            text: commandResponse.TextMessage,
            replyMarkup: commandResponse.ReplyKeyboardMarkup,
            cancellationToken: cancellationToken);
    else
        await botClient.SendTextMessageAsync(
            chatId,
            commandResponse.TextMessage,
            commandResponse.ParseMode,
            cancellationToken: cancellationToken);
}