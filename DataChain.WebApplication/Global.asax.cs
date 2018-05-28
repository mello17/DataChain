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
using DataChain.WebApi.Models;
using Microsoft.AspNet.Builder;
using System.Data.Entity;
using System.Threading;

namespace DataChain.WebApplication
{
    public class WebApiApplication : System.Web.HttpApplication
    {

         List<Task> runningTasks = new List<Task>();

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

            BlockBuilder builder = new BlockBuilder();
            runningTasks.Add(builder.CompleteBlockAdding(CancellationToken.None));

        }

       
    }
}
