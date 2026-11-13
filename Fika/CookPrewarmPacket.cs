using Fika.Core.Networking.LiteNetLib;
using Fika.Core.Networking.LiteNetLib.Utils;

namespace Aero.Grenade.Fika
{
    public struct CookPrewarmPacket : INetSerializable
    {
        public int NetId;
        public float CookTime;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(NetId);
            writer.Put(CookTime);
        }

        public void Deserialize(NetDataReader reader)
        {
            NetId = reader.GetInt();
            CookTime = reader.GetFloat();
        }

        public static void OnReceived(CookPrewarmPacket packet, NetPeer peer)
        {
            FikaCookNetwork.StoreRemoteCook(packet.NetId, packet.CookTime);
        }
    }
}
