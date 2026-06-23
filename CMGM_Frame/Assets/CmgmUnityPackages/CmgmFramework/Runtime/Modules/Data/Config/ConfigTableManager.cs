using System;
using System.Collections.Generic;
using System.IO;
using CMGM.Core;

namespace CMGM.Data
{
/// <summary>
/// 配表管理器，管理策划配置表中数据的读取和转化
/// </summary>
public class ConfigTableManager : LazySingleton<ConfigTableManager>
{
    private static readonly IConfigTableCodec Codec = new ExcelBinaryConfigTableCodec();

    private ConfigTableManager() { }

    /// <summary>
    /// 用于存储内存中所有Excel表数据的容器
    /// </summary>
    private Dictionary<string, object> tableDic = new Dictionary<string, object>();

    /// <summary>
    /// 加载Excel表的2进制数据到内存中 （已读过则不重复读）
    /// </summary>
    /// <typeparam name="T">表名（xlsx页签名）</typeparam>
    public void LoadTable<T>()
    {
        if (tableDic.ContainsKey(typeof(T).Name)) return;

        Type containerType = typeof(T);
        object containerObj = Activator.CreateInstance(containerType);
        object dicObject = containerType.GetField("dataDic").GetValue(containerObj);

        Type rowType = null;
        Type dicType = dicObject.GetType();
        if (dicType.IsGenericType && dicType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            rowType = dicType.GetGenericArguments()[1];

        if (rowType == null)
            throw new InvalidOperationException("无法从 dataDic 推断值类型");

        string tableName = typeof(T).Name;
        string fileLabel = tableName + Consts.CMGMFILE_EXTENSION;
        string filePath = Consts.Paths.ConfigData + "/" + fileLabel;

        byte[] raw = File.ReadAllBytes(filePath);
        CipherTool.Decryption(ref raw);

        byte[] payload = CmgmFileFormat.Unpack(raw, CmgmFileKind.Config, fileLabel, out var header);
        string keyName = Codec.Decode(payload, containerObj, rowType);

        tableDic.Add(tableName, containerObj);
        CmgmFileVersionDebugLog.LogConfigTableIfNeeded(fileLabel, header, containerObj, rowType, keyName);
    }

    /// <summary>
    /// 从内存中得到一张表的信息(建议有预加载逻辑，不建议未加载就获取)
    /// </summary>
    /// <remarks>提示：如果没有加载过表格，则会实时加载表格，可能导致游戏卡顿</remarks>
    /// <typeparam name="T">表名（xlsx页签名）</typeparam>
    public T GetTable<T>() where T : class
    {
        string tableName = typeof(T).Name;

        if (!tableDic.ContainsKey(tableName))
        {
            CmgmLog.fWarning($"未在内存中找到表格{tableName}，开始实时加载表格。（建议添加预加载逻辑）");
            LoadTable<T>();
            CmgmLog.fNormal($"表格{tableName}读取完成。");
        }

        return tableDic[tableName] as T;
    }
}
}
