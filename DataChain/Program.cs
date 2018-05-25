using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using DataChain.WebApplication.Models;
using System.Web;


namespace DataChain
{
   public class Program
    {
        

         static void Main(string[] args)
        {
            WebSocketBlockStream stream = 
                new WebSocketBlockStream(new Uri(ConfigurationSettings.AppSettings["endPoint"]));
            WebSocketServer socketServer = new WebSocketServer();
            socketServer.Start(ConfigurationSettings.AppSettings["endPoint"]);


            while (true)
            {
                if (HttpContext.Current != null)
                stream.ProcessRequest(HttpContext.Current);
            }

          

        }
    }
}
