// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Text;
using Server;

Listener listener = new Listener();
IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

listener.Init(endPoint, () => new GameSession());
Console.WriteLine("Listening....");
    
while (true)
{
    ;
}

class GameSession : Session
{
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected : {endPoint}");

        try
        {
            byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to Server");
            Send(sendBuff);
            Thread.Sleep(1000);
            Disconnect();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public override void OnRecv(ArraySegment<byte> buffer)
    {
        string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        Console.WriteLine($"[From Client] {recvData}");
    }

    public override void OnSend(int numOfBytes)
    {
        Console.WriteLine($"Transferred bytes : {numOfBytes}");
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnDisconnected : {endPoint}");
    }
}

