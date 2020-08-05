using System.Linq;
using System.Threading.Tasks;
using DailyTic_Bot.Controllers.Abstractions;
using DailyTic_Bot.Controllers.Commands;
using DailyTic_Bot.Controllers.Services;
using DailyTic_Bot.Models;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DailyTic_Bot.Controllers
{
    [ApiController]
    [Route("api/message/update")]
    public class BotController : Controller
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ICommandService _commandService;
        private readonly IStateService _stateService;
        private readonly IBackgroundJobClient _jobClient;
        private readonly BotContext _db;
        private INotificationJob _notificationJob;
        private string? _stateName;
        private bool timeLate = false;

        public BotController(ICommandService commandService, ITelegramBotClient telegramBotClient, BotContext context,
            IStateService stateService, IBackgroundJobClient jobClient, INotificationJob notificationJob)
        {
            _db = context;
            _commandService = commandService;
            _stateService = stateService;
            _jobClient = jobClient;
            _notificationJob = notificationJob;
            _telegramBotClient = telegramBotClient;
        }


        
        [HttpGet]
        public IActionResult Get()
        {

            return Ok("Started");
        }
        
         public async Task DbNotification(long chatId, string text)
        {
            await _telegramBotClient.SendTextMessageAsync(chatId, text);
        }
        
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            if (update == null)
            {

                return Ok();
            }
            if (update.Type == UpdateType.Message){
                var message = update.Message;
                var chatId = message.Chat.Id;
                //_jobClient.Enqueue(()=> new NotificationJob().Send(message));
                if (_db.UtcTimes.FirstOrDefault(x => x.ChatId == chatId) == null)
                {
                    await new StartCommand().Execute(message, _telegramBotClient, _db);
                    return Ok();
                }

                if (message.Text != "/start")
                {
                    if (_db.States.FirstOrDefault(x => x.ChatId == chatId) != null &&
                        _db.States.FirstOrDefault(x => x.ChatId == chatId).States != null)
                    {
                        foreach (var state in _stateService.Get())
                        {
                            _stateName = _db.States.FirstOrDefault(x => x.ChatId == chatId).States;
                            if (_stateName == state.Name)
                            {
                                await state.Execute(message, _telegramBotClient, _db, update, _jobClient, _stateService);
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (var command in _commandService.Get().Where(command => command.Contains(message)))
                        {
                            await command.Execute(message, _telegramBotClient, _db);
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var command in _commandService.Get().Where(command => command.Contains(message)))
                    {
                        await command.Execute(message, _telegramBotClient, _db);
                        break;
                    }
                }
                return Ok();
            }
            return Ok();
        }
    }
}