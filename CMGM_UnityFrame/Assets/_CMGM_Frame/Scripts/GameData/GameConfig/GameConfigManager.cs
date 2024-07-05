using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
/// <summary>
/// 游戏配置管理器，管理策划配置表中配置的数据的读取和转化
/// </summary>
public class GameConfigManager : Singleton<GameConfigManager>
{
    private GameConfigManager() { }

    /// <summary>
    /// 2进制数据存储位置路径
    /// </summary>
    public static string BINARY_CONFIG_PATH = Application.streamingAssetsPath + "/GameConfig/";
    /// <summary>
    /// 配置数据文件后缀名
    /// </summary>
    public static string DATAFILE_EXTENSION = ".cmgm";
    /// <summary>
    /// 用于存储内存中所有Excel表数据的容器
    /// </summary>
    private Dictionary<string, object> tableDic = new Dictionary<string, object>();


    /// <summary>
    /// 加载Excel表的2进制数据到内存中 （已读过则不重复读）
    /// </summary>
    /// <typeparam name="T">容器类名</typeparam>
    /// <typeparam name="K">数据结构类类名</typeparam>
    public void LoadTable<T, K>()
    {
        //CmgmLog.TODO("这可以优化成只传一个泛型吗,然后数据类的容器结构也再优化一下。\n\n" +

        //    "过了几天后的留言：我感觉都不需要两个类来装数据，不需要容器类，因为容器类里就一个字典，" +
        //    "把字典直接写到管理器类里不就完了。\n" +
        //    "然后GetTable也不需要了，直接如“GameConfigManager.Instance.RoleInfoDic”就得到该字典就完了\n" +
        //    "然后LoadTable就可以改成LoadTable<T> (IDictionary dic),然后通过IDictionary赋值键值对\n" +
        //    "那要改整个配置数据管理的代码了，有空得做了。" +

        //    "又过了几天后的留言：LoadTable<T>这里的T是不是也不用传了，" +
        //    "直接用dic的value的类型getType来反射是不是就行了，之后试试." +

        //    "后续：试完后倾向于认为，别乱改了，能用就行。遂注释掉这条TODO");


        if (tableDic.ContainsKey(typeof(T).Name)) return;

        //读取 excel表对应的2进制文件 来进行解析
        using (FileStream fs = File.Open(BINARY_CONFIG_PATH + typeof(K).Name + DATAFILE_EXTENSION, FileMode.Open, FileAccess.Read))
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

            //创建容器类对象
            Type contaninerType = typeof(T);
            object contaninerObj = Activator.CreateInstance(contaninerType);
            //得到数据结构类的Type
            Type classType = typeof(K);
            //通过反射 得到数据结构类 所有字段的信息
            FieldInfo[] infos = classType.GetFields();

            //读取每一行的信息
            for (int i = 0; i < count; i++)
            {
                //实例化一个数据结构类 对象
                object dataObj = Activator.CreateInstance(classType);
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
                //得到容器对象中的 字典对象
                object dicObject = contaninerType.GetField("dataDic").GetValue(contaninerObj);
                //通过字典对象得到其中的 Add方法
                MethodInfo mInfo = dicObject.GetType().GetMethod("Add");
                //得到数据结构类对象中 指定主键字段的值
                object keyValue = classType.GetField(keyName).GetValue(dataObj);
                mInfo.Invoke(dicObject, new object[] { keyValue, dataObj });
            }

            //把读取完的表记录下来
            tableDic.Add(typeof(T).Name, contaninerObj);

            fs.Close();
        }
    }

    /// <summary>
    /// 从内存中得到一张表的信息(得到前必须先加载)
    /// </summary>
    /// <typeparam name="T">容器类名</typeparam>
    /// <returns></returns>
    public T GetTable<T>() where T : class
    {
        string tableName = typeof(T).Name;
        if (tableDic.ContainsKey(tableName))
            return tableDic[tableName] as T;
        else
        {
            CmgmLog.FrameLogWarning($"未在内存中找到表格{tableName}，返回了空。\n" +
                $"本框架不允许在没有LoadTable的情况下GetTable。请检查table数据是否已载入内存。");
        }
        return null;
    }

}



