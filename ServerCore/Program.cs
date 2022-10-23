// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Text;
using Server;

Listener listener = new Listener();

IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

void OnAcceptHandler(Socket clientSocket)
{
    try
    {
        Session session = new Session();
        session.Start(clientSocket);

        byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to Server");
        session.Send(sendBuff);
        
        Thread.Sleep(1000);
        session.Disconnect();
        session.Disconnect();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }

}

listener.Init(endPoint, OnAcceptHandler);
Console.WriteLine("Listening....");
    
while (true)
{
    ;
}