using System.IO;
using UnityEngine;

public class C2S_CatInputRequest : C2SNetworkMsg
{
    public override ushort MsgType => (ushort)NetworkMessageType.C2S.CatInput;
    public uint Sequence;
    public uint ClientTick;
    public Vector2 Direction;
    public float CameraYaw;
    public byte PressedActions;
    public byte HeldActions;
    

    public override byte[] Encode()
    {
        using var stream = new MemoryStream(32);
        using var writer = new BinaryWriter(stream);
        writer.Write(MsgType);
        writer.Write(Sequence);
        writer.Write(ClientTick);
        NetworkUtils.WriteVector2(writer, Direction);
        writer.Write(CameraYaw);
        writer.Write(PressedActions);
        writer.Write(HeldActions);
        return stream.ToArray();
    }

    public override void Decode(byte[] payload)
    {
        try
        {
            Reset();
            using var stream = new MemoryStream(payload, false);
            using var reader = new BinaryReader(stream);
            if (reader.ReadUInt16() != MsgType)
            {
                return;
            }
            Sequence = reader.ReadUInt32();
            ClientTick = reader.ReadUInt32();
            Direction = NetworkUtils.ReadVector2(reader);
            CameraYaw = reader.ReadSingle();
            PressedActions = reader.ReadByte();
            HeldActions = reader.ReadByte();
        }
        catch
        {
            Reset();
        }
    }

    public override void Reset()
    {
        Sequence = 0;
        ClientTick = 0;
        Direction = Vector2.zero;
        CameraYaw = 0;
        PressedActions = 0;
        HeldActions = 0;
    }
}