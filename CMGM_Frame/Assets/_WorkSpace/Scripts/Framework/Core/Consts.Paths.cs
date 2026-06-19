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

            public static string ScriptsPath => WorkSpace + "/Scripts";

            public static string HotRes => WorkSpace + "/HotRes";
            public static string HotScene => HotRes + "/Scenes";
            public static string Lua_Path => HotRes + "/Lua";
            public static string HotRes_UIPanel => WorkSpace + "/HotRes/UI/Panels";

            /// <summary>游戏存档目录（运行时路径）</summary>
            public static string ARCHIVE_PATH => Application.persistentDataPath + "/Archives";

            /// <summary>配表二进制输出目录（StreamingAssets）</summary>
            public static string ConfigData => Application.streamingAssetsPath + "/TableConfig/";

            /// <summary>框架目录（Core + Modules）</summary>
            public static class Framework
            {
                public static string Root => ScriptsPath + "/Framework";
                public static string Core => Root + "/Core";
                public static string Modules => Root + "/Modules";
                /// <summary>框架级 Editor（路径检查、通用模板等，编译边界2.9）</summary>
                public static string Editor => Root + "/Editor";

                /// <summary>Framework/Modules/Data/ 模块（Archive 存档管线 + Config 配表管线）</summary>
                public static class DataModule
                {
                    public static string Root => Modules + "/Data";
                    public static string Archive => Root + "/Archive";
                    public static string Config => Root + "/Config";
                    public static string Editor => Root + "/Editor";
                }
            }

            /// <summary>游戏层目录（Panel、存档结构、配表 Container 等）</summary>
            public static class Game
            {
                public static string Root => ScriptsPath + "/Game";
                public static string UI_Panels => Root + "/UI/Panels";
                /// <summary>游戏运行时存档结构（如 GameRuntimeData）</summary>
                public static string Archive => Root + "/Archive";
                /// <summary>Excel 导表生成的 *Container.cs 输出目录</summary>
                public static string Config => Root + "/Config";
            }
        }
    }
}
