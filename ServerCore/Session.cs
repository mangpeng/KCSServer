using System.Net.Sockets;
using System.Text;

namespace Server;

public class Session
{
    private Socket socket;

    public void Start(Socket socket)
    {
        this.socket = socket;
        SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
        recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
        // recvArgs.UserToken = 1; // 필요한 데이터 전달 가능.
        recvArgs.SetBuffer(new byte[1024], 0, 1024);
        
        RegisterRecv(recvArgs);
    }

    public void Send(byte[] sendBuff)
    {
        socket.Send(sendBuff);
    }

    public void Disconnect()
    {
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }

    #region 네트워크 통신

    void RegisterRecv(SocketAsyncEventArgs args)
    {
        bool pending = socket.ReceiveAsync(args);
        if (pending == false)
        {
            OnRecvCompleted(null, args);
        }
    }
    
    void OnRecvCompleted(object? sender, SocketAsyncEventArgs args)
    {
        if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
        {
            try
            {
                string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                Console.WriteLine($"[From Client] {recvData}");
                RegisterRecv(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }
        else
        {
            // todo : disconnect
        }
    }

    #endregion
 
}