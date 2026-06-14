using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CmgmFrameSettings : ScriptableObject
{
    private static CmgmFrameSettings instance;
    public static CmgmFrameSettings Instance
        => instance ??= Resources.Load<CmgmFrameSettings>(nameof(CmgmFrameSettings));

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
