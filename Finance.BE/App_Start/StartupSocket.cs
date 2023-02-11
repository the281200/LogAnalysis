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
            /*RecurringJob.AddOrUpdate( () => order.RequestCheckContract(), "0 0 0 * * ?", TimeZoneInfo.Utc);
            RecurringJob.AddOrUpdate( () => order.RequestCheckNoti(), "0 0 0 * * ?", TimeZoneInfo.Utc);*/
            //RecurringJob.AddOrUpdate(() => order.GetDataFromLogFile(), "0 5 * * *", TimeZoneInfo.Local);
            app.UseHangfireServer();
        }
    }
}
