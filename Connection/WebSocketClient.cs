/**
* WebSocketClient.cs
* Author: Nguyen Xuan Truong
* Created at: 02/05/2022
*/

using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Event.LicenseEvent;

namespace Connection.WebSocketClient {
    class WebSocketClient {
        // private UTF8Encoding encoder; // For websocket text message encoding.
        private const UInt64 MAXREADSIZE = 1 * 1024 * 1024;

        private const string TYPE_KEY = "type";
        private const string EVENT_TYPE = "license";

        private const string EXTRAS_KEY = "extras";

        private const string OBJECTS_KEY = "objects";
        private const string LICENSE_KEY = "license";
        private const string CROP_KEY = "crop_id";
        private const string IMAGE_KEY = "image_id";


        private ClientWebSocket ws;
        public WebSocketClient() {
            this.ws = new ClientWebSocket();
        }
        public async Task connect(string urlServer) {
            Console.WriteLine("Connect server: {0}", new Uri(urlServer).ToString());
            if (urlServer.Contains("wss")) {
                // ws.Options.ClientCertificates = new X509CertificateCollection();
                ws.Options.RemoteCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
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
                    try {
                        JObject jEvent = JObject.Parse(result);
                        if ( (jEvent != null) && (jEvent.ContainsKey(TYPE_KEY)) && (jEvent.GetValue(TYPE_KEY).ToString().Equals(EVENT_TYPE)) ) {
                            JArray jObjects = JArray.Parse(jEvent.GetValue(EXTRAS_KEY).SelectToken(OBJECTS_KEY).ToString());
                            Console.WriteLine("Objects: {0}", jObjects);
                            if ( (jObjects != null) ) {
                                foreach (JObject jObject in jObjects.Children<JObject>() ) {
                                    Console.WriteLine("Object: {0}", jObject);
                                }   
                            }
                           
                        }
                    }
                    catch (System.Exception e) {
                        Console.WriteLine("Error: {0}", e);
                        throw;
                    }
                    
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