using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CmgmFrameSettings : ScriptableObject
{
    private static CmgmFrameSettings instance;
    public static CmgmFrameSettings Instance
        => instance ??= Resources.Load<CmgmFrameSettings>(nameof(CmgmFrameSettings));

    [BoxGroup("控制台相关")]
    [LabelText("是否激活LOG系统")]
    [Tooltip("游戏发布后可以选择关闭，节约性能并防止玩家通过一些手段看到控制台信息")]
    public bool IS_LOG_ACTIVE;

    [BoxGroup("游戏初始化")]
    [LabelText("主角所在的默认场景名")]
    public string DEFAULT_SCENE;
}
