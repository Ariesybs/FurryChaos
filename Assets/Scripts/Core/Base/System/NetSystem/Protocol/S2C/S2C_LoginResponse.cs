using System;

public class S2C_LoginResponse :S2CNetworkMsg
{
    public override ushort MsgType => (ushort)NetworkMessageType.S2C.LoginResponse;
    
    public override byte[] Encode()
    {
        return Array.Empty<byte>();
    }

    public override void Decode(byte[] payload)
    {
        
    }

    public override void Reset()
    {
        
    }
}