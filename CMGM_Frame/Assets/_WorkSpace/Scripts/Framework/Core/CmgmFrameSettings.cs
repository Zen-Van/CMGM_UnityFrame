using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMGM.Core
{
    public class CmgmFrameSettings : ScriptableObject
    {
        private static CmgmFrameSettings instance;
        public static CmgmFrameSettings Instance
            => instance ??= Resources.Load<CmgmFrameSettings>(nameof(CmgmFrameSettings));

        [BoxGroup("项目路径")]
        [LabelText("工作区根目录")]
        [InfoBox("框架与热更资源的根路径，默认 Assets/_WorkSpace。换项目时可在此修改，Consts.Paths 会从此读取。")]
        public string WORK_SPACE_ROOT = "Assets/_WorkSpace";

        [BoxGroup("流程配置")]
        [LabelText("主场景名（HotRes/Scenes 下，不含路径与后缀）")]
        public string MAIN_SCENE_NAME = "MainScene";

        [BoxGroup("流程配置")]
        [LabelText("主界面 Panel 名（HotRes/UI/Panels 下 Prefab 名，不含路径与后缀）")]
        public string MAIN_PANEL_NAME = "MainPanel";

        [BoxGroup("控制台相关")]
        [LabelText("是否激活LOG系统")]
        public bool IS_LOG_ACTIVE;

        

        [BoxGroup("Lua解析器")]
        [LabelText("Editor下热加载Lua")]
        public bool IS_HOT_LUA;

        [BoxGroup("Lua解析器")]
        [LabelText("Lua引导文件路径")]
        [InfoBox("这里定义了一些公用函数和所有的lua到C#的公共绑定")]
        public string ROOT_LUA_URI;
    }
}
