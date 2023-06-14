using betabotLightness.DB.Entity;
using betabotLightness.DB.Repository;
using betabotLightness.Handlers;
using betabotLightness.Models;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using User = betabotLightness.DB.Entity.User;

/*
 * Реализация телеграмм-бота
 * с обработкой авторизации и бана
 */
var botClient = new TelegramBotClient("6007637356:AAGyFV6InyTCY5wGDTI207T1I7ttqOHIgus");

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
    var chatId = message.Chat.Id;
    var messageText = message.Text;
    if (message.Photo != null)
        return;
    /*    if (message.Photo == null)
            return;

        var hashSet = new HashSet<string>();
        foreach(var photo in message.Photo)
        {
            botClient.SendMediaGroupAsync
            var fileInfo = await botClient.GetFileAsync(photo.FileId);
            if (!hashSet.Contains(photo.FileId))
            {
                hashSet.Add(photo.FileId);
            }
        }
        if (!hashSet.Any())
            return;


        var list = new List<IAlbumInputMedia>();
        foreach (var fileId in hashSet)
        {
            list.Add(new InputMediaPhoto(new InputMedia(fileId)));
        }
        await botClient.SendMediaGroupAsync(chatId, list);
        return;*/


    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

    using var rep = new UserRepository();

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
                "Здравствуйте! Это бот проекта \"Лёгкость\". Введите свой токен для авторизации");
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

                var commandResponse = new CommandResponse
                {
                    TextMessage =
                    "Извините, Вас ещё нет в списке пользователей. Свяжитесь с администратором и пришлите Ваш токен"
                };
                await SendMessage(chatId, commandResponse, cancellationToken);
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
    switch (user.Role)
    {
        case betabotLightness.Models.Enums.Role.Client:
            return await userHandler.HandleCommandAsync(messageText, user);
        case betabotLightness.Models.Enums.Role.Admin:
            return await adminHandler.HandleCommandAsync(messageText, user);
        case betabotLightness.Models.Enums.Role.None:
        default:
            throw new NotImplementedException();
    }
}

async Task<CommandResponse> HandleUnauthorizeUser(User user, long chatId)
{
    switch (user.Role)
    {
        case betabotLightness.Models.Enums.Role.Client:
            return await userHandler.AuthorizeUser(chatId, user);
        case betabotLightness.Models.Enums.Role.Admin:
            return await adminHandler.AuthorizeUser(chatId, user);
        case betabotLightness.Models.Enums.Role.None:
        default:
            throw new NotImplementedException();
    }
}


async Task SendMessage(long chatId, CommandResponse commandResponse, CancellationToken cancellationToken)
{
    /*    if (commandResponse.Payload != null && commandResponse.Payload.Any())
        {
            await botClient.SendDocumentAsync(
                chatId,
                doc,
                parseMode: commandResponse.ParseMode,
                cancellationToken: cancellationToken
            );
        }*/


    if (commandResponse.ChatIds != null && commandResponse.ChatIds.Any())
    {
        foreach (var chatIdClient in commandResponse.ChatIds)
        {
            await botClient.SendTextMessageAsync(
          chatIdClient,
          commandResponse.TextMessage,
          commandResponse.ParseMode,
          cancellationToken: cancellationToken);
        }
    }
    else if (commandResponse.ReplyKeyboardMarkup != null)
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