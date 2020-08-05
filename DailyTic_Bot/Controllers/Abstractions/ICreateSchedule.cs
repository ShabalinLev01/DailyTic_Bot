using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DailyTic_Bot.Controllers.JobsCommands
{
    public interface ICreateSchedule
    {
        Task SamplingTime(Message message, ReplyKeyboardMarkup keyBoard);
    }
}