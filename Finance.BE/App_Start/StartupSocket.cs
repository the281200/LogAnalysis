using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;
using System;


[assembly: OwinStartup(typeof(WEB.App_Start.StartupSocket))]

namespace WEB.App_Start
{
    public class StartupSocket
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();

            Hangfire.GlobalConfiguration.Configuration
            .UseSqlServerStorage(
               "DefaultConnection");

            //var configRepository = System.Web.Http.GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IConfigRepository)) as ConfigRepository;
            var order = new WEB.Controllers.HomeController();

            app.UseHangfireDashboard("/testHangfire");
            RecurringJob.AddOrUpdate(() => order.GetDataFromLogFile(), "*/1 * * * *", TimeZoneInfo.Local);
            RecurringJob.AddOrUpdate(() => order.CheckDDOSAttack(), "*/2 * * * *", TimeZoneInfo.Local);
            RecurringJob.AddOrUpdate(() => order.CheckXSSAttack(), "*/2 * * * *", TimeZoneInfo.Local);
            RecurringJob.AddOrUpdate(() => order.CheckBruteForceAttack(), "*/2 * * * *", TimeZoneInfo.Local);
            app.UseHangfireServer();
        }
    }
}
