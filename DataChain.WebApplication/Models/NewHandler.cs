using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Web.WebSockets;
using System.Net.WebSockets;

namespace DataChain.WebApplication.Models
{
    public class BlockChainHandler : WebSocketHandler
    {
        private static WebSocketCollection clients = new WebSocketCollection();

        public override void OnOpen()
        {
            
            clients.Broadcast("Create new connect");
            clients.Add(this);
        }

        public override void OnMessage(string message)
        {
            Send("Echo: " + message);
        }


        public override void OnClose()
        {
            clients.Remove(this);
        }

        public static async Task Connect(string uri)
        {
            Thread.Sleep(1000);
            ClientWebSocket clientWebSocket = null;
            try
            {
                clientWebSocket = new ClientWebSocket();
                await clientWebSocket.ConnectAsync(new Uri(uri),CancellationToken.None);
            }
            catch
            {

            }

        } 

    }
}