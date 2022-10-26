namespace Client;

public class SessionManager
{
    private static SessionManager _session = new SessionManager();
    public static SessionManager Instance => _session;

    private List<ServerSession> _sessions = new List<ServerSession>();
    private object _lock = new object();

    public ServerSession Generate()
    {
        lock (_lock)
        {
            ServerSession session = new ServerSession();
            _sessions.Add(session);
            return session;
        }
    }

    public void SendForEach()
    {
        lock (_lock)
        {
            foreach (var s in _sessions)
            {
                C2S_Chat chatPacket = new C2S_Chat();
                chatPacket.chat = $"Hello Server!";
                ArraySegment<byte> segment = chatPacket.Write();
                
                s.Send(segment);
            }
        }
    }
}