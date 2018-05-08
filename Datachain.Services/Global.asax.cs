﻿using System.Web;
using System.Data.Entity;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using DataChain.EntityFramework;
using Owin;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.WebSockets.Server;
using Microsoft.AspNet.Http;
using DataChain.Infrastructures;
using DataChain.Services.Models;

namespace Datachain.Services
{
    public class WebApiApplication : HttpApplication
    {
        private List<Task> runningTasks = new List<Task>();

        protected void Application_Start(IApplicationBuilder app, IAppBuilder webApi)
        {

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Database.SetInitializer(new MigrationInitializer());

            app.UseWebSockets();

            app.Map("/chain", webSocketsApps =>
            {
                webSocketsApps.UseWebSockets();
            });

            HttpConfiguration config = new HttpConfiguration();
            config.EnsureInitialized();


            webApi.UseWebApi(config);
            app.ApplicationServices.GetService(typeof(TransactionValidator));

            WebSocketTransactionStream stream = (WebSocketTransactionStream)app.ApplicationServices.GetService(typeof(WebSocketTransactionStream));

            if (stream != null)
            {
                runningTasks.Add(new Task(() => {
                    stream.ProcessRequest(System.Web.HttpContext.Current);
                }));
            }



        }




    }
}
