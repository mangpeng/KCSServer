using ServerCore;

namespace Client.Packet;

class PacketHandler
{
    public static void S2C_ChatHandler(PacketSession session, IPacket packet)
    {
        S2C_Chat chatPacket = packet as S2C_Chat;
        ServerSession serverSession = session as ServerSession;
        
        // Console.WriteLine(chatPacket.chat);
    }
}
