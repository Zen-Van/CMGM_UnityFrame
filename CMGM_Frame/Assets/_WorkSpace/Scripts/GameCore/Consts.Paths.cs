using UnityEngine;

namespace CMGM.Core
{
    /// <summary>
    /// 项目路径与文件后缀常量。WorkSpace 根路径可在 CmgmFrameSettings 中配置，其余路径由此派生。
    /// </summary>
    public static class Consts
    {
        public const string DATAFILE_EXTENSION = ".cmgm";

        public static class Paths
        {
            /// <summary>
            /// 工作区根目录（默认 Assets/_WorkSpace，见 CmgmFrameSettings.WORK_SPACE_ROOT）
            /// </summary>
            public static string WorkSpace
            {
                get
                {
                    var settings = CmgmFrameSettings.Instance;
                    return settings != null && !string.IsNullOrEmpty(settings.WORK_SPACE_ROOT)
                        ? settings.WORK_SPACE_ROOT
                        : "Assets/_WorkSpace";
                }
            }

            public static string HotRes => WorkSpace + "/HotRes";
            public static string HotScene => HotRes + "/Scenes";
            public static string Lua_Path => HotRes + "/Lua";
            public static string RhythmMap_Path => HotRes + "/RhythmMap";

            public static string ScriptsPath => WorkSpace + "/Scripts";
            public static string Script_UI_Panel_Path => ScriptsPath + "/GameUI/Panels";
            public static string HotRes_UIPanel => WorkSpace + "/HotRes/UI/Panels";

            /// <summary>游戏存档目录（运行时路径）</summary>
            public static string ARCHIVE_PATH => Application.persistentDataPath + "/Archives";

            /// <summary>配表二进制输出目录（StreamingAssets）</summary>
            public static string ConfigData => Application.streamingAssetsPath + "/GameConfig/";
        }
    }
}
