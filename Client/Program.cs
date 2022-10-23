// See https://aka.ms/new-console-template for more information


using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using ServerCore;

// string host = Dns.GetHostName();
// IPHostEntry ipHost = Dns.GetHostEntry(host);
// IPAddress ipAddr = ipHost.AddressList[0];
IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

Connector connector = new Connector();
connector.Connect(endPoint, () => new GameSession());

while (true)
{
    
}

class GameSession : Session
{
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected : {endPoint}");
        
        for (int i = 0; i < 5; i++)
        {
            byte[] sendBuff = Encoding.UTF8.GetBytes($"hello world {i}");
            Send(sendBuff);    
        }
    }

    public override void OnRecv(ArraySegment<byte> buffer)
    {
        string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        Console.WriteLine($"[From Server] {recvData}");
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