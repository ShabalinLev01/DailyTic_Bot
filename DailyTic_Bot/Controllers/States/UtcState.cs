using System;
using System.Linq;
using System.Threading.Tasks;
using DailyTic_Bot.Controllers.Abstractions;
using DailyTic_Bot.Models;
using Hangfire;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DailyTic_Bot.Controllers.Commands
{
    public class UtcState : TelegramState
    {
        public BotContext db;
        
        public override string Name { get; } = "Utc";

        public override async Task Execute(Message message, ITelegramBotClient botClient, BotContext context, Update update, IBackgroundJobClient jobClient)
        {
            db = context;
            var chatId = message.Chat.Id;
            var utcTime = message.Text;

            utcTime = utcTime.Trim();
            if (Convert.ToInt32(utcTime) <= 12 && Convert.ToInt32(utcTime) >= -12)
            {
                var keyBoard = new ReplyKeyboardMarkup
                {
                    Keyboard = new[]
                    {
                        new[]
                        {
                            new KeyboardButton("\U0001F50D Открыть лист напоминаний")
                        }
                    }
                };
                keyBoard.ResizeKeyboard = true;
                db.States.FirstOrDefault(x => x.ChatId == chatId).States = "Add";
                db.UtcTimes.FirstOrDefault(x => x.ChatId == chatId).UtcZone = utcTime;
                db.SaveChanges();
                await botClient.SendTextMessageAsync(chatId, "Сейчас вы можете добавлять напоминания.",
                    parseMode: ParseMode.Html, false, false, 0, keyBoard);
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, "Your UTC zone is uncorrect. Enter again");
            }
        }
    }
}