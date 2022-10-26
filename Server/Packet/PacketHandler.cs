﻿using ServerCore;

namespace Server.Packet;

class PacketHandler
{
    public static void C2S_ChatHandler(PacketSession session, IPacket packet)
    {
        C2S_Chat chatPacket = packet as C2S_Chat;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null)
            return;

        GameRoom room = clientSession.Room;
        room.Push(()=>room.Broadcast(clientSession, chatPacket.chat));
    }
}   
