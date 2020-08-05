using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DailyTic_Bot.Controllers.Abstractions;
using DailyTic_Bot.Controllers.JobsCommands;
using DailyTic_Bot.Models;
using Hangfire;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Char;

namespace DailyTic_Bot.Controllers.Commands
{
    public class OpenState : TelegramState
    {
        private BotContext _db;
        private ITelegramBotClient _botClient;
        private ReplyKeyboardMarkup _keyBoardExit;
        private Message _message;

        public override string Name { get; } = "Open";

        public override async Task Execute(Message message, ITelegramBotClient botClient, BotContext context,
            Update update, IBackgroundJobClient jobClient, IStateService stateService)
        {
                        
            _db = context;
            _botClient = botClient;
            _message = message;
            var keyBoardExit = new ReplyKeyboardMarkup
            {
                Keyboard = new[]
                {
                    new[]
                    {
                        new KeyboardButton("\U0001F50D Выйти в меню добавления")
                    }
                }
            };
            
            _keyBoardExit = keyBoardExit;

            var chatId = message.Chat.Id;
            if (message.Text.Contains("\U0001F50D Выйти в меню добавления"))
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
                await botClient.SendTextMessageAsync(chatId, "Сейчас вы можете добавлять напоминания.",
                    parseMode: ParseMode.Html, false, false, 0, keyBoard);
                _db.States.FirstOrDefault(x => x.ChatId == chatId).States = "Add";
                _db.SaveChanges();
            }
            else if (message.Text.Contains("\U0001F50D Открыть лист напоминаний"))
            {
                await ListOfRemider(chatId);
            }
            else if (message.Text.Contains(" удалить"))
            {
                await EditOrDelete(message, chatId, " удалить");
            }
            else if (message.Text.Contains(" изменить "))
            {
                await EditOrDelete(message, chatId, " изменить ");
            }
            else
            {
                await EditOrDelete(message, chatId, "Вы ввели что-то не то. Попробуйте снова или вернитесь к добавлению напоминаний.");
            }
        }

        private async Task ListOfRemider(long chatId)
        {
            _keyBoardExit.ResizeKeyboard = true;
            await _botClient.SendTextMessageAsync(chatId, "\U0001F4D3 Список ваших напоминаний:\U0001F4D3");
            var api = JobStorage.Current.GetMonitoringApi();
            long c = JobStorage.Current.GetMonitoringApi().ScheduledCount();
            var scheduledJobs = api.ScheduledJobs(0, (int) c);
            string sendJobs = "";
            int id = 1;
            foreach (var job in scheduledJobs)
            {
                var jobThis = job.Value;
                var index = (string) jobThis.Job.Arguments.GetValue(0);
                var time = jobThis.EnqueueAt.AddHours(
                    Convert.ToDouble(_db.UtcTimes.FirstOrDefault(x => x.ChatId == chatId).UtcZone));
                if (index.Contains(chatId.ToString()))
                {
                    sendJobs += id + ")  " + jobThis.Job.Arguments.GetValue(1) + "  " +
                                time.ToShortDateString() + " в " + time.ToShortTimeString() + "\n";
                    id++;
                }
            }
            if (sendJobs != "")
            {
                await _botClient.SendTextMessageAsync(chatId, sendJobs,ParseMode.Default, false, false, 0, _keyBoardExit);
            }
            else
            {
                await _botClient.SendTextMessageAsync(chatId, "\U0001F64CНапоминания отсутсвуют\U0001F64C", ParseMode.Default, false, false, 0, _keyBoardExit);
            }
            await _botClient.SendTextMessageAsync(chatId,
                "Выберите нужное напоминание для редактирование или удаления." +
                "\n\U00002705Пример удалить: '1 удалить'\U00002705" +
                "\nДля того, чтобы изменить - просто задайте новое напоминание." +
                "\n\U00002705'11 изменить Позвонить маме через 30 минут'\U00002705", ParseMode.Default, false, false, 0, _keyBoardExit);
        }

        private async Task EditOrDelete(Message message, long chatId, string choosePar)
        {
            int c = message.Text.IndexOf(choosePar);
            var numInText = message.Text.Remove(c, message.Text.Length - c);
            var newScheduledJob = message.Text.Remove(0, (c + choosePar.Length));;
            newScheduledJob = newScheduledJob.Trim();
            numInText = numInText.Trim();
            if (IsNumber(numInText, 0))
            {
                if(choosePar == " удалить") await Delete(chatId, Convert.ToInt32(numInText));
                else if(choosePar == " изменить ") await Edit(chatId, Convert.ToInt32(numInText), newScheduledJob);
                await ListOfRemider(chatId);
            }

        }

        private async Task Delete(long chatId, int userNumberJob)
        {
            var api = JobStorage.Current.GetMonitoringApi();
            long c = JobStorage.Current.GetMonitoringApi().ScheduledCount();
            var scheduledJobs = api.ScheduledJobs(0, (int) c);
            List<string> neededJob = new List<string>();
            int id = 1;
            foreach (var job in scheduledJobs)
            {
                var jobThis = job.Value;
                var index = (string) jobThis.Job.Arguments.GetValue(0);
                if (index.Contains(chatId.ToString()))
                {
                    neededJob.Add(job.Key);
                    id++;
                }
            }

            if (neededJob.Count >= userNumberJob)
            {
                BackgroundJob.Delete(neededJob[userNumberJob-1]);
            }
            else
            {
                await _botClient.SendTextMessageAsync(chatId, "\U00002757 Извините, такой позиции нету в вашем листе.\U00002757");
            }
        }
        
        private async Task Edit(long chatId, int userNumberJob, string newSchedule)
        {
            var api = JobStorage.Current.GetMonitoringApi();
            long c = JobStorage.Current.GetMonitoringApi().ScheduledCount();
            var scheduledJobs = api.ScheduledJobs(0, (int) c);
            List<string> neededJob = new List<string>();
            int id = 1;
            foreach (var job in scheduledJobs)
            {
                var jobThis = job.Value;
                var index = (string) jobThis.Job.Arguments.GetValue(0);
                if (index.Contains(chatId.ToString()))
                {
                    neededJob.Add(job.Key);
                    id++;
                }
            }

            if (neededJob.Count >= userNumberJob)
            {
                var editMesage = _message;
                editMesage.Text = newSchedule;
                BackgroundJob.Delete(neededJob[userNumberJob-1]);
                await new CreateSchedule(_botClient, _db).SamplingTime(editMesage, _keyBoardExit);
            }
            else
            {
                await _botClient.SendTextMessageAsync(chatId, "\U00002757 Извините, такой позиции нету в вашем листе.\U00002757");
            }
        }
    }
}
