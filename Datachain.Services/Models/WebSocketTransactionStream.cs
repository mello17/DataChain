using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Net.WebSockets;
using System.Net;
using WebSocketSharp.Server;
using System.Web.WebSockets;
using DataChain.DataLayer;
using DataChain.DataLayer.Interfaces;
using DataChain.EntityFramework;

namespace DataChain.Services.Models 
{
    public class WebSocketTransactionStream : IHttpHandler //WebSocketBehavior,
    {
        private const int MaxMessageSize = 1024;

        private UriBuilder endpoint;
        private ClientWebSocket client = new ClientWebSocket();
        public WebSocketTransactionStream(Uri _endpoint)
        {
            this.endpoint = new UriBuilder(_endpoint);

            if (endpoint.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                this.endpoint.Scheme = "wss";
            else
                this.endpoint.Scheme = "ws";
        }

        public bool IsReusable { get { return false; } }

        public void  ProcessRequest(HttpContext context)
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
            ITransactionSubscriber store = new TransactionSubscriber();
            Transaction tx = await store.GetLastTransactionAsync();

            while (webSocket.State == WebSocketState.Open)
            {
                HexString rawTransactions;
                

                using (MemoryStream stream = new MemoryStream(MaxMessageSize))
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(receiveData, cancelationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, cancelationToken);
                    }
                    else
                    {
                        await stream.WriteAsync(receiveData.Array, receiveData.Offset, receiveData.Count);
                        stream.Seek(0, SeekOrigin.Begin);

                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            rawTransactions = new HexString(reader.ReadBytes((int)stream.Length));
                           
                        }
                       
                        await store.AddTransactionAsync(new[] { Serializer.TransactionDecode(rawTransactions) });
                    }

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
                

               // ProcessRequest(listener);
            }
        }
    }
}