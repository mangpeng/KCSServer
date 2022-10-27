﻿using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore;

public abstract class PacketSession : Session
{
    //[size(2)][id(2)][.........]
    public static readonly int HeaderSize = 2;
    public sealed override int OnRecv(ArraySegment<byte> buffer)
    {
        int processLen = 0;
        int packetCount = 0;

        while (true)
        {
            if (buffer.Count < HeaderSize)
                break;

            ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            if (buffer.Count < dataSize)
                break;
            
            OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));
            ++packetCount;

            processLen += dataSize;
            buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
        }

        if (packetCount > 1)
        {
            Console.WriteLine($"패킷 모아받기 : {packetCount}");
        }
        
        return processLen;
    }

    public abstract void OnRecvPacket(ArraySegment<byte> buffer);
}


public abstract class Session
{
    private Socket _socket;
    private int _disconnected = 0;

    private RecvBuffer _recvBuffer = new RecvBuffer(65535);

    private object _lock = new object();
    private Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
    
    SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
    SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
    
    List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
    
    public abstract void OnConnected(EndPoint endPoint);
    public abstract int OnRecv(ArraySegment<byte> buffer);
    public abstract void OnSend(int numOfBytes);
    public abstract void OnDisconnected(EndPoint endPoint);


    void Clear()
    {
        lock (_lock)
        {
            _sendQueue.Clear();
            _pendingList.Clear();
        }
    }
    
    public void Start(Socket socket)
    {
        _socket = socket;
        
        _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
        _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
        
        RegisterRecv();
    }
    
    // Send는 Recv에 비해서 까다로움
    // 호출 시점이 언제가 될지 모르기 때문에
    
    // 개선점 :
    // - 패킷을 무조건 보내기 보다는 응답이 오지 않으면 패킷을 보내지 않고 대기 하는 등의 처리가 필요함
    //     클라를 해킹해서 악의적으로 서버에게 계속 보내는 경우...
    // - 패킷을 모아 보내기 작은 패킷을 여러개 보내기 보다, 큰 버퍼에 모아서 보내는 것 필요
    //    이러한 최적화는 server core에서도 할수 있으나 게임 컨텐츠단에서도 가능하다.
    public void Send(ArraySegment<byte> sendBuff)
    {
        lock (_lock)
        {
            _sendQueue.Enqueue(sendBuff);
            if (_pendingList.Count == 0)
            {
                RegisterSend();
            }
        }
    }
    
    public void Send(List<ArraySegment<byte>> sendBuffList)
    {
        // _sendArgs.BufferList 에 크기가 0이면 크래쉬 나므로
        if (sendBuffList.Count == 0)
            return;
        
        lock (_lock)
        {
            foreach (var s in sendBuffList)
            {
                _sendQueue.Enqueue(s);    
            }
            
            if (_pendingList.Count == 0)
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

        OnDisconnected(_socket.RemoteEndPoint);
        
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
        Clear();
    }

    #region 네트워크 통신

    void RegisterSend()
    {
        // thread-unsafe
        if (_disconnected == 1)
            return;
        
        // BufferList, SetBuffer 둘다 세팅하면 에러남
        // byte[] buff = _sendQueue.Dequeue();
        //_sendArgs.SetBuffer(buff, 0, buff.Length);
        
        while (_sendQueue.Count > 0)
        {
            ArraySegment<byte> buff = _sendQueue.Dequeue();
            _pendingList.Add(buff);
        }
        _sendArgs.BufferList = _pendingList;

        try
        {
            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
            {
                OnSendCompleted(null, _sendArgs);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"SendAsync : {e}");
            throw;
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
                    _sendArgs.BufferList = null;
                    _pendingList.Clear();

                    OnSend(_sendArgs.BytesTransferred);

                    // OnSendCompleted 비동기로 수행되는 경우, 그 동안에 다른 스레드에 의해 sendQueue에 일감이 있을 수 있으므로
                    if (_sendQueue.Count > 0)
                    {
                        RegisterSend();
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

    void RegisterRecv()
    {
        if (_disconnected == 1)
            return;
        
        _recvBuffer.Clean();
        var segment = _recvBuffer.WriteSegment;
        _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

        try
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false)
            {
                OnRecvCompleted(null, _recvArgs);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"ReceiveAsync : {e}");
            throw;
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
                // write 커서 이동
                if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                {
                    Disconnect();
                    return;
                }
                
                // 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리 했는지 받는다.
                int processLen = OnRecv(_recvBuffer.ReadSegment);
                if (processLen < 0 || _recvBuffer.DataSize < processLen)
                {
                    Disconnect();
                    return;
                }
                
                // read 커서 이동
                if (_recvBuffer.OnRead(processLen) == false)
                {
                    Disconnect();
                    return;
                }
                
                RegisterRecv();
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