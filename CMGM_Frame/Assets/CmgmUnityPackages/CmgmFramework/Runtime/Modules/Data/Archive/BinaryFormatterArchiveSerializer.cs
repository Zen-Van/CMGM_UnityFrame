using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace CMGM.Data
{
    /// <summary>
    /// 使用 <see cref="BinaryFormatter"/> 的存档 payload 编解码（技术债，由存档格式优化1.2替换）。
    /// </summary>
    public sealed class BinaryFormatterArchiveSerializer : IArchiveSerializer
    {
        public byte[] Serialize(ISaveable obj)
        {
            using var ms = new MemoryStream();
            new BinaryFormatter().Serialize(ms, obj);
            return ms.ToArray();
        }

        public T Deserialize<T>(byte[] payload) where T : class, ISaveable
        {
            if (payload == null || payload.Length == 0)
                return default;

            using var ms = new MemoryStream(payload);
            return new BinaryFormatter().Deserialize(ms) as T;
        }
    }
}
