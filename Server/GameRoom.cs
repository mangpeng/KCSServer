using ServerCore;

namespace Server;

public class GameRoom : IJobQueue
{
    private List<ClientSession> _sessions = new List<ClientSession>();
    private JobQueue _jobQueue = new JobQueue();
    private List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();


    public void Push(Action job)
    {
        _jobQueue.Push(job);    
    }
    
    public void Enter(ClientSession session)
    {
        _sessions.Add(session);
        session.Room = this;
    }

    public void Leave(ClientSession session)
    {
        _sessions.Remove(session);
    }

    // main thread 1군데서 실행된다.
    public void Flush()
    {
        // N ^ 2
        foreach (var s in _sessions)
        {
            s.Send(_pendingList);
        }

        Console.WriteLine($"Flushed {_pendingList.Count} items");
        _pendingList.Clear();
    }

    public void Broadcast(ClientSession session, string chat)
    {
        S2C_Chat packet = new S2C_Chat();
        packet.playerId = session.SessionId;
        packet.chat = $"{chat} I am {packet.playerId}";
        
        ArraySegment<byte> segment = packet.Write();

        // 패킷 모아 보내기 => main thread 에서 처리
        _pendingList.Add(segment);
        
        // N ^ 2
        // foreach (var s in _sessions)
        // {
        //     s.Send(segment);
        // }
    }
}