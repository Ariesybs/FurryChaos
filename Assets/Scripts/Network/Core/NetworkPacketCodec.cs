using System;
using System.IO;
using UnityEngine;

namespace FurryChaos.Networking
{
    public static class NetworkPacketCodec
    {
        public const int ProtocolVersion = 1;

        public static byte[] EncodeJoinRequest()
        {
            using var stream = new MemoryStream(8);
            using var writer = new BinaryWriter(stream);
            writer.Write((byte)NetworkMessageType.JoinRequest);
            writer.Write(ProtocolVersion);
            return stream.ToArray();
        }

        public static byte[] EncodeJoinAccepted(long entityId)
        {
            using var stream = new MemoryStream(16);
            using var writer = new BinaryWriter(stream);
            writer.Write((byte)NetworkMessageType.JoinAccepted);
            writer.Write(entityId);
            return stream.ToArray();
        }

        public static byte[] EncodeInputFrame(in NetworkInputFrame input)
        {
            using var stream = new MemoryStream(48);
            using var writer = new BinaryWriter(stream);
            writer.Write((byte)NetworkMessageType.InputFrame);
            writer.Write(input.EntityId);
            writer.Write(input.Sequence);
            writer.Write(input.ClientTick);
            writer.Write(input.Direction.x);
            writer.Write(input.Direction.y);
            writer.Write(input.CameraYaw);
            writer.Write(input.PressedActions);
            writer.Write(input.HeldActions);
            return stream.ToArray();
        }

        public static byte[] EncodeSnapshot(in NetworkCharacterSnapshot snapshot)
        {
            using var stream = new MemoryStream(96);
            using var writer = new BinaryWriter(stream);
            writer.Write((byte)NetworkMessageType.CharacterSnapshot);
            writer.Write(snapshot.EntityId);
            writer.Write(snapshot.ServerTick);
            writer.Write(snapshot.LastProcessedInputSequence);
            Write(writer, snapshot.Position);
            Write(writer, snapshot.Rotation);
            Write(writer, snapshot.Velocity);
            writer.Write(snapshot.FsmState);
            writer.Write(snapshot.Gait);
            writer.Write(snapshot.IsGrounded);
            writer.Write(snapshot.JumpLinkId);
            writer.Write(snapshot.JumpPhase);
            writer.Write(snapshot.JumpNormalizedTime);
            return stream.ToArray();
        }

        public static bool TryGetMessageType(byte[] payload, out NetworkMessageType messageType)
        {
            if (payload == null || payload.Length == 0)
            {
                messageType = default;
                return false;
            }

            messageType = (NetworkMessageType)payload[0];
            return Enum.IsDefined(typeof(NetworkMessageType), messageType);
        }

        public static bool TryDecodeJoinRequest(byte[] payload, out int protocolVersion)
        {
            var value = 0;
            var success = TryRead(payload, NetworkMessageType.JoinRequest, reader =>
            {
                value = reader.ReadInt32();
            });
            protocolVersion = value;
            return success;
        }

        public static bool TryDecodeJoinAccepted(byte[] payload, out long entityId)
        {
            var value = 0L;
            var success = TryRead(payload, NetworkMessageType.JoinAccepted, reader =>
            {
                value = reader.ReadInt64();
            });
            entityId = value;
            return success;
        }

        public static bool TryDecodeInputFrame(byte[] payload, out NetworkInputFrame input)
        {
            var value = default(NetworkInputFrame);
            var success = TryRead(payload, NetworkMessageType.InputFrame, reader =>
            {
                value.EntityId = reader.ReadInt64();
                value.Sequence = reader.ReadUInt32();
                value.ClientTick = reader.ReadUInt32();
                value.Direction = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                value.CameraYaw = reader.ReadSingle();
                value.PressedActions = reader.ReadByte();
                value.HeldActions = reader.ReadByte();
            });
            input = value;
            return success;
        }

        public static bool TryDecodeSnapshot(byte[] payload, out NetworkCharacterSnapshot snapshot)
        {
            var value = default(NetworkCharacterSnapshot);
            var success = TryRead(payload, NetworkMessageType.CharacterSnapshot, reader =>
            {
                value.EntityId = reader.ReadInt64();
                value.ServerTick = reader.ReadUInt32();
                value.LastProcessedInputSequence = reader.ReadUInt32();
                value.Position = ReadVector3(reader);
                value.Rotation = ReadQuaternion(reader);
                value.Velocity = ReadVector3(reader);
                value.FsmState = reader.ReadByte();
                value.Gait = reader.ReadByte();
                value.IsGrounded = reader.ReadBoolean();
                value.JumpLinkId = reader.ReadInt32();
                value.JumpPhase = reader.ReadByte();
                value.JumpNormalizedTime = reader.ReadSingle();
            });
            snapshot = value;
            return success;
        }

        private static bool TryRead(byte[] payload, NetworkMessageType expectedType, Action<BinaryReader> read)
        {
            try
            {
                using var stream = new MemoryStream(payload, false);
                using var reader = new BinaryReader(stream);
                if ((NetworkMessageType)reader.ReadByte() != expectedType)
                    return false;

                read(reader);
                return stream.Position == stream.Length;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void Write(BinaryWriter writer, Vector3 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
        }

        private static void Write(BinaryWriter writer, Quaternion value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
            writer.Write(value.w);
        }

        private static Vector3 ReadVector3(BinaryReader reader)
            => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        private static Quaternion ReadQuaternion(BinaryReader reader)
            => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }
}
