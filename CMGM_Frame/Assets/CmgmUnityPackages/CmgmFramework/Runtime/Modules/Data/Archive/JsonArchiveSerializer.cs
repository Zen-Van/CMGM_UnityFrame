using System;
using System.Text;
using Newtonsoft.Json;

namespace CMGM.Data
{
    /// <summary>
    /// 使用 Newtonsoft.Json 的存档 payload 编解码（UTF-8 JSON）。
    /// </summary>
    public sealed class JsonArchiveSerializer : IArchiveSerializer
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Include,
        };

        public byte[] Serialize(ISaveable obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            string json = JsonConvert.SerializeObject(obj, Settings);
            return Encoding.UTF8.GetBytes(json);
        }

        public T Deserialize<T>(byte[] payload) where T : class, ISaveable
        {
            if (payload == null || payload.Length == 0)
                return default;

            string json = Encoding.UTF8.GetString(payload);
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }
    }
}
