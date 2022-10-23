using System.Net.Sockets;
using System.Text;

namespace Server;

public class Session
{
    private Socket _socket;
    private int _disconnected = 0;

    private object _lock = new object();
    
    private Queue<byte[]> _sendQueue = new Queue<byte[]>();
    SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
    private bool _pending = false;
    
    public void Start(Socket socket)
    {
        this._socket = socket;
        
        SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
        recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
        // recvArgs.UserToken = 1; // 필요한 데이터 전달 가능.
        recvArgs.SetBuffer(new byte[1024], 0, 1024);
        RegisterRecv(recvArgs);
        
        _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
    }
    
    // Send는 Recv에 비해서 까다로움
    // 호출 시점이 언제가 될지 모르기 때문에
    public void Send(byte[] sendBuff)
    {
        lock (_lock)
        {
            _sendQueue.Enqueue(sendBuff);
            if (_pending == false)
            {
                RegisterSend();
            }
        }
    }
    
    public void Disconnect()
    {
        // 멀티스레드 환경에서 disconnect 두번 호출시 크래쉬 나기 때문에 조치
        if (Interlocked.Exchange(ref _disconnected, 1) == 1)
        {
            return;
        }
        
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
    }

    #region 네트워크 통신

    void RegisterSend()
    {
        _pending = true;
        byte[] buff = _sendQueue.Dequeue();
        _sendArgs.SetBuffer(buff, 0, buff.Length);
        
        bool pending = _socket.SendAsync(_sendArgs);
        if (pending == false)
        {
            OnSendCompleted(null, _sendArgs);
        }
    }

    void OnSendCompleted(object? sender, SocketAsyncEventArgs args)
    {
        // RegisterSend에 의해 직접 호출된 경우 Send(lock)->RegisterSend->OnSendCompted이기 때문에 lock 걸려 있어서 괜찮지만
        // args 콜백에 의해 호출되면 별도의 스레드가 수행 하므로 lock이 필요.
        lock (_lock)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    // OnSendCompleted 비동기로 수행되는 경우, 그 동안에 다른 스레드에 의해 sendqueue에 일감이 있을 수 있으므로
                    if (_sendQueue.Count > 0)
                    {
                        RegisterSend();
                    }
                    else
                    {
                        _pending = false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnSendCompleted Failed {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }
    }

    void RegisterRecv(SocketAsyncEventArgs args)
    {
        bool pending = _socket.ReceiveAsync(args);
        if (pending == false)
        {
            OnRecvCompleted(null, args);
        }
    }
    
    // 스레드 풀에서 임의 스레드가 꺠어나서 해당 호출을 처리한다.
    // 별도의 스레드가 처리하더라도 실제로 SocketAsyncEventArgs 하나이기 때문에 OnRecvCompleted를 처리하는 스레드는 하나다.
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
                Console.WriteLine($"OnRecvCompleted Failed{e}");
            }
            
        }
        else
        {
            Disconnect();
        }
    }

    #endregion
 
}