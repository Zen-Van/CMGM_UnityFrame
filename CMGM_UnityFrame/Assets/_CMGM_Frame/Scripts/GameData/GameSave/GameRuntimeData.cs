using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//会在游戏存档周期内整体占用内存的数据
[System.Serializable]
public class GameRuntimeData : I_Saveable
{
    /// <summary>
    /// 当前游戏运行时的数据对象
    /// <para>请不要在业务代码中随意更改其值</para>
    /// <para>存档载入前该值为空，切换存档时该值发生变化</para>
    /// </summary>
    [System.NonSerialized] public static GameRuntimeData Instance = null;


    #region 场景数据
    /// <summary>
    /// 所有加载过的场景（所有该存档角色去过的场景）
    /// </summary>
    public List<string> SavedScenes = new List<string>();

    /// <summary>
    /// 世界中的拾取物数据，数据结构为（拾取物GUID , 是否已被拾取）
    /// </summary>
    public Dictionary<string, bool> PickUps = new Dictionary<string, bool>();
    //因为拾取物由系统自动管理，不需要策划配置，因此用GUID。而对白需要从配置表中读取，因此使用策划配置的ID。
    /// <summary>
    /// 世界中的对白数据，数据结构为（对白GUID , 对白返回值）
    /// </summary>
    public Dictionary<string, short> Dialogs = new Dictionary<string, short>();
    //返回值记录了对白的状态，0为未触发过，有选项的对白不同选项返回值不同，有些对白需要计数那么每次说完返回值++

    #endregion


    #region 系统数据
    /// <summary>
    /// 博物词条，数据结构为（博物词条ID，是否解锁）
    /// </summary>
    public Dictionary<string, bool> MuseumItem = new Dictionary<string, bool>();

    /// <summary>
    /// 主线任务
    /// </summary>
    public List<int> MainQuestId = new List<int>();
    /// <summary>
    /// 支线任务
    /// </summary>
    public List<int> SideQuestId = new List<int>();
    #endregion


    #region 背包数据
    /// <summary>
    /// 金钱数据
    /// </summary>
    public int Money = 0;
    /// <summary>
    /// 背包数据，数据结构为 List <（道具ID,道具数目）>
    /// </summary>
    public Dictionary<string, int> BagDatas = new Dictionary<string, int>();
    #endregion

    #region 玩家数据
    public string Player_curScene = "MC_MapTest_01";
    public float Player_Pos_X = 0;
    public float Player_Pos_Y = 0;
    public float Player_Pos_Z = 0;

    #endregion

}
