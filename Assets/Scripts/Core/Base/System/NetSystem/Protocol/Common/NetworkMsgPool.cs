using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class NetworkMsgPool
{
    private const int MaxRetainedPerType = 128;

    private static readonly object s_Lock = new();

    private static readonly Dictionary<Type, Stack<NetworkMsg>> s_Pools = new();

    // 用于检测同一个对象被重复归还。
    private static readonly HashSet<NetworkMsg> s_PooledObjects = new(ReferenceComparer.Instance);

    public static T Get<T>() where T : NetworkMsg, new()
    {
        lock (s_Lock)
        {
            var type = typeof(T);

            if (s_Pools.TryGetValue(type, out var pool) && pool.Count > 0)
            {
                var message = (T)pool.Pop();
                s_PooledObjects.Remove(message);
                return message;
            }
        }

        return new T();
    }

    public static void Release(NetworkMsg message)
    {
        if (message == null)
        {
            return;
        }

        lock (s_Lock)
        {
            if (s_PooledObjects.Contains(message))
            {
                throw new InvalidOperationException($"网络消息被重复释放：{message.GetType().Name}");
            }

            message.Reset();

            var type = message.GetType();

            if (!s_Pools.TryGetValue(type, out var pool))
            {
                pool = new Stack<NetworkMsg>();
                s_Pools.Add(type, pool);
            }

            // 防止对象池无限增长。
            if (pool.Count >= MaxRetainedPerType)
            {
                return;
            }

            pool.Push(message);
            s_PooledObjects.Add(message);
        }
    }

    public static int Count<T>() where T : NetworkMsg
    {
        lock (s_Lock)
        {
            return s_Pools.TryGetValue(typeof(T), out var pool) ? pool.Count : 0;
        }
    }

    public static void Clear()
    {
        lock (s_Lock)
        {
            s_Pools.Clear();
            s_PooledObjects.Clear();
        }
    }

    private sealed class ReferenceComparer : IEqualityComparer<NetworkMsg>
    {
        public static readonly ReferenceComparer Instance = new();

        public bool Equals(NetworkMsg x, NetworkMsg y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(NetworkMsg obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}