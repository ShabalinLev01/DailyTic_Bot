using System;
using System.Linq;
using System.Threading.Tasks;
using DailyTic_Bot.Controllers.Abstractions;
using DailyTic_Bot.Controllers.Services;
using DailyTic_Bot.Models;
using Hangfire;
using Hors;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DailyTic_Bot.Controllers.Commands
{
    public class AddState : TelegramState
    {
        
        public BotContext db;
        private IBackgroundJobClient _jobClient;

        public override string Name { get; } = "Add";

        public override async Task Execute(Message message, ITelegramBotClient botClient, BotContext context,
            Update update, IBackgroundJobClient jobClient)
        {
            db = context;
            _jobClient = jobClient;
            var chatId = message.Chat.Id;
            if (message.Text.Contains("\U0001F50D Открыть лист напоминаний"))
            {
                await botClient.SendTextMessageAsync(chatId, "Список ваших напоминаний:"); 
                var api = JobStorage.Current.GetMonitoringApi();
                long c = JobStorage.Current.GetMonitoringApi().ScheduledCount();
                var scheduledJobs = api.ScheduledJobs(0, (int) c);
                string sendJobs="";
                int id = 1;
                foreach (var job in scheduledJobs)
                {
                    var jobThis = job.Value;
                    var index = (string) jobThis.Job.Arguments.GetValue(0);
                    var time = jobThis.EnqueueAt.AddHours(Convert.ToDouble(db.UtcTimes.FirstOrDefault(x => x.ChatId == chatId).UtcZone));
                    if (index.Contains(chatId.ToString()))
                    {
                        sendJobs += id + ")  " + jobThis.Job.Arguments.GetValue(1) +"  "+
                                    time.ToShortDateString() +" в "+ time.ToShortTimeString() + "\n";
                        id++;
                    }
                }
                if (sendJobs != "") await botClient.SendTextMessageAsync(chatId, sendJobs);
                else await botClient.SendTextMessageAsync(chatId, "Напоминания отсутсвуют");
            }
            else
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
                        if (result.Dates[0].DateFrom.ToShortTimeString() == "0:00" && result.Dates[0].DateTo.ToShortTimeString()  == "23:59")
                        {
                            date = date.AddHours(11);
                            var text = result.Text;
                            var formatText = result.TextWithTokens;
                            string day;
                            if (date.ToShortDateString() == timeUser.Date.ToShortDateString()) day = "сегодня";
                            else if (date.ToShortDateString() == timeUser.AddDays(1).Date.ToShortDateString()) day = "завтра";
                            else if (date.ToShortDateString() == timeUser.AddDays(2).Date.ToShortDateString()) day = "послезавтра";
                            else day = date.ToShortDateString();
                            await botClient.SendTextMessageAsync(chatId,
                                text: "Напомню вам " + day + " в " + date.ToShortTimeString(),
                                parseMode: ParseMode.Html, false, false, 0, keyBoard);
                            var delayDate = date - timeUser;
                            BackgroundJob.Schedule(
                                () => new NotificationJob(botClient).Send(message, text, date), delayDate);  
                        }
                        else
                        {
                            var text = result.Text;
                            var formatText = result.TextWithTokens;
                            string day;
                            if (result.Dates[0].DateFrom.Date.ToShortDateString() == timeUser.Date.ToShortDateString()) day = "сегодня";
                            else if (result.Dates[0].DateFrom.Date.ToShortDateString() == timeUser.AddDays(1).Date.ToShortDateString()) day = "завтра";
                            else if (result.Dates[0].DateFrom.Date.ToShortDateString() == timeUser.AddDays(2).Date.ToShortDateString()) day = "послезавтра";
                            else day = result.Dates[0].DateFrom.Date.ToShortDateString();
                            await botClient.SendTextMessageAsync(chatId,
                                text: "Напомню вам " + day + " в " + result.Dates[0].DateFrom.ToShortTimeString(),
                                parseMode: ParseMode.Html, false, false, 0, keyBoard);
                            var delayDate = date - timeUser;
                            BackgroundJob.Schedule(
                                () => new NotificationJob(botClient).Send(message, text, date), delayDate);
                        }
                                                
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, "Неправильный ввод даты или текста. Повторите ввод",
                            parseMode: ParseMode.Html, false, false, 0, keyBoard);
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "Неправильный ввод даты или текста. Повторите ввод",
                        parseMode: ParseMode.Html, false, false, 0, keyBoard);
                }
            }
        }
    }
}