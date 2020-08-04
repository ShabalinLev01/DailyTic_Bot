using System.ComponentModel.DataAnnotations;

namespace DailyTic_Bot.Models
{
    public class State
    {
        [Key]
        public int Id { get; set; }
        public int? ChatId { get; set; }
        public string? States { get; set; }
    }
}