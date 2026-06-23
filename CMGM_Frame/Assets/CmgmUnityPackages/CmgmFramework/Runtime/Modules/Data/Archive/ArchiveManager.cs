using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using CMGM.Core;
using UnityEngine;
using UnityEngine.Events;

namespace CMGM.Data
{
/// <summary>
/// 存档管理器，该管理器须在框架启动时预热
/// </summary>
public class ArchiveManager : BootSingleton<ArchiveManager>
{
    /// <summary>
    /// 存档元数据名
    /// </summary>
    private static string ARCHIVE_META_NAME = "ArchiveMeta";

    private static readonly IArchiveSerializer Serializer = new JsonArchiveSerializer();

    private ArchiveManager() { }

    protected override UniTask OnInitAsync()
    {
        LoadArchiveMeta();
        return UniTask.CompletedTask;
    }
    public ArchiveMetaDataSet ArchiveMeta { get; private set; }

    //只有在该管理器初始化时load存档元数据
    //只有每次触发存档操作时save存档元数据
    //因此相关函数及调用都封装进了内部，外部无需关心存档元数据，外部只需要调用runtimedata相关即可
    #region 存档元数据管理
    /// <summary>
    /// 存档元数据的数据结构
    /// <para>包括有几个存档、和每个存档UI界面上显示的数据</para>
    /// </summary>
    public class ArchiveMetaDataSet : ISaveable
    {
        public struct SingleArchiveMeta
        {
            public int? id;
            public string name;
            public System.DateTime? savedTime;
            public int? mainQuestId;

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
        if (!File.Exists(Consts.Paths.ARCHIVE_PATH + "/" + ARCHIVE_META_NAME + Consts.CMGMFILE_EXTENSION))
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
            //得到存档文件列表
            string[] files = Directory.GetFiles(Consts.Paths.ARCHIVE_PATH);
            List<int> archiveIdList = new List<int>();

            //得到实际存在的ID列表
            foreach (string file in files)
            {
                string archiveName = Path.GetFileNameWithoutExtension(file);

                //Debug.Log(archiveName);
                //排除其中的元数据文件本身
                if (archiveName == ARCHIVE_META_NAME) continue;

                int archiveId = GetArchiveIdFromName(archiveName);
                archiveIdList.Add(archiveId);

                //存档Id,在元数据中找不到的情况
                if (!ArchiveMeta.dataSet.ContainsKey(archiveId))
                {
                    //2、添加文件存在的item将其元数据记录为引用丢失值（玩家会看到一条不知道何时何地存储的未知存档，但是可以加载进去游戏）
                    ArchiveMeta.dataSet[archiveId] = new ArchiveMetaDataSet.SingleArchiveMeta(null, null, null, null);
                    CmgmLog.fWarning($"发现了存档文件archive_{archiveId}，但没有在元数据集中发现其元数据，已自动添加为未知的存档数据");
                }
            }

            //元数据Id,没有对应存档的情况
            foreach (var containId in ArchiveMeta.dataSet.Keys)
            {
                if (!archiveIdList.Contains(containId))
                {
                    //1、从元数据中删去文件不存在的item（避免点了一条看似存在的存档，但却加载失败）
                    ArchiveMeta.dataSet.Remove(containId);
                    CmgmLog.fWarning($"发现了archive_{containId}的元数据，但没有在文件夹中发现存档文件，已自动删除该存档");
                }
            }

        }
    }
    private void SaveArchiveMeta()
    {
        Save(ArchiveMeta,ARCHIVE_META_NAME);
    }
    #endregion

    #region 运行时数据管理
    public ISaveable RuntimeDataInstance { get; private set; } = null;

    public T GetRuntimeData<T>() where T : class, ISaveable => RuntimeDataInstance as T;

    /// <summary>
    /// 清除当前游戏运行时档案
    /// </summary>
    public void ClearRuntimeData()
    {
        RuntimeDataInstance = null;
    }

    /// <summary>
    /// 新建游戏运行时档案，由调用方提供存档数据实例
    /// </summary>
    /// <param name="runtimeData">游戏层创建的存档数据实例</param>
    /// <param name="initArchive">初始化 RuntimeData 数据</param>
    public void NewRuntimeData(ISaveable runtimeData, UnityAction initArchive)
    {
        initArchive?.Invoke();
        RuntimeDataInstance = runtimeData;
    }

    /// <summary>
    /// 读取游戏运行时档案，将硬盘存档文件读入 RuntimeDataInstance
    /// </summary>
    /// <param name="archiveID">存档 id</param>
    public void LoadRuntimeData<T>(int archiveID) where T : class, ISaveable
    {
        if (!File.Exists(Consts.Paths.ARCHIVE_PATH + "/" + GetArchiveNameFromId(archiveID) + Consts.CMGMFILE_EXTENSION))
        {
            CmgmLog.fError("试图读取不存在的文件："
            + Consts.Paths.ARCHIVE_PATH + "/" + GetArchiveNameFromId(archiveID) + Consts.CMGMFILE_EXTENSION);
            return;
        }

        RuntimeDataInstance = Load<T>(GetArchiveNameFromId(archiveID));
    }

    /// <summary>
    /// 存储游戏运行时档案，将 RuntimeDataInstance 写入硬盘（会同时写入 ArchiveMeta）
    /// </summary>
    /// <param name="archiveIdx">存档栏位</param>
    /// <param name="mainQuestIdForMeta">写入存档元数据的主线任务 ID（游戏层提供）</param>
    public void SaveRuntimeData(int archiveIdx, int? mainQuestIdForMeta = null)
    {
        if (RuntimeDataInstance == null)
        {
            CmgmLog.fError("当前游戏运行时数据为空，无法将空值写入存档文件");
            return;
        }

        ArchiveMeta.dataSet[archiveIdx] = new ArchiveMetaDataSet.SingleArchiveMeta
            (archiveIdx, GetArchiveNameFromId(archiveIdx), System.DateTime.Now, mainQuestIdForMeta);
        SaveArchiveMeta();

        Save(RuntimeDataInstance, GetArchiveNameFromId(archiveIdx));
    }
    #endregion

    #region  基础的方法与函数
    private string GetArchiveNameFromId(int archiveID) => "Archive_" + archiveID;
    private int GetArchiveIdFromName(string archiveName) => int.Parse(archiveName.Substring(8));
    /// <summary>
    /// 序列化一个类的对象并存储
    /// </summary>
    /// <param name="obj">实现了 <see cref="ISaveable"/> 的对象</param>
    /// <param name="fileName">存档名</param>
    private void Save(ISaveable obj, string fileName)
    {
        if (!Directory.Exists(Consts.Paths.ARCHIVE_PATH))
            Directory.CreateDirectory(Consts.Paths.ARCHIVE_PATH);

        byte[] payload = Serializer.Serialize(obj);
        byte[] container = CmgmFileFormat.Pack(CmgmFileKind.Archive, payload);
        CipherTool.Encryption(ref container);

        File.WriteAllBytes(Consts.Paths.ARCHIVE_PATH + "/" + fileName + Consts.CMGMFILE_EXTENSION, container);
    }

    /// <summary>
    /// 数据反序列化成类的对象
    /// </summary>
    /// <typeparam name="T">读取数据的类</typeparam>
    /// <param name="fileName">存档名</param>
    private T Load<T>(string fileName) where T : class, ISaveable
    {
        if (!File.Exists(Consts.Paths.ARCHIVE_PATH + "/" + fileName + Consts.CMGMFILE_EXTENSION))
            return default;

        byte[] raw = File.ReadAllBytes(Consts.Paths.ARCHIVE_PATH + "/" + fileName + Consts.CMGMFILE_EXTENSION);
        CipherTool.Decryption(ref raw);

        byte[] payload = CmgmFileFormat.Unpack(
            raw, CmgmFileKind.Archive, fileName + Consts.CMGMFILE_EXTENSION, out var header);
        T obj = Serializer.Deserialize<T>(payload);
        CmgmFileVersionDebugLog.LogArchiveIfNeeded(fileName + Consts.CMGMFILE_EXTENSION, header, obj);
        return obj;
    }
    #endregion
}
}
