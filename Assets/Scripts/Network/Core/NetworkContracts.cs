using System;
using UnityEngine;

namespace FurryChaos.Networking
{
    public enum NetworkLaunchMode : byte
    {
        Disabled,
        Server,
        Client
    }

    public enum NetworkRole : byte
    {
        Standalone,
        LocalPredicted,
        RemoteProxy,
        ServerAuthority
    }

    public enum NetworkMessageType : byte
    {
        JoinRequest = 1,
        JoinAccepted = 2,
        InputFrame = 3,
        CharacterSnapshot = 4
    }

    [Serializable]
    public struct NetworkInputFrame
    {
        public long EntityId;
        public uint Sequence;
        public uint ClientTick;
        public Vector2 Direction;
        public float CameraYaw;
        public byte PressedActions;
        public byte HeldActions;
    }

    [Serializable]
    public struct NetworkCharacterSnapshot
    {
        public long EntityId;
        public uint ServerTick;
        public uint LastProcessedInputSequence;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public byte FsmState;
        public byte Gait;
        public bool IsGrounded;
        public int JumpLinkId;
        public byte JumpPhase;
        public float JumpNormalizedTime;
    }

    public interface INetworkTransport : IDisposable
    {
        bool IsRunning { get; }
        bool IsServer { get; }

        event Action<int> Connected;
        event Action<int> Disconnected;
        event Action<int, byte[]> DataReceived;

        bool StartServer(ushort port);
        bool StartClient(string address, ushort port);
        void Poll();
        void Send(int connectionId, byte[] payload);
        void Broadcast(byte[] payload);
        void Stop();
    }
}
