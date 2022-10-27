namespace ServerCore;

public class SendBufferHelper
{
    public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => null);

    public static int ChunkSize { get; set; } = 65535 * 100;

    public static ArraySegment<byte> Open(int reserveSize)
    {
        if (CurrentBuffer.Value == null)
        {
            CurrentBuffer.Value = new SendBuffer(ChunkSize);
        }
        
        if (CurrentBuffer.Value.FreeSize < reserveSize)
        {
            CurrentBuffer.Value = new SendBuffer(ChunkSize);
        }

        return CurrentBuffer.Value.Open(reserveSize);
    }
    
    public static ArraySegment<byte> Close(int usedSize)
    {
        return CurrentBuffer.Value.Close(usedSize);
    }


}
public class SendBuffer
{
    // TLS를 이용하여 각자 자신의 스레드만 SendBufferChunk에 접근하긴 하지만
    // send를 호출한 스레드가 반드시 pendingList를 처리하는 것이 아니기 때문에
    // _buffer은 다수의 스레드가 참조할 수 있다. 
    // 그럼에도 write가 아니라 read 작업만 하기 때문에 문제는 없다.
    private byte[] _buffer;
    private int _usedSize = 0;
    
    public int FreeSize => _buffer.Length - _usedSize;

    public SendBuffer(int chunkSize)
    {
        _buffer = new byte[chunkSize];
    }

    public ArraySegment<byte> Open(int reserveSize)
    {
        if (reserveSize > FreeSize)
            return null;
        
        return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
    }
    
    public ArraySegment<byte> Close(int usedSize)
    {
        ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
        _usedSize += usedSize;

        return segment;
    } 
}