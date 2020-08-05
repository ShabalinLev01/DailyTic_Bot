using System;
using System.Linq;
using System.Threading.Tasks;
using DailyTic_Bot.Controllers.Abstractions;
using DailyTic_Bot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DailyTic_Bot.Controllers.Commands
{
    public class StartCommand : TelegramCommand
    {
        private BotContext db;
        
        public override string Name => @"/start";

        public override bool Contains(Message message)
        {
            if (message.Type != MessageType.Text)
                return false;

            return message.Text.Contains(Name);
        }

        public override async Task Execute(Message message, ITelegramBotClient botClient, BotContext context)
        {
            db = context;
            var chatId = message.Chat.Id;
            
            if (db.UtcTimes.FirstOrDefault(x=>x.ChatId == chatId) == null)
            {
                State NewState = new State();
                NewState.ChatId = Convert.ToInt32(chatId);
                NewState.States = "Utc";
                db.States.Add(NewState);
                UtcTime NewUser = new UtcTime();
                NewUser.ChatId = Convert.ToInt32(chatId);
                NewUser.UtcZone = null;
                db.UtcTimes.Add(NewUser);
                db.SaveChanges();
                await botClient.SendTextMessageAsync(chatId,
                    "Привет! Вижу ты новый пользователь. Для дальнейшей работы тебе нужно ввести UTC(Всемирное " +
                    "координированное время) твоего региона. " +
                    "\n\nСейчас в UTC 0 = " +
                    DateTime.UtcNow.Hour + ":" + DateTime.UtcNow.Minute +
                    "!!!! \n\nПример если UTC 0 = 12:41, а твое время составляет " +
                    "14:41 введи '+2', а если 7:41 введи '-3'",
                    parseMode: ParseMode.Html, false, false, 0);
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
                
                db.States.FirstOrDefault(x => x.ChatId == chatId).States = "Add";
                db.SaveChanges();
                
                await botClient.SendTextMessageAsync(chatId, "\U0001F64CСейчас вы можете добавлять напоминания.\U0001F64C\n" +
                                                             "Примеры добавления:\n" +
                                                             "\U00002705'Набрать маме через 10 минут'\U00002705\n" +
                                                             "\U00002705'Поздравить Анатолия с Днём рождения 26 января'\U00002705\n" +
                                                             "\U00002705'Успеть купить продукты завтра вечером'\U00002705",
                    parseMode: ParseMode.Html, false, false, 0, keyBoard);
            }
        }
        
    }
}