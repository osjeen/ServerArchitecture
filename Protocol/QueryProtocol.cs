using System;
using System.IO;
using System.Text;

public class QueryProtocol
{
    public static byte[] Serialize(PacketData data)
    {
        if(data is ClientAssign_PacketData data_ca){
            using (MemoryStream ms = new MemoryStream())
            {
            using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8))
            {
                writer.Write(data_ca.QueryType);
                writer.Write(data_ca.path);
            }
            return ms.ToArray();
            }
        }else if(data is DivisionClients_PacketData data_dc)
        {
            using (MemoryStream ms = new MemoryStream())
            {
            using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8))
            {
                writer.Write(data_dc.QueryType);
                writer.Write(data_dc.path);
            }
            return ms.ToArray();
            }
        }

        return null;
    }
}

public enum PacketType : byte
{
    None = 0,
    ClientAssign = 1,
    DivisionClients = 2
}

public class PacketData
{
    public virtual byte QueryType => 0;
}

public class ClientAssign_PacketData : PacketData
{
    public override byte QueryType => 1;
    public string path;
}

public class DivisionClients_PacketData : PacketData
{
    public override byte QueryType => 2;
    public string path;
}