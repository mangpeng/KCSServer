using System.Net;
using Server;
using ServerCore;



public partial class Program
{
    static public GameRoom Room = new GameRoom();
    
    static public IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
    static public IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

    public static void Main()
    {
        Listener listener = new Listener();
        listener.Init(endPoint, () => SessionManager.Instance.Generate()); 
        Console.WriteLine("Listening....");
    
        while (true)
        {
            ;
        }    
    }
}
