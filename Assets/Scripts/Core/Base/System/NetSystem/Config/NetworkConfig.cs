using UnityEngine;

[CreateAssetMenu(menuName = "FurryChaos/Network Config", fileName = "NetworkConfig")]
public sealed class NetworkConfig : ScriptableObject
{
    [Header("协议")]
    [Min(1)]
    public int ProtocolVersion = 1;

    [Min(1)]
    public int ServerTickRate = 30;

    [Header("服务器地址")]
    public string DevelopmentAddress = "127.0.0.1";
    public ushort DevelopmentPort = 7777;

    public string BootstrapUrl = "https://api.ariesybs.games";

    [Header("连接")]
    [Min(1f)]
    public float ConnectTimeoutSeconds = 10f;

    [Header("心跳")]
    [Min(0.1f)]
    public float HeartbeatIntervalSeconds = 3f;

    [Min(1f)]
    public float HeartbeatTimeoutSeconds = 10f;

    [Min(1f)]
    public float ServerClientTimeoutSeconds = 15f;

    [Header("断线重连")]
    [Min(0)]
    public int MaxReconnectAttempts = 10;

    public float[] ReconnectDelays =
    {
        0f, 1f, 2f, 4f, 8f, 10f
    };

    public float GetReconnectDelay(int attempt)
    {
        if (ReconnectDelays == null || ReconnectDelays.Length == 0)
        {
            return 1f;
        }

        int index = Mathf.Clamp(attempt, 0, ReconnectDelays.Length - 1);

        return Mathf.Max(0f, ReconnectDelays[index]);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        HeartbeatIntervalSeconds = Mathf.Max(0.1f, HeartbeatIntervalSeconds);

        HeartbeatTimeoutSeconds = Mathf.Max(HeartbeatIntervalSeconds * 2f, HeartbeatTimeoutSeconds);

        ServerClientTimeoutSeconds = Mathf.Max(HeartbeatTimeoutSeconds, ServerClientTimeoutSeconds);
    }
#endif
}