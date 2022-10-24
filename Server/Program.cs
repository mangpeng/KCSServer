using System.Net;
using System.Text;
using ServerCore;

Listener listener = new Listener();
IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

listener.Init(endPoint, () => new GameSession());
Console.WriteLine("Listening....");
    
while (true)
{
    ;
}

class Packet
{
    public ushort size;
    public ushort id;
}

class GameSession : PacketSession
{
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected : {endPoint}");

        // Packet packet = new Packet { size = 100, id = 10 };
        //
        // var openSegment = SendBufferHelper.Open(4096);
        // byte[] buff1 = BitConverter.GetBytes(packet.size);
        // byte[] buff2 = BitConverter.GetBytes(packet.id);
        // Array.Copy(buff1, 0, openSegment.Array, openSegment.Offset, buff1.Length);
        // Array.Copy(buff2, 0, openSegment.Array, openSegment.Offset+ buff1.Length, buff2.Length);
        // var sendBuff = SendBufferHelper.Close(buff1.Length + buff2.Length);
        // Send(sendBuff);
        
        Thread.Sleep(1000);
        Disconnect();
    }
    
    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2);

        Console.WriteLine($"RecvPacket {size} {id}");

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