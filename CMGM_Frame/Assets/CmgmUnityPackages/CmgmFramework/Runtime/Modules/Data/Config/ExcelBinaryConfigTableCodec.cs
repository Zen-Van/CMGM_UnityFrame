using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using CMGM.Core;

namespace CMGM.Data
{
    /// <summary>
    /// Excel 配表 v1 自定义二进制 payload 编解码。
    /// </summary>
    public sealed class ExcelBinaryConfigTableCodec : IConfigTableCodec
    {
        private static readonly HashSet<string> ValidTypes = new()
        {
            "int", "float", "bool", "string"
        };

        public byte[] Encode(ConfigTableEncodeInput input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            using var ms = new MemoryStream();
            ms.Write(BitConverter.GetBytes(input.Rows.Count), 0, 4);

            byte[] keyNameBytes = Encoding.UTF8.GetBytes(input.KeyFieldName ?? string.Empty);
            ms.Write(BitConverter.GetBytes(keyNameBytes.Length), 0, 4);
            ms.Write(keyNameBytes, 0, keyNameBytes.Length);

            string emptyPosInfo = "";
            for (int i = 0; i < input.Rows.Count; i++)
            {
                var row = input.Rows[i];
                for (int j = 0; j < input.ColumnTypes.Count; j++)
                {
                    string typeName = input.ColumnTypes[j];
                    string cell = j < row.Count ? row[j] : null;

                    try
                    {
                        if (string.IsNullOrEmpty(cell))
                        {
                            emptyPosInfo += $"（{i},{j}）";
                            WriteDefaultValue(ms, typeName);
                        }
                        else
                        {
                            WriteCellValue(ms, typeName, cell);
                        }
                    }
                    catch (Exception ex)
                    {
                        CmgmLog.fError(
                            $"序列化表{input.TableName}在（{i},{j}）处的值时出现错误，" +
                            $"请检查此处的值变量类型是否正确，是否按规则填写。\n{ex.Message}");
                    }
                }
            }

            if (!string.IsNullOrEmpty(emptyPosInfo))
            {
                CmgmLog.fNegative(
                    $"数据表{input.TableName}中：{emptyPosInfo}处的值为空，写入了对应类型变量的默认值");
            }

            return ms.ToArray();
        }

        public string Decode(byte[] payload, object tableContainer, Type rowType)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));
            if (tableContainer == null)
                throw new ArgumentNullException(nameof(tableContainer));
            if (rowType == null)
                throw new ArgumentNullException(nameof(rowType));

            object dicObject = tableContainer.GetType().GetField("dataDic")?.GetValue(tableContainer)
                ?? throw new InvalidOperationException("容器缺少 dataDic 字段");

            int index = 0;
            int count = BitConverter.ToInt32(payload, index);
            index += 4;

            int keyNameLength = BitConverter.ToInt32(payload, index);
            index += 4;
            string keyName = Encoding.UTF8.GetString(payload, index, keyNameLength);
            index += keyNameLength;

            FieldInfo[] infos = rowType.GetFields();
            MethodInfo addMethod = dicObject.GetType().GetMethod("Add")
                ?? throw new InvalidOperationException("dataDic 缺少 Add 方法");

            for (int i = 0; i < count; i++)
            {
                object rowObj = Activator.CreateInstance(rowType);

                foreach (FieldInfo info in infos)
                {
                    if (info.FieldType == typeof(int))
                    {
                        info.SetValue(rowObj, BitConverter.ToInt32(payload, index));
                        index += 4;
                    }
                    else if (info.FieldType == typeof(float))
                    {
                        info.SetValue(rowObj, BitConverter.ToSingle(payload, index));
                        index += 4;
                    }
                    else if (info.FieldType == typeof(bool))
                    {
                        info.SetValue(rowObj, BitConverter.ToBoolean(payload, index));
                        index += 1;
                    }
                    else if (info.FieldType == typeof(string))
                    {
                        int length = BitConverter.ToInt32(payload, index);
                        index += 4;
                        info.SetValue(rowObj, Encoding.UTF8.GetString(payload, index, length));
                        index += length;
                    }
                }

                object keyValue = rowType.GetField(keyName)?.GetValue(rowObj);
                addMethod.Invoke(dicObject, new[] { keyValue, rowObj });
            }

            return keyName;
        }

        private static void WriteDefaultValue(Stream ms, string typeName)
        {
            switch (typeName)
            {
                case "int":
                    WriteBytes(ms, BitConverter.GetBytes(default(int)));
                    break;
                case "float":
                    WriteBytes(ms, BitConverter.GetBytes(default(float)));
                    break;
                case "bool":
                    WriteBytes(ms, BitConverter.GetBytes(default(bool)));
                    break;
                case "string":
                    WriteBytes(ms, BitConverter.GetBytes(0));
                    break;
                default:
                    throw new NotSupportedException($"不支持的列类型：{typeName}");
            }
        }

        private static void WriteCellValue(Stream ms, string typeName, string cell)
        {
            if (!ValidTypes.Contains(typeName))
                throw new NotSupportedException($"不支持的列类型：{typeName}");

            switch (typeName)
            {
                case "int":
                    WriteBytes(ms, BitConverter.GetBytes(int.Parse(cell)));
                    break;
                case "float":
                    WriteBytes(ms, BitConverter.GetBytes(float.Parse(cell)));
                    break;
                case "bool":
                    WriteBytes(ms, BitConverter.GetBytes(bool.Parse(cell)));
                    break;
                case "string":
                    byte[] strBytes = Encoding.UTF8.GetBytes(cell);
                    WriteBytes(ms, BitConverter.GetBytes(strBytes.Length));
                    ms.Write(strBytes, 0, strBytes.Length);
                    break;
            }
        }

        private static void WriteBytes(Stream ms, byte[] bytes) => ms.Write(bytes, 0, bytes.Length);
    }
}
