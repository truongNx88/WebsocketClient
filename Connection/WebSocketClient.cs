/**
* WebSocketClient.cs
* Author: Nguyen Xuan Truong
* Created at: 05/02/2022
*/

using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;

namespace Connection.WebSocketClient {
    class WebSocketClient {
        // private UTF8Encoding encoder; // For websocket text message encoding.
        private const UInt64 MAXREADSIZE = 1 * 1024 * 1024;
        private ClientWebSocket ws;
        public WebSocketClient() {
            this.ws = new ClientWebSocket();
        }
        public async Task connect(string urlServer) {
            Console.WriteLine("Connect server: {0}", new Uri(urlServer).ToString());
            if (urlServer.Contains("wss")) {
                ws.Options.ClientCertificates = new X509CertificateCollection();
            }
            await ws.ConnectAsync(new Uri(urlServer), CancellationToken.None);
            if (isConnecting()) {
                Console.WriteLine("Connected");
            }
        }

        
        #region [Receive]
    
        private async Task<string> receive(UInt64 maxSize = MAXREADSIZE)
        {
        // A read buffer, and a memory stream to stuff unknown number of chunks into:
            byte[] buf = new byte[4 * 1024];
            var ms = new MemoryStream();
            ArraySegment<byte> arrayBuf = new ArraySegment<byte>(buf);
            WebSocketReceiveResult chunkResult = null;
        
            if (isConnectionOpen())
            {
                do
                {
                chunkResult = await ws.ReceiveAsync(arrayBuf, CancellationToken.None);
                ms.Write(arrayBuf.Array, arrayBuf.Offset, chunkResult.Count);
                // Console.WriteLine("Size of Chunk message: " + chunkResult.Count);
                if ((UInt64)(chunkResult.Count) > MAXREADSIZE)
                {
                    Console.Error.WriteLine("Warning: Message is bigger than expected!");
                }
                } while (!chunkResult.EndOfMessage);
                ms.Seek(0, SeekOrigin.Begin);
        
                // Looking for UTF-8 JSON type messages.
                if (chunkResult.MessageType == WebSocketMessageType.Text) {
                    return streamToString(ms, Encoding.UTF8);
                }
            }
            return "";
        }
    
        public async Task receiveMessage () {
            Console.WriteLine("WebSocket Message Receiver looping.");
            string result;
            while (isConnectionOpen()) {
                result = await receive();
                if ((result != null) && (result.Length > 0)) {
                    Console.WriteLine("Result message: {0}", result);
                }
                else {
                    Task.Delay(50).Wait();
                }
            }
        }
    
        #endregion

        private static string streamToString(MemoryStream ms, Encoding encoding)
        {
        string readString = "";
        if (encoding == Encoding.UTF8)
        {
            using (var reader = new StreamReader(ms, encoding))
            {
            readString = reader.ReadToEnd();
            }
        }
        return readString;
        }

        public bool isConnecting()
        {
        return ws.State == WebSocketState.Connecting;
        }
    
        public bool isConnectionOpen()
        {
        return ws.State == WebSocketState.Open;
        }
    }
}