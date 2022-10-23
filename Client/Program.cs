// See https://aka.ms/new-console-template for more information


using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

// string host = Dns.GetHostName();
// IPHostEntry ipHost = Dns.GetHostEntry(host);
// IPAddress ipAddr = ipHost.AddressList[0];
IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

while (true)
{
    Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

    try
    {
        socket.Connect(endPoint);
        Console.WriteLine($"Connected To {socket.RemoteEndPoint.ToString()}");

        for (int i = 0; i < 5; i++)
        {
            byte[] sendBuff = Encoding.UTF8.GetBytes($"hello world {i}");
            int sendBytes = socket.Send(sendBuff);    
        }

        byte[] recvBuff = new byte[1024];
        int recvBytes = socket.Receive(recvBuff);
        string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
        Console.WriteLine($"[From Server] {recvData}");
        
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
    
    Thread.Sleep(100);
}