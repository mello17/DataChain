using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Configuration;
using DataChain.WebApplication.Controllers;
using DataChain.WebApplication.Models;
using DataChain.DataProvider;
using DataChain.Infrastructure;
using DataChain.Abstractions.Interfaces;
using DataChain.WebApi.Models;
using Microsoft.AspNet.Builder;
using System.Data.Entity;
using System.Threading;

namespace DataChain.WebApplication
{
    public class WebApiApplication : System.Web.HttpApplication
    {

         List<Task> runningTasks = new List<Task>();
         
         BlockBuilder builder = new BlockBuilder(new BlockSubscriber(),new TransactionSubscriber());

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Database.SetInitializer(new MigrationInitializer());
           
            ControllerBuilder.Current.SetControllerFactory(
                new CustomControllerFactory());
            
            WebSocketBlockStream stream = new WebSocketBlockStream(new Uri(ConfigurationManager.AppSettings["endPoint"]));
            
            if(stream != null)
            {
                runningTasks.Add(stream.ProcessRequest(HttpContext.Current));
            }

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            DateTime startTime = RoundCurrentToNextFiveMinutes();

            Task timerTask = RunPeriodically( 
                startTime, TimeSpan.FromMinutes(5), tokenSource.Token);
            runningTasks.Add(builder.CompleteBlockAdding(CancellationToken.None));

            Task.WaitAll(runningTasks.ToArray(), tokenSource.Token);

        }

      private  async Task RunPeriodically( DateTime startTime, TimeSpan interval, CancellationToken token)
        {
            DateTime _nextRunTime = startTime;

            while (true)
            {
                TimeSpan delay = _nextRunTime - DateTime.UtcNow;

                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, token);
                }
                await builder.CompleteBlockAdding(CancellationToken.None);
               
                _nextRunTime += interval;
            }
        }

       private DateTime RoundCurrentToNextFiveMinutes()
        {
            DateTime now = DateTime.UtcNow,
                result = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);

            return result.AddMinutes(((now.Minute / 5) + 1) * 5);
        }


    }
}
