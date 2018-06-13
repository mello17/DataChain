using System;
using System.Configuration;
using DataChain.WebApplication.Models;


namespace DataChain
{
   public class Program
    {
        

         static void Main(string[] args)
        {
            WebSocketBlockStream stream = 
                new WebSocketBlockStream(new Uri(ConfigurationSettings.AppSettings["endPoint"]));
            WebSocketServer socketServer = new WebSocketServer();
            socketServer.Start(ConfigurationSettings.AppSettings["endPoint"]).Wait();

            Console.ReadKey();


        }
    }
}
