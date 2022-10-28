// See https://aka.ms/new-console-template for more information
using System.Net;
using Client;
using ServerCore;

// string host = Dns.GetHostName();
// IPHostEntry ipHost = Dns.GetHostEntry(host);
// IPAddress ipAddr = ipHost.AddressList[0];
IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
IPEndPoint endPoint = new IPEndPoint(ipAddr, 1234);

Connector connector = new Connector();
connector.Connect(endPoint, 
    () => SessionManager.Instance.Generate(),
    10);

while (true)
{
    SessionManager.Instance.SendForEach();
    
    Thread.Sleep(250);
}

