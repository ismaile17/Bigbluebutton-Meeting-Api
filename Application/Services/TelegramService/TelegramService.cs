using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;

public class TelegramService
{
    private readonly TelegramBotClient _botClient;
    private readonly string _chatId;

    public TelegramService(string botToken, string chatId)
    {
        _botClient = new TelegramBotClient(botToken);
        _chatId = chatId;
    }

    public async Task SendMessageAsync(string message)
    {

        await _botClient.SendTextMessageAsync(
            chatId: _chatId,
            text: message,
            parseMode: ParseMode.Markdown
        );

    }
}
