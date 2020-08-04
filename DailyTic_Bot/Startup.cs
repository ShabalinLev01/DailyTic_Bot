using System;
using DailyTic_Bot.Controllers.Abstractions;
using DailyTic_Bot.Controllers.Services;
using DailyTic_Bot.Models;
using DevelopersGame.Web;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;

namespace DailyTic_Bot
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            string connection = _configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<BotContext>(options =>
                options.UseSqlServer(connection));
            services
                .AddTelegramBotClient(_configuration)
                .AddScoped<ICommandService, CommandService>()
                .AddScoped<IStateService, StateService>()
                .AddScoped<INotificationJob, NotificationJob>()
                .AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                })
                .AddFluentValidation();
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseSqlServerStorage(_configuration.GetConnectionString("HangfireConnection"),
                    new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero
                    }));

            // Add the processing server as IHostedService
           services.AddHangfireServer();
           
           
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider, IBackgroundJobClient backgroundJobClient)
        {
            app.UseDeveloperExceptionPage();
            app.UseRouting();
            app.UseHangfireDashboard();
            app.UseHangfireServer();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}