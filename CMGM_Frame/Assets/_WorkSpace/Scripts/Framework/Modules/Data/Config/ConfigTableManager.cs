using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using CMGM.Core;
using UnityEngine;

namespace CMGM.Data
{
/// <summary>
/// 配表管理器，管理策划配置表中数据的读取和转化
/// </summary>
public class ConfigTableManager : Singleton<ConfigTableManager>
{
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
        if (tableDic.ContainsKey(typeof(T).Name)) return;   //如果已经读过了就不再读了

        #region 旧通过泛型获取Type的过程（需要两个泛型变量，已弃用）
        // //得到容器类的Type
        // Type contaninerType = typeof(T);
        // //得到容器类对象
        // object contaninerObj = Activator.CreateInstance(contaninerType);
        // //得到容器对象中的 字典对象
        // object dicObject = contaninerType.GetField("dataDic").GetValue(contaninerObj);
        // //得到数据结构类的Type
        // Type classType = typeof(K);
        // //实例化一个数据结构类 对象
        // object dataObj = Activator.CreateInstance(classType);
        #endregion
        #region 新通过泛型获取Type的过程（只需要一个泛型变量，已启用）
        //得到容器类的Type
        Type containerType = typeof(T);
        //得到容器类对象
        object contaninerObj = Activator.CreateInstance(containerType);
        //得到容器对象中的 字典对象
        object dicObject = containerType.GetField("dataDic").GetValue(contaninerObj);
        //得到数据结构类的Type
        Type rowType = null;
        // 通过字典对象的实际类型推断值类型
        Type dicType = dicObject.GetType();

        // 尝试作为 Dictionary<,> 类型解析
        if (dicType.IsGenericType && dicType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            Type[] genericArgs = dicType.GetGenericArguments();
            rowType = genericArgs[1]; // 第二个参数是值的类型
        }

        if (rowType == null)
        {
            throw new InvalidOperationException("无法从 dataDic 推断值类型");
        }

        //实例化一个数据结构类 对象
        object dataObj = Activator.CreateInstance(rowType);
        #endregion

        #region 读取硬盘数据
        //读取 excel表对应的2进制文件 来进行解析
        using (FileStream fs = File.Open(
            Consts.Paths.ConfigData + typeof(T).Name + Consts.DATAFILE_EXTENSION,
            FileMode.Open, FileAccess.Read))
        {
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            fs.Close();
            //解密
            CipherTool.Decryption(ref bytes);

            //用于记录当前读取了多少字节了
            int index = 0;

            //读取多少行数据
            int count = BitConverter.ToInt32(bytes, index);
            index += 4;

            //读取主键的名字
            int keyNameLength = BitConverter.ToInt32(bytes, index);
            index += 4;
            string keyName = Encoding.UTF8.GetString(bytes, index, keyNameLength);
            index += keyNameLength;

            //通过反射 得到数据结构类 所有字段的信息
            FieldInfo[] infos = rowType.GetFields();

            //读取每一行的信息
            for (int i = 0; i < count; i++)
            {
                foreach (FieldInfo info in infos)
                {
                    if (info.FieldType == typeof(int))
                    {
                        //相当于就是把2进制数据转为int 然后赋值给了对应的字段
                        info.SetValue(dataObj, BitConverter.ToInt32(bytes, index));
                        index += 4;
                    }
                    else if (info.FieldType == typeof(float))
                    {
                        info.SetValue(dataObj, BitConverter.ToSingle(bytes, index));
                        index += 4;
                    }
                    else if (info.FieldType == typeof(bool))
                    {
                        info.SetValue(dataObj, BitConverter.ToBoolean(bytes, index));
                        index += 1;
                    }
                    else if (info.FieldType == typeof(string))
                    {
                        //读取字符串字节数组的长度
                        int length = BitConverter.ToInt32(bytes, index);
                        index += 4;
                        info.SetValue(dataObj, Encoding.UTF8.GetString(bytes, index, length));
                        index += length;
                    }
                }

                //读取完一行的数据了 应该把这个数据添加到容器对象中
                //通过字典对象得到其中的 Add方法
                MethodInfo mInfo = dicObject.GetType().GetMethod("Add");
                //得到数据结构类对象中 指定主键字段的值
                object keyValue = rowType.GetField(keyName).GetValue(dataObj);
                mInfo.Invoke(dicObject, new object[] { keyValue, dataObj });

            }

            //把读取完的表记录下来
            tableDic.Add(typeof(T).Name, contaninerObj);

            fs.Close();
        }
        #endregion
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