using CMGM.Core;
using CMGM.UI;
using UnityEngine;

public class InputManager : SingletonAutoMono<InputManager>
{
    protected override void Awake()
    {
        base.Awake();

        //初始化inputActions
        inputActions = new InputActions_Main();
        inputActions.Enable();

        #region 初始化输入系统时就注册的游戏输入事件(系统输入事件)
        //顶层UI隐藏事件（当输入UI取消按钮时，隐藏顶层面板）
        UI.Cancel.started += (ctx) =>
        {
            if (UIManager.Instance.GetTopDynamicPanel() != null)
                UIManager.Instance.HidePanel(UIManager.Instance.GetTopDynamicPanel().name, false);
        };
        #endregion
    }

    private InputActions_Main inputActions;
    /// <summary> 所有玩家输入集 </summary>
    public InputActions_Main.GamePlayActions Gameplay => inputActions.GamePlay;
    /// <summary> 所有UI输入集 </summary>
    public InputActions_Main.UIActions UI => inputActions.UI;
}
