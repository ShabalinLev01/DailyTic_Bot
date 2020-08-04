using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DailyTic_Bot.Controllers.Services
{
    public interface INotificationJob
    {
        Task Send(Message message, string text, DateTime time);
    }

    public class NotificationJob : INotificationJob
    {
        private readonly ITelegramBotClient _botClient;
        
        public NotificationJob(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task Send(Message message, string text, DateTime time)
        {
            await _botClient.SendTextMessageAsync(message.Chat.Id, text);
        }
    }
}