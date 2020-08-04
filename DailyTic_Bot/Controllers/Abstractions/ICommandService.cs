using System.Collections.Generic;

namespace DailyTic_Bot.Controllers.Abstractions
{
    public interface ICommandService
    {
        List<TelegramCommand> Get();
    }
}