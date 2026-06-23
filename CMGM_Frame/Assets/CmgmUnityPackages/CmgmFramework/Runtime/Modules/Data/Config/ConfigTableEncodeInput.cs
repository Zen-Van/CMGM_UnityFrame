using System.Collections.Generic;

namespace CMGM.Data
{
    /// <summary>
    /// Excel 导表时传入 <see cref="IConfigTableCodec.Encode"/> 的表数据快照。
    /// </summary>
    public sealed class ConfigTableEncodeInput
    {
        public string TableName { get; set; }

        public string KeyFieldName { get; set; }

        /// <summary>与列顺序一致的类型名：int / float / bool / string。</summary>
        public IReadOnlyList<string> ColumnTypes { get; set; }

        /// <summary>数据行；每行与 <see cref="ColumnTypes"/> 等长的单元格字符串。</summary>
        public IReadOnlyList<IReadOnlyList<string>> Rows { get; set; }
    }
}
