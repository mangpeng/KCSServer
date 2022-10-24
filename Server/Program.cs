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

class Knight
{
    public int hp;
    public int attack;
}

class GameSession : Session
{
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected : {endPoint}");

        Knight knight = new Knight { hp = 100, attack = 10 };
        
        var openSegment = SendBufferHelper.Open(4096);
        byte[] buff1 = BitConverter.GetBytes(knight.hp);
        byte[] buff2 = BitConverter.GetBytes(knight.attack);
        Array.Copy(buff1, 0, openSegment.Array, openSegment.Offset, buff1.Length);
        Array.Copy(buff2, 0, openSegment.Array, openSegment.Offset+ buff1.Length, buff2.Length);
        
        var sendBuff = SendBufferHelper.Close(buff1.Length + buff2.Length);
        Send(sendBuff);
        
        Thread.Sleep(1000);
        Disconnect();
    }
    
    public override int OnRecv(ArraySegment<byte> buffer)
    {
        string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        Console.WriteLine($"[From Client] {recvData}");

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