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
using System.Web.WebSockets;
using Microsoft.Web.WebSockets;
using DataChain.DataLayer;
using DataChain.Infrastructures;

namespace DataChain.WebApplication.Models 
{
    public class WebSocketBlockStream 
    {
        private const int MaxMessageSize = 1024;
        
        private readonly UriBuilder endpoint;
      
        public WebSocketBlockStream(Uri _endpoint)
        {
            this.endpoint = new UriBuilder(_endpoint);

            if (endpoint.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                this.endpoint.Scheme = "wss";
            else
                this.endpoint.Scheme = "ws";
        }


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

            while (webSocket.State == WebSocketState.Open)
            {
                HexString rawBlockChain;
                

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
                            rawBlockChain = new HexString(reader.ReadBytes((int)stream.Length));
                           
                        }

                        ChainSerializer serializer = new ChainSerializer();
                        serializer.Decode(rawBlockChain.ToByteArray());
                    }

                }
            }

        }

      
    }
}