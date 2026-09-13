using System;
using System.IO;

public sealed class C2S_JoinRequest : C2SNetworkMsg
{
    public override ushort MsgType =>
        (ushort)NetworkMessageType.C2S.JoinRequest;

    public override byte[] Encode()
    {
        using var stream = new MemoryStream(32);
        using var writer = new BinaryWriter(stream);
        writer.Write(MsgType);
        return stream.ToArray();
    }

    public override void Decode(byte[] payload)
    {
        
    }

    public override void Reset()
    {
        
    }
}