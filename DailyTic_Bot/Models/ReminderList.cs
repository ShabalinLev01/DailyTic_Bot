using System;
using System.ComponentModel.DataAnnotations;

namespace DailyTic_Bot.Models
{
    public class ReminderList
    {
        [Key]
        public int Id { get; set; }
        public int ChatId { get; set; }
        public string Reminder { get; set; }
        public DateTime? Time { get; set; }
    }
}