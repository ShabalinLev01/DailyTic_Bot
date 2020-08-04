using Microsoft.EntityFrameworkCore;

namespace DailyTic_Bot.Models
{
    public class BotContext : DbContext
    {
        public DbSet<UtcTime> UtcTimes { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<ReminderList> ReminderLists { get; set; }
        
        public BotContext(DbContextOptions<BotContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}