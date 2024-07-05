using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

public class GameArchiveManager : Singleton<GameArchiveManager>
{
    /// <summary>
    /// 存档元数据名
    /// </summary>
    private static string ARCHIVE_META_NAME = "ArchiveRoot";
    /// <summary>
    /// 2进制数据存储位置路径
    /// </summary>
    private static string ARCHIVE_PATH = Application.persistentDataPath + "/Archives/";
    /// <summary>
    /// 存档数据文件后缀名
    /// </summary>
    private static string DATAFILE_EXTENSION = ".cmgm";

    private GameArchiveManager() 
    {
        //每次游戏运行GameArchive初始化时，读取存档元数据
        LoadArchiveMeta();
    }
    public ArchiveMetaDataSet ArchiveMeta {  get; private set; }

    //只有在该管理器初始化时load存档元数据
    //只有每次触发存档操作时save存档元数据
    //因此相关函数及调用都封装进了内部，外部无需关心存档元数据，外部只需要调用runtimedata相关即可
    #region 存档元数据管理
    /// <summary>
    /// 存档元数据的数据结构
    /// <para>包括有几个存档、和每个存档UI界面上显示的数据</para>
    /// </summary>
    [System.Serializable]
    public class ArchiveMetaDataSet : I_Saveable
    {
        [System.Serializable]
        public struct SingleArchiveMeta
        {
            int? id;
            string name;
            System.DateTime? savedTime;
            int? mainQuestId;

            public SingleArchiveMeta(int? id, string name, System.DateTime? savedTime, int? mainQuestId)
            {
                this.id = id;
                this.name = name;
                this.savedTime = savedTime;
                this.mainQuestId = mainQuestId;
            }
        }

        public Dictionary<int, SingleArchiveMeta> dataSet = new Dictionary<int, SingleArchiveMeta>();
    }
    private void LoadArchiveMeta()
    {
        //找不到元文件就new一个并save了
        if (!File.Exists(ARCHIVE_PATH + ARCHIVE_META_NAME + DATAFILE_EXTENSION))
        {
            ArchiveMeta = new ArchiveMetaDataSet();
            SaveArchiveMeta();
        }
        else
        {
            ArchiveMeta = Load<ArchiveMetaDataSet>(ARCHIVE_META_NAME);

            /*
            Load后立刻检查元数据和存档文件数目与命名是否对的上:
              1、从元数据中删去文件不存在的item（避免点了一条看似存在的存档，但却加载失败）
              2、添加文件存在的item将其元数据记录为引用丢失值（玩家会看到一条不知道何时何地存储的未知存档，但是可以加载进去游戏）
            */
            string[] files = Directory.GetFiles(ARCHIVE_PATH.Substring(0, ARCHIVE_PATH.Length - 1));
            List<int> trueIdList = new List<int>();

            //得到实际存在的ID列表
            foreach (string file in files)
            {
                string archiveName = Path.GetFileNameWithoutExtension(file);

                //Debug.Log(archiveName);
                //排除根文件
                if (archiveName == ARCHIVE_META_NAME) continue;

                int trueId = GetArchiveIdFromName(archiveName);
                trueIdList.Add(trueId);

                //实际存在的Id,在已记录Id中找不到的情况
                if (!ArchiveMeta.dataSet.ContainsKey(trueId))
                {
                    //2、添加文件存在的item将其元数据记录为引用丢失值（玩家会看到一条不知道何时何地存储的未知存档，但是可以加载进去游戏）
                    ArchiveMeta.dataSet[trueId] = new ArchiveMetaDataSet.SingleArchiveMeta(null,null,null,null);
                    CmgmLog.FrameLogWarning($"发现了存档文件archive_{trueId}，但没有在元数据集中发现其元数据，已自动添加为未知的存档数据");
                }
            }

            //已记录Id,在实际存在的Id中找不到的情况
            foreach (var containId in ArchiveMeta.dataSet.Keys)
            {
                if (!trueIdList.Contains(containId))
                {
                    //1、从元数据中删去文件不存在的item（避免点了一条看似存在的存档，但却加载失败）
                    ArchiveMeta.dataSet.Remove(containId);
                    CmgmLog.FrameLogWarning($"发现了archive_{containId}的元数据，但没有在文件夹中发现存档文件，已自动删除该存档");
                }
            }

        }
    }
    private void SaveArchiveMeta()
    {
        Save(ArchiveMeta, ARCHIVE_META_NAME);
    }
    #endregion

    //每次触发读档操作时load运行时数据
    //每次触发存档操作时save运行时数据
    #region 运行时数据管理
    /// <summary>
    /// 新建一个存档数据并写入GameRuntimeData.Instance
    /// </summary>
    public void NewRuntimeData()
    {
        var tempRuntimeData = new GameRuntimeData();
        //其他初始化逻辑

        //角色初始化、主角入队

        GameRuntimeData.Instance = tempRuntimeData;
    }
    /// <summary>
    /// 读取游戏运行时档案，读入GameRuntimeData.Instance
    /// </summary>
    /// <param name="archiveID">存档id</param>
    public void LoadRuntimeData(int archiveID)
    {
        if (!File.Exists(ARCHIVE_PATH + GetArchiveNameFromId(archiveID) + DATAFILE_EXTENSION))
        {
            CmgmLog.FrameLogError("试图读取不存在的文件：" + ARCHIVE_PATH + GetArchiveNameFromId(archiveID) + DATAFILE_EXTENSION);
            return;
        }

        GameRuntimeData.Instance = Load<GameRuntimeData>(GetArchiveNameFromId(archiveID));
    }
    /// <summary>
    /// 存储游戏运行时档案（会同时写入ArchiveMeta）
    /// </summary>
    /// <param name="archiveID">存档id</param>
    public void SaveRuntimeData(int archiveID)
    {
        if(GameRuntimeData.Instance == null)
        {
            CmgmLog.FrameLogError("当前游戏运行时数据为空，无法将空值写入存档文件");
            return;
        }

        //修改ArchiveMeta数据
        ArchiveMeta.dataSet[archiveID] = new ArchiveMetaDataSet.SingleArchiveMeta
            (archiveID, GetArchiveNameFromId(archiveID), System.DateTime.Now, 
            GameRuntimeData.Instance.MainQuestId.Count > 0 ? GameRuntimeData.Instance.MainQuestId[0] : null); //判断使该值可空
        //保存ArchiveMeta
        SaveArchiveMeta();

        //赋值角色位置
        GameRuntimeData.Instance.Player_Pos_X = GameSystem.curPlayer.transform.position.x;
        GameRuntimeData.Instance.Player_Pos_Y = GameSystem.curPlayer.transform.position.y;
        GameRuntimeData.Instance.Player_Pos_Z = GameSystem.curPlayer.transform.position.z;

        //查看保存的是否正确
        string scenes = "";
        foreach(string s in GameRuntimeData.Instance.SavedScenes)
        {
            scenes += s+"\n";
        }
        Debug.Log(scenes);

        //保存RuntimeData
        Save(GameRuntimeData.Instance, GetArchiveNameFromId(archiveID));
    }
    #endregion

    #region  基础的方法与函数
    private string GetArchiveNameFromId(int archiveID) => "Archive_" + archiveID;
    private int GetArchiveIdFromName(string archiveName) => int.Parse(archiveName.Substring(8));
    /// <summary>
    /// 序列化一个类的对象并存储
    /// </summary>
    /// <param name="obj">继承了接口I_Saveable的类对象</param>
    /// <param name="fileName">存档名</param>
    private void Save(I_Saveable obj, string fileName)
    {
        //先判断路径文件夹有没有
        if (!Directory.Exists(ARCHIVE_PATH))
            Directory.CreateDirectory(ARCHIVE_PATH);

        using(MemoryStream ms = new MemoryStream())
        {
            BinaryFormatter bf = new BinaryFormatter();

            bf.Serialize(ms, obj);
            byte[] data = ms.GetBuffer();

            CipherTool.Encryption(ref data);

            File.WriteAllBytes(ARCHIVE_PATH + fileName + DATAFILE_EXTENSION, data);
            ms.Close();
        }
    }
    /// <summary>
    /// 数据反序列化成类的对象
    /// </summary>
    /// <typeparam name="T">读取数据的类</typeparam>
    /// <param name="fileName">存档名</param>
    private T Load<T>(string fileName) where T : class, I_Saveable
    {
        //如果不存在这个文件 就直接返回泛型对象的默认值
        if (!File.Exists(ARCHIVE_PATH + fileName + DATAFILE_EXTENSION))
            return default;

        byte[] data = File.ReadAllBytes(ARCHIVE_PATH + fileName + DATAFILE_EXTENSION);
        CipherTool.Decryption(ref data);

        T obj;
        using (MemoryStream ms = new MemoryStream(data))
        {
            BinaryFormatter bf = new BinaryFormatter();
            obj = bf.Deserialize(ms) as T;
            ms.Close();
        }

        return obj;
    }
    #endregion
}
