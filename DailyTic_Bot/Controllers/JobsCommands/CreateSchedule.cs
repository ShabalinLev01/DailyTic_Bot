using System;
using System.Linq;
using System.Threading.Tasks;
using DailyTic_Bot.Controllers.Services;
using DailyTic_Bot.Models;
using Hangfire;
using Hors;
using Hors.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DailyTic_Bot.Controllers.JobsCommands
{
    public class CreateSchedule : ICreateSchedule
    {
        private BotContext db;

        private ITelegramBotClient _botClient;
        private Message _message;
        private ReplyKeyboardMarkup _keyBoard;

        public CreateSchedule(ITelegramBotClient botClient, BotContext context)
        {
            _botClient = botClient;
            db = context;
        }

        public async Task SamplingTime(Message message, ReplyKeyboardMarkup keyBoard)
        {
            _message = message;
            _keyBoard = keyBoard;
            
            var chatId = message.Chat.Id;
            var timeUser = DateTime.UtcNow;
            
            var userUtc = db.UtcTimes.FirstOrDefault(x => x.ChatId == chatId).UtcZone;
            timeUser = timeUser.AddHours(Convert.ToDouble(userUtc));

            var horsTextParser = new HorsTextParser();
            var result = horsTextParser.Parse(message.Text, timeUser);

            if (result.Dates.Any() && result.Text != "")
            {
                if (result.Dates[0].DateFrom > timeUser)
                {
                    var date = result.Dates[0].DateFrom;
                    if (result.Dates[0].DateFrom.ToShortTimeString() == "0:00" &&
                        result.Dates[0].DateTo.ToShortTimeString() == "23:59")
                    {
                        await SendSchedule(result, date, timeUser);
                    }
                    else
                    {
                        await SendSchedule(result, date, timeUser);
                    }

                }
                else
                {
                    await _botClient.SendTextMessageAsync(chatId, "\U0000274CНеправильный ввод даты или текста. Повторите ввод\U0000274C",
                        parseMode: ParseMode.Html, false, false, 0, keyBoard);
                }
            }
            else
            {
                await _botClient.SendTextMessageAsync(chatId, "\U0000274CНеправильный ввод даты или текста. Повторите ввод\U0000274C",
                    parseMode: ParseMode.Html, false, false, 0, keyBoard);
            }
        }
        private async Task SendSchedule(HorsParseResult result, DateTime date, DateTime timeUser)
        {
            var text = result.Text;
            var formatText = result.TextWithTokens;
            string day;
            if (date.ToShortDateString() == timeUser.Date.ToShortDateString()) 
                day = "сегодня";
            else if (date.ToShortDateString() == timeUser.AddDays(1).Date.ToShortDateString())
                day = "завтра";
            else if (date.ToShortDateString() == timeUser.AddDays(2).Date.ToShortDateString())
                day = "послезавтра";
            else day = date.ToShortDateString();
            await _botClient.SendTextMessageAsync(_message.Chat.Id,
                text: "\U000023F0Напомню вам " + day + " в " + date.ToShortTimeString() + "\U000023F0",
                parseMode: ParseMode.Html, false, false, 0);
            var delayDate = date - timeUser;
            BackgroundJob.Schedule(
                () => new NotificationJob(_botClient).Send(_message, text, date), delayDate);
        }
    }
}