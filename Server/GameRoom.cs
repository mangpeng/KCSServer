namespace Server;

public class GameRoom
{
    private List<ClientSession> _sessions = new List<ClientSession>();

    private object _lock = new object();

    public void Enter(ClientSession session)
    {
        lock (_lock)
        {
            _sessions.Add(session);
            session.Room = this;    
        }
    }

    public void Leave(ClientSession session)
    {
        lock (_lock)
        {
            _sessions.Remove(session);
        }
    }

    public void Broadcast(ClientSession session, string chat)
    {
        S2C_Chat packet = new S2C_Chat();
        packet.playerId = session.SessionId;
        packet.chat = $"{chat} I am {packet.playerId}";
        
        ArraySegment<byte> segment = packet.Write();

        lock (_lock)
        {
            foreach (var s in _sessions)
            {
                s.Send(segment);
            }
        }
    }
}