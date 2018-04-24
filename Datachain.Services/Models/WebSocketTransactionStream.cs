using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Net.WebSockets;
using System.Net;
using System.Web.WebSockets;

namespace DataChain.Services.Models
{
    public class WebSocketTransactionStream :IHttpHandler
    {
        private const int MaxMessageSize = 1024;

        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext context)
        {
            if (context.IsWebSocketRequest)
            {
                context.AcceptWebSocketRequest(WebSocketRequestHandler);
            }
        }

        public async Task WebSocketRequestHandler(AspNetWebSocketContext webSocketContext)
        {

            WebSocket webSocket = webSocketContext.WebSocket;
            
            ArraySegment<Byte> receiveData = new ArraySegment<Byte>(new Byte[MaxMessageSize]);
            var cancelationToken = new CancellationToken();

            while (webSocket.State == WebSocketState.Open)
            {
               
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(receiveData, cancelationToken);

                if(result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, cancelationToken);
                }
                else
                {
                    var payloadData = receiveData.Array.Where(x => x != 0).ToArray();
                    var receiveString = UTF8Encoding.UTF8.GetString(payloadData, 0, payloadData.Length);
                  //var bytes = UTF8Encoding.UTF8.GetBytes(receiveString);

                }
            }

        }

        public async Task ReadStream(string prefix)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();

              //  ProcessRequest(listener);
            }
        }
    }
}