using Owin;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.WebSockets.Server;
using Microsoft.AspNet.Http;
using DataChain.Infrastructures;
using DataChain.Services.Models;


namespace DataChain.Services
{
    public partial class Startup
    {
        private List<Task> runningTasks = new List<Task>();

        public void Configuration(IApplicationBuilder app, IAppBuilder webApi)
        {

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
             runningTasks.Add( new Task( ()=> {
                  stream.ProcessRequest(System.Web.HttpContext.Current);
              }));
            }




           
        }
    }
}