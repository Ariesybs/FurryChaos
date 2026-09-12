using System.IO;
using UnityEngine;

public static class NetworkUtils
{
    public static void WriteVector2(BinaryWriter writer, Vector2 value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
    }
    public static void WriteVector3(BinaryWriter writer, Vector3 value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
    }

    public static Vector2 ReadVector2(BinaryReader reader)
    {
        return new Vector2(
            reader.ReadSingle(),
            reader.ReadSingle());
    }
    public static Vector3 ReadVector3(BinaryReader reader)
    {
        return new Vector3(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle());
    }

    public static void WriteQuaternion(BinaryWriter writer, Quaternion value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
        writer.Write(value.w);
    }

    public static Quaternion ReadQuaternion(BinaryReader reader)
    {
        return new Quaternion(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(), 
            reader.ReadSingle());
    }
}