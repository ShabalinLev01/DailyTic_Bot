using System.ComponentModel.DataAnnotations;

namespace DailyTic_Bot.Models
{
    public class UtcTime
    {
        [Key]
        public int Id { get; set; }
        public int? ChatId { get; set; }
        public string? UtcZone { get; set; }
    }
}