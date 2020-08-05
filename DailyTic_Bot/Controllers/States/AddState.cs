using System;
using System.Linq;
using System.Threading.Tasks;
using DailyTic_Bot.Controllers.Abstractions;
using DailyTic_Bot.Controllers.JobsCommands;
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
            Update update, IBackgroundJobClient jobClient, IStateService stateService)
        {
            db = context;
            _jobClient = jobClient;
            var chatId = message.Chat.Id;
            if (message.Text.Contains("\U0001F50D Открыть лист напоминаний"))
            {
                db.States.FirstOrDefault(x => x.ChatId == chatId).States = "Open";
                db.SaveChanges();
                await new OpenState().Execute(message, botClient, db, update, _jobClient, stateService);
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

                await new CreateSchedule(botClient, db).SamplingTime(message, keyBoard);
            }
        }
    }
}