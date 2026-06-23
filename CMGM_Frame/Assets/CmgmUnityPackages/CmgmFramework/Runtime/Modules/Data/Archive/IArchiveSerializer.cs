namespace CMGM.Data
{
    /// <summary>
    /// 存档 payload 编解码（不含 .cmgm 容器头与 CipherTool）。
    /// </summary>
    public interface IArchiveSerializer
    {
        byte[] Serialize(ISaveable obj);

        T Deserialize<T>(byte[] payload) where T : class, ISaveable;
    }
}
