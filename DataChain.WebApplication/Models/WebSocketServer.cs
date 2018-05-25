using DataChain.Infrastructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace DataChain.WebApplication.Models
{
    public class WebSocketServer
    {
        private static List<Task> runningTasks = new List<Task>();

        public async void Start(string httpListenerPrefix)
        {
            HttpListener httpListener = new HttpListener();
            httpListener.Prefixes.Add(httpListenerPrefix);
            httpListener.Start();

            while (true)
            {
                HttpListenerContext httpListenerContext = await httpListener.GetContextAsync();
                if (httpListenerContext.Request.IsWebSocketRequest)
                {
                    ProccessRequest(httpListenerContext);
                }
                else
                {
                    httpListenerContext.Response.StatusCode = 400;
                    httpListenerContext.Response.Close();
                }
            }
        }

        private async void ProccessRequest(HttpListenerContext httpListenerContext)
        {
            WebSocketContext webSocketContext = null;
            try
            {
                webSocketContext = await httpListenerContext.AcceptWebSocketAsync(subProtocol: null);
                string ipAddress = httpListenerContext.Request.RemoteEndPoint.ToString();
            }
            catch(Exception ex)
            {
                httpListenerContext.Response.StatusCode = 500;
                httpListenerContext.Response.Close();
                return;
            }

            WebSocket webSocket = webSocketContext.WebSocket;
            try
            {
                byte[] receiveBuffer = new byte[1024];
                ChainSerializer chainSerializer = new ChainSerializer();
                ChainConnector connector = new ChainConnector();
                receiveBuffer = chainSerializer.Encode(connector.GetLocalChain().BlockChain);
                while (webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    }
                    else
                    {
                        await webSocket.SendAsync(new ArraySegment<byte>(receiveBuffer, 0, receiveResult.Count)
                            , WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {

                httpListenerContext.Response.StatusCode = 500;
                httpListenerContext.Response.Close();
                return;
            }

            finally
            {
                if (webSocket != null)
                {
                    webSocket.Dispose();
                }
            }


        } 

    }
}