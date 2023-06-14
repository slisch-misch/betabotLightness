using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace betabotLightness.Models;
/* Класс-модель для вывода сообщений, возврата клавиатуры и форматирования сообщений
 * 
 * 
 */

internal class CommandResponse
{
    public string TextMessage { get; set; }
    public ReplyKeyboardMarkup ReplyKeyboardMarkup { get; set; }
    public ParseMode? ParseMode { get; set; }
    public byte[] Payload { get; set; }
    public IEnumerable<long> ChatIds { get; set; }
}