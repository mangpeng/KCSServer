using ServerCore;

namespace Server;

public class GameRoom : IJobQueue
{
    private List<ClientSession> _sessions = new List<ClientSession>();

    private JobQueue _jobQueue = new JobQueue();
    

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

    public void Broadcast(ClientSession session, string chat)
    {
        S2C_Chat packet = new S2C_Chat();
        packet.playerId = session.SessionId;
        packet.chat = $"{chat} I am {packet.playerId}";
        
        ArraySegment<byte> segment = packet.Write();

        foreach (var s in _sessions)
        {
            s.Send(segment);
        }
    }
}