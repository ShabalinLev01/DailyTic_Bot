using System.Collections.Generic;
using DailyTic_Bot.Controllers.Abstractions;
using DailyTic_Bot.Controllers.Commands;

namespace DailyTic_Bot.Controllers.Services
{
    public class CommandService: ICommandService
    {
        private readonly List<TelegramCommand> _commands;

        
        public CommandService()
        {
            _commands = new List<TelegramCommand>
            {
                new StartCommand()
            };
        }

        public List<TelegramCommand> Get() => _commands;
    }
}