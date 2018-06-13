using DataChain.Infrastructure;
using DataChain.Abstractions;
using DataChain.DataProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Text;
using NLog;

namespace DataChain.WebApplication.Models
{
    public class WebSocketServer
    {
        private readonly ILogger log;
        private readonly HttpListener httpListener;

        public WebSocketServer()
        {
            log = LogManager.GetCurrentClassLogger();
            httpListener = new HttpListener();
        }

        public async Task Start(string httpListenerPrefix)
        {
           
            httpListener.Prefixes.Add(httpListenerPrefix);
            httpListener.Start();
           
            while (httpListener.IsListening)
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
            string ipAddress;

            try
            {
                webSocketContext = await httpListenerContext.AcceptWebSocketAsync(subProtocol: null);
                ipAddress = httpListenerContext.Request.RemoteEndPoint.ToString();
                
            }
            catch(Exception )
            {
                httpListenerContext.Response.StatusCode = 500;
                httpListenerContext.Response.Close();
                return;
            }

            WebSocket webSocket = webSocketContext.WebSocket;
            try
            {
                byte[] receiveBuffer = new byte[256];
                ChainSerializer chainSerializer = new ChainSerializer();
                IEnumerable<Block> blocks;
                BlockRepository subscriber = new BlockRepository();
                ArraySegment<byte> segment = new ArraySegment<byte>(receiveBuffer);
                byte[] block = new byte[256];
                while (true)
                {

                    using (MemoryStream stream = new MemoryStream())
                    {
                        WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(
                        segment, CancellationToken.None);

                        if (receiveResult.MessageType == WebSocketMessageType.Close)
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        }
                        else
                        {
                            await stream.WriteAsync(segment.Array, segment.Offset, segment.Count);
                            stream.Seek(0, SeekOrigin.Begin);

                            do
                            {
                                using (BinaryReader reader = new BinaryReader(stream))
                                {
                                    block = reader.ReadBytes((int)stream.Length);
                                }
                            } while (!receiveResult.EndOfMessage);

                            blocks = chainSerializer.Decode(block);

                            if (blocks.Count() > 0)
                            {
                                var block_list = blocks.ToList();
                                var localBlocks = subscriber.GetBlocks();
                                if (localBlocks.Any(b => b.Hash == block_list[0].Hash))
                                {
                                    return;
                                }
                                else
                                {
                                    subscriber.AddBlock(block_list[0]);
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                log.Error("Error when adding block, reason: " + ex.Message);
                httpListenerContext.Response.StatusCode = 500;
                // httpListenerContext.Response.Close();
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

        public void Stop()
        {
            if (httpListener.IsListening)
            {
                httpListener.Close();
            }
        }

    }
}