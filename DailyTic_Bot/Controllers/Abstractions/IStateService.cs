using System.Collections.Generic;

namespace DailyTic_Bot.Controllers.Abstractions
{
    public interface IStateService
    {
        List<TelegramState> Get();
    }
}