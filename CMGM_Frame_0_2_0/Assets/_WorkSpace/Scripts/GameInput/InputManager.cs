using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    private InputManager()
    {
        inputActions.Enable();

        #region 初始化输入系统时就注册的游戏输入事件(系统输入事件)
        //顶层UI隐藏事件（当输入UI取消按钮时，隐藏顶层面板）
        InputManager.Instance.UI.Cancel.started += (ctx) =>
        {
            if (UIManager.Instance.GetTopDynamicPanel() != null)
                UIManager.Instance.HidePanel(UIManager.Instance.GetTopDynamicPanel().name, false);
        };
        #endregion
    }
    ~InputManager()
    {
        inputActions.Disable();
    }

    private InputActions_Main inputActions = new InputActions_Main();
    /// <summary> 所有玩家输入集 </summary>
    public InputActions_Main.GamePlayActions Gameplay => inputActions.GamePlay;
    /// <summary> 所有UI输入集 </summary>
    public InputActions_Main.UIActions UI => inputActions.UI;
}
