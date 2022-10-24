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

class Packet
{
    public ushort size;
    public ushort id;
}

class GameSession : Session
{
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected : {endPoint}");
        Packet packet = new Packet { size = 4, id = 7 };
        
        for (int i = 0; i < 5; i++)
        {
            var openSegment = SendBufferHelper.Open(4096);
            byte[] buff1 = BitConverter.GetBytes(packet.size);
            byte[] buff2 = BitConverter.GetBytes(packet.id);
            Array.Copy(buff1, 0, openSegment.Array, openSegment.Offset, buff1.Length);
            Array.Copy(buff2, 0, openSegment.Array, openSegment.Offset+ buff1.Length, buff2.Length);
            var sendBuff = SendBufferHelper.Close(packet.size);
            Send(sendBuff);    
        }
    }

    public override int OnRecv(ArraySegment<byte> buffer)
    {
        string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        Console.WriteLine($"[From Server] {recvData}");

        return buffer.Count;
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