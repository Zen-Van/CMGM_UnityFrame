using UnityEngine;

namespace CMGM.Core
{
    /// <summary>
    /// 控制台信息打印的封装
    /// （方便管理是否开启控制台打印）
    /// （TODO:方便管理log编码防止信息泄露）
    /// </summary>
    public class CmgmLog
    {
        #region 框架Log打印
        public static void fPositive(string log)
        {
            if (!CmgmFrameSettings.Instance.IS_LOG_ACTIVE) return;

            Debug.Log($"<color=Cyan>[小蝉]</color>：{log}");
        }
        public static void fNormal(string log)
        {
            if (!CmgmFrameSettings.Instance.IS_LOG_ACTIVE) return;

            Debug.Log($"<color=white>[小蝉]</color>：{log}");
        }
        public static void fNegative(string log)
        {
            if (!CmgmFrameSettings.Instance.IS_LOG_ACTIVE) return;

            Debug.Log($"<color=#e0c110>[小蝉]</color>：{log}");
        }
        public static void fWarning(string log)
        {
            if (!CmgmFrameSettings.Instance.IS_LOG_ACTIVE) return;

            Debug.LogWarning($"<color=#d69509>[小蝉]</color>：{log}");
        }

        public static void fError(string log)
        {
            Debug.LogError($"<color=#FF7F00>[小蝉]</color>：{log}");
        }
        #endregion

        #region yarnLog打印
        public static void yarnNormal(string log)
        {
            if (!CmgmFrameSettings.Instance.IS_LOG_ACTIVE) return;

            Debug.Log($"<color=#white>[YarnSpinner]</color>：{log}");
        }
        #endregion

        public static void Log(string log)
        {
            if (!CmgmFrameSettings.Instance.IS_LOG_ACTIVE) return;

            Debug.Log($"<color=#gray>[Game]</color>：{log}");
        }

        public static void Warning(string log)
        {
            if (!CmgmFrameSettings.Instance.IS_LOG_ACTIVE) return;

            Debug.LogWarning($"<color=#gray>[Game]</color>：{log}");
        }

        public static void Error(string log)
        {
            Debug.LogError($"<color=#gray>[Game]</color>：{log}");
        }

        public static void TODO(string log)
        {
            if (!CmgmFrameSettings.Instance.IS_LOG_ACTIVE) return;

            Debug.Log($"<color=#d4eb07>[TODO]</color>：{log}");
        }
    }
}
