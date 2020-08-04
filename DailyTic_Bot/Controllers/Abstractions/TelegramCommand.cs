using DailyTic_Bot.Models;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DailyTic_Bot.Controllers.Abstractions
{
    public abstract class TelegramCommand
    {
        public abstract string Name { get; }

        public abstract Task Execute(Message message, ITelegramBotClient client, BotContext context);

        public abstract bool Contains(Message message);
    }
}