// See https://aka.ms/new-console-template for more information
using System.Net;
using Client;
using ServerCore;

// string host = Dns.GetHostName();
// IPHostEntry ipHost = Dns.GetHostEntry(host);
// IPAddress ipAddr = ipHost.AddressList[0];
IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

Connector connector = new Connector();
connector.Connect(endPoint, 
    () => SessionManager.Instance.Generate(),
    100);

while (true)
{
    SessionManager.Instance.SendForEach();
    
    Thread.Sleep(250);
}

