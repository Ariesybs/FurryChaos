using System.IO;
using UnityEngine;

public sealed class S2C_CatSnapshot : S2CNetworkMsg
{
    public override ushort MsgType => (ushort)NetworkMessageType.S2C.CatSnapshot;
    
    public long EntityId;
    public uint ServerTick;
    public uint LastProcessedInputSequence;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Velocity;
    public byte MovementState;
    public bool IsGrounded;
    
    public override byte[] Encode()
    {
        using var stream = new MemoryStream(64);
        using var writer = new BinaryWriter(stream);
        writer.Write(MsgType);
        writer.Write(EntityId);
        writer.Write(ServerTick);
        writer.Write(LastProcessedInputSequence);
        NetworkUtils.WriteVector3(writer, Position);
        NetworkUtils.WriteQuaternion(writer, Rotation);
        NetworkUtils.WriteVector3(writer, Velocity);
        writer.Write(MovementState);
        writer.Write(IsGrounded);
        return stream.ToArray();
    }

    public override void Decode(byte[] payload)
    {
        Reset();
        if (payload == null || payload.Length == 0)
        {
            return;
        }
        try
        {
            using var stream = new MemoryStream(payload, false);
            using var reader = new BinaryReader(stream);
            if (reader.ReadUInt16() != MsgType)
            {
                return;
            }
            EntityId = reader.ReadInt64();
            ServerTick = reader.ReadUInt32();
            LastProcessedInputSequence = reader.ReadUInt32();
            Position = NetworkUtils.ReadVector3(reader);
            Rotation = NetworkUtils.ReadQuaternion(reader);
            Velocity = NetworkUtils.ReadVector3(reader);
            MovementState = reader.ReadByte();
            IsGrounded = reader.ReadBoolean();
            // 不接受带有多余或缺失数据的消息。
            if (stream.Position != stream.Length)
            {
                Reset();
            }
        }
        catch
        {
            Reset();
        }
    }

    public override void Reset()
    {
        EntityId = 0;
        ServerTick = 0;
        LastProcessedInputSequence = 0;
        Position = Vector3.zero;
        Rotation = Quaternion.identity;
        Velocity = Vector3.zero;
        MovementState = 0;
        IsGrounded = false;
    }
}