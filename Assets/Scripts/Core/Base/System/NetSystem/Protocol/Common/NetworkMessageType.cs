/// <summary>
/// 消息协议编号分段
/// 1000～1999 C2S 客户端请求
/// 2000～2999 S2C 服务器响应
/// </summary>
public static class NetworkMessageType
{
    public enum C2S : ushort
    {
        LoginRequest = 1000,
        JoinRequest = 1001, // 加入游戏房间请求
        CatInput = 1002, // 输入上传
    }

    public enum S2C : ushort
    {
        LoginResponse = 2000,
        JoinResponse = 2001, // 加入房间响应
        CatSnapshot = 2002, // 状态回传
    }
}