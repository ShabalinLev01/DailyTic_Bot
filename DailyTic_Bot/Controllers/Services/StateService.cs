using System.Collections.Generic;
using DailyTic_Bot.Controllers.Abstractions;
using DailyTic_Bot.Controllers.Commands;

namespace DailyTic_Bot.Controllers.Services
{
    public class StateService: IStateService
    {
        private readonly List<TelegramState> _states;

        
        public StateService()
        {
            _states = new List<TelegramState>
            {
                new UtcState(),
                new AddState(),
                new OpenState()
            };
        }

        public List<TelegramState> Get() => _states;
    }
}