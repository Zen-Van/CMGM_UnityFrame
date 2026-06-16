using System.Collections.Generic;

namespace CMGM.Game
{
    /// <summary>
    /// 游戏运行时存档数据（仅在 <see cref="GameArchiveManager"/> 中持有唯一实例）。
    /// </summary>
    [System.Serializable]
    public class GameRuntimeData : I_Saveable
    {
        #region 场景数据

        /// <summary>
        /// 所有加载过的场景（所有该存档角色去过的场景）
        /// </summary>
        public List<string> SavedScenes = new List<string>();

        /// <summary>
        /// 世界中的拾取物数据，数据结构为（拾取物GUID , 是否已被拾取）
        /// </summary>
        public Dictionary<string, bool> PickUps = new Dictionary<string, bool>();

        /// <summary>
        /// 世界中的对白数据，数据结构为（对白GUID , 对白返回值）
        /// </summary>
        public Dictionary<string, short> Dialogs = new Dictionary<string, short>();

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
        /// 背包数据，数据结构为 List &lt;（道具ID,道具数目）&gt;
        /// </summary>
        public Dictionary<string, int> BagDatas = new Dictionary<string, int>();

        #endregion

        #region 玩家数据

        public float Player_Pos_X = 0;
        public float Player_Pos_Y = 0;
        public float Player_Pos_Z = 0;

        #endregion
    }
}
