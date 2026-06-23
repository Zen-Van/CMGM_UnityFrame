namespace CMGM.Data
{
    /// <summary>
    /// 配表 payload 编解码（不含 .cmgm 容器头与 CipherTool）。
    /// </summary>
    public interface IConfigTableCodec
    {
        byte[] Encode(ConfigTableEncodeInput input);

        /// <param name="tableContainer">带 <c>dataDic</c> 字段的容器实例。</param>
        /// <param name="rowType"><c>dataDic</c> 的值类型（*Row）。</param>
        /// <returns>解析得到的主键字段名（供调试日志等使用）。</returns>
        string Decode(byte[] payload, object tableContainer, System.Type rowType);
    }
}
