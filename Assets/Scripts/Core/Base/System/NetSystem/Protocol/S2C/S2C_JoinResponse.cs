using System;
using System.IO;

public sealed class S2C_JoinResponse : S2CNetworkMsg
{
    public override ushort MsgType =>
        (ushort)NetworkMessageType.S2C.JoinResponse;

    public int ConnectionId;
    public override byte[] Encode()
    {
        using var stream = new MemoryStream(32);
        using var writer = new BinaryWriter(stream);
        writer.Write(MsgType);
        writer.Write(ConnectionId);
        return stream.ToArray();
    }

    public override void Decode(byte[] payload)
    {
        try
        {
            using var stream = new MemoryStream(payload, false);
            using var reader = new BinaryReader(stream);
            if (reader.ReadUInt16() != MsgType)
            {
                return;
            }

            ConnectionId = reader.ReadInt32();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public override void Reset()
    {
        
    }
}