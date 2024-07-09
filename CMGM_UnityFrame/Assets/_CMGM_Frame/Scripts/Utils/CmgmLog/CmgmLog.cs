using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制台信息打印的封装
/// （方便管理是否开启控制台打印、方便管理编辑器模式和运行时模式下控制台打印的区别）
/// </summary>
public class CmgmLog
{
    public static void FrameLogPositive(string log)
    {
        if (!CmgmFrameSettings.Instance.IS_LOG_ACTIVE) return;

        Debug.Log($"<color=Cyan>[蝉的框架]</color>：{log}");
    }
    public static void FrameLog_Normal(string log)
    {
        if (!CmgmFrameSettings.Instance.IS_LOG_ACTIVE) return;

        Debug.Log($"<color=LightCyan>[蝉的框架]</color>：{log}");
    }
    public static void FrameLog_Negative(string log)
    {
        if(!CmgmFrameSettings.Instance.IS_LOG_ACTIVE) return;

        Debug.Log($"<color=#e0c110>[蝉的框架]</color>：{log}");
    }
    public static void FrameLogWarning(string log)
    {
        if (!CmgmFrameSettings.Instance.IS_LOG_ACTIVE) return;

        Debug.LogWarning($"<color=#d69509>[蝉的框架]</color>：{log}");
    }
    
    public static void FrameLogError(string log)
    {
        if(!CmgmFrameSettings.Instance.IS_LOG_ACTIVE) return;

        Debug.LogError($"<color=#FF7F00>[蝉的框架]</color>：{log}");
    }

    public static void TODO(string log)
    {
        if (!CmgmFrameSettings.Instance.IS_LOG_ACTIVE) return;

        Debug.Log($"<color=#d4eb07>[TODO]</color>：{log}");
    }
}
