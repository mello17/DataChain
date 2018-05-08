using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using Microsoft.AspNet.WebSockets.Server;
using Microsoft.AspNet.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using DataChain.DataLayer.Interfaces;


namespace Datachain.Services.Models
{
    public class WebSocketTransactionMiddleware
    {

        private RequestDelegate next;
        private ChainWebSocketHandler handler;
       

        public WebSocketTransactionMiddleware(RequestDelegate _next,
                                          ChainWebSocketHandler _handler)
        {
            next = _next;
            handler = _handler;
        }


        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return;

            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var store = (IBlockSubscriber)context.RequestServices.GetService(typeof(IBlockSubscriber));
            store.Init();
            await handler.OnConnected(webSocket);

            string blockAddress =  context.Request.Query["blockHash"];



        }

    }
}