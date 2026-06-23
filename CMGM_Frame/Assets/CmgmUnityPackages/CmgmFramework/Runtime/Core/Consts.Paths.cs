using UnityEngine;

namespace CMGM.Core
{
    /// <summary>
    /// 项目路径与文件后缀常量。WorkSpace 根路径可在 CmgmFrameSettings 中配置，其余路径由此派生。
    /// </summary>
    public static class Consts
    {
        public const string CMGMFILE_EXTENSION = ".cmgm";

        public static class Paths
        {
            /// <summary>
            /// 游戏层工作区根目录（默认 Assets/_WorkSpace，见 CmgmFrameSettings.WORK_SPACE_ROOT）
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

            /// <summary>测试层根目录（不进包，默认 Assets/_TestSpace）</summary>
            public const string TestSpace = "Assets/_TestSpace";

            /// <summary>公共资源根目录（默认 Assets/_PublicRes）</summary>
            public const string PublicRes = "Assets/_PublicRes";

            /// <summary>游戏层脚本根（_WorkSpace/Scripts）</summary>
            public static string ScriptsPath => WorkSpace + "/Scripts";

            /// <summary>测试层脚本根（_TestSpace/Scripts）</summary>
            public static string TestScriptsPath => TestSpace + "/Scripts";

            /// <summary>可跨项目拷贝的包根目录（项目脚手架1.4）</summary>
            public const string PackageRoot = "Assets/CmgmUnityPackages";

            /// <summary>CmgmUnityPackages 下各包路径</summary>
            public static class Package
            {
                public static string Framework => PackageRoot + "/CmgmFramework";
                public static string GameKits => PackageRoot + "/CmgmGameKits";
            }

            public static string HotRes => WorkSpace + "/HotRes";
            public static string HotScene => HotRes + "/Scenes";
            public static string Lua_Path => HotRes + "/Lua";
            public static string HotRes_UIPanel => WorkSpace + "/HotRes/UI/Panels";

            /// <summary>游戏存档目录（运行时路径）</summary>
            public static string ARCHIVE_PATH => Application.persistentDataPath + "/Archives";

            /// <summary>配表二进制输出目录（StreamingAssets）</summary>
            public static string ConfigData => Application.streamingAssetsPath + "/TableConfig/";

            /// <summary>框架目录（Resources + Editor + Runtime 三分）</summary>
            public static class Framework
            {
                public static string Root => Package.Framework;
                /// <summary>运行时代码根（Core / Modules / Integrations / Bootstrap）</summary>
                public static string Runtime => Root + "/Runtime";
                public static string Core => Runtime + "/Core";
                public static string Modules => Runtime + "/Modules";
                public static string Bootstrap => Runtime + "/Bootstrap";
                public static string Integrations => Runtime + "/Integrations";
                /// <summary>框架级 Editor（路径检查、通用模板等，编译边界2.9）</summary>
                public static string Editor => Root + "/Editor";

                /// <summary>框架内置 Resources（Settings、UI 基建、Logo、字体；Resources.Load）</summary>
                public static string Resources => Root + "/Resources";

                /// <summary>Runtime/Modules/Data/ 模块（Archive 存档管线 + Config 配表管线）</summary>
                public static class DataModule
                {
                    public static string Root => Modules + "/Data";
                    public static string Archive => Root + "/Archive";
                    public static string Config => Root + "/Config";
                    public static string Editor => Root + "/Editor";
                }
            }

            /// <summary>_WorkSpace/Scripts 下子目录（游戏层代码；无 Scripts/Game 中间层）</summary>
            public static class WorkSpaceScripts
            {
                public static string Root => ScriptsPath;
                public static string Bootstrap => Root + "/Bootstrap";
                public static string UI_Panels => Root + "/UI/Panels";
                /// <summary>游戏运行时存档结构脚本（如 GameRuntimeData）</summary>
                public static string Archive => Root + "/Archive";
                /// <summary>框架/工具链自动生成的脚本（勿手改）</summary>
                public static string Generated => Root + "/_Generated";
                /// <summary>Excel 导表生成的 Container 脚本输出目录</summary>
                public static string Config => Generated + "/Config";
            }
        }
    }
}
