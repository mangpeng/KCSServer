using System.Net;
using System.Net.Sockets;

namespace Server;

public class Listener
{
    private Socket _listenSocket;
    private Func<Session> _sessionFactory;

    public void Init(IPEndPoint endPoint, Func<Session> sessionFactory)
    {
        _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _sessionFactory = sessionFactory;
        
        _listenSocket.Bind(endPoint);
        _listenSocket.Listen(10);

        for (int i = 0; i < 10; i++)
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);    
        }
    }

    void RegisterAccept(SocketAsyncEventArgs args)
    {
        args.AcceptSocket = null;
        
        bool pending = _listenSocket.AcceptAsync(args);
        if (pending == false)
        {
            OnAcceptCompleted(null, args);
        }
    }

    // this function is red zone!!
    private void OnAcceptCompleted(object? sender, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success)
        {
            Session session = _sessionFactory();
            session.Start(args.AcceptSocket);
            
            // todo :여기서 클라가 접속을 끊으면 AcceptSocket에 접근이 불가능.. 크래쉬 여지 있음. 조치 필요
            session.OnConnected(args.AcceptSocket.RemoteEndPoint);
        }
        else
        {
            Console.WriteLine(args.SocketError.ToString());
        }
        
        RegisterAccept(args);
    }

    public Socket Accept()
    {
        return _listenSocket.Accept();
    }
}