using System.Net;
using Server;
using ServerCore;



public partial class Program
{
    static public GameRoom Room = new GameRoom();
    
    static public IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
    static public IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

    static void FlushRoom()
    {
        Room.Push(()=>Room.Flush());
        JobTimer.Instance.Push(FlushRoom, 250);
    }
    
    public static void Main()
    {
        Listener listener = new Listener();
        listener.Init(endPoint, () => SessionManager.Instance.Generate()); 
        Console.WriteLine("Listening....");

        // todo 더 작은 시간 단위의 jobqueue 어떻게 관리할 것인지 
        //FlushRoom();
        JobTimer.Instance.Push(FlushRoom);
        
        while (true)
        {
            JobTimer.Instance.Flush();
        }    
    }
}
