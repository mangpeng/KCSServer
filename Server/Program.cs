using System.Net;
using Server;
using ServerCore;

PacketManager.Instance.Register();

Listener listener = new Listener();
IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

listener.Init(endPoint, () => new ClientSession());
Console.WriteLine("Listening....");
    
while (true)
{
    ;
}

