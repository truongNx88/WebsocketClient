namespace DemoWebsocket;
using Connection.WebSocketClient;

static class Program
{
    public static string wsUrl = "ws://localhost:10086";
    public static string wssUrl = "wss://socketsbay.com/wss/v2/2/demo/";
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        
        WebSocketClient client = new WebSocketClient();
        // Task connection = Task.Run(() => client.connect("wss://socketsbay.com/wss/v2/2/demo/"));
        Task connection = client.connect(wssUrl);
        connection.Wait();
        Task receiveMessage = Task.Run( () => client.receiveMessage());
        receiveMessage.Wait();

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        // ApplicationConfiguration.Initialize();
        // Application.Run(new Form1());
    }    
}