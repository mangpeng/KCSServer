using System.Net;
using System.Net.Sockets;

namespace Server;

public class Listener
{
    private Socket _listenSocket;
    private Action<Socket> _onAcceptHandler;

    public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
    {
        _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _onAcceptHandler = onAcceptHandler;
        
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
            _onAcceptHandler.Invoke(args.AcceptSocket);
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