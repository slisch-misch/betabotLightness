using System.Collections.Concurrent;
using System.Text;
using betabotLightness.DB.Entity;
using betabotLightness.DB.Repository;
using betabotLightness.Models;
using betabotLightness.Models.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace betabotLightness.Handlers;

/* Класс взаимодействий обычного пользователя
 * 
 * 
 */
internal class UserHandler
{
    private readonly ConcurrentDictionary<long, LastCommand> _lastCommands = new();

    public async Task<CommandResponse> AuthorizeUser(long chatId, User user)
    {
        using var rep = new UserRepository();
        await rep.UpdateUserChatIdByUserIdAsync(chatId, user.Id);

        return new CommandResponse
        {
            TextMessage =
                $"Здравствуйте, {user.FirstName} {user.MidName}, в этом чате вы будете получать сообщения от Евгения и его кураторов.",            
        };
        //1 - Вернуть сообщение
        //2 - Вернуть клавиатуру
        //3 - Установить ChatId для User 
    }

    public async Task<CommandResponse> HandleCommandAsync(string textMessage, User user)
    {
        ///Обрабатываем default command
        switch (textMessage)
        {
            default:
                throw new Exception("У обычного пользователя нет команд");
        }
    }
}