using System;
using System.Net.WebSockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using DataChain.EntityFramework;
using System.Text;
using System.Linq;
using System.Web;

namespace DataChain.WebApplication.Models
{
    public abstract class ChainWebSocketHandler
    {
        protected ChainWebSocketManager WebSocketConnectionManager { get; set; }
        
        public ChainWebSocketHandler(ChainWebSocketManager webSocketConnectionManager)
        {
            WebSocketConnectionManager = webSocketConnectionManager;
        }

        public virtual async Task OnConnected(WebSocket socket)
        {
            WebSocketConnectionManager.AddSocket(socket);
        }

        public virtual async Task OnDisconnected(WebSocket socket)
        {
            await WebSocketConnectionManager.RemoveSocket(WebSocketConnectionManager.GetId(socket));
        }

        public async Task SendMessageAsync(WebSocket socket, string message)
        {
            if (socket.State != WebSocketState.Open)
                return;

            await socket.SendAsync(buffer: new ArraySegment<byte>(array: Serializer.ToBinaryArray(message),
                                                                  offset: 0,
                                                                  count: message.Length),
                                   messageType: WebSocketMessageType.Text,
                                   endOfMessage: true,
                                   cancellationToken: CancellationToken.None);
        }

    }
}