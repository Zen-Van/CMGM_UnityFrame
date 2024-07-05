using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogPanel : BasePanel
{
    //控件
    private TMP_Text txtName;
    private TMP_Text txtContent;
    private Image imgRole;

    public bool autoTalk = false;
    [Range(1.0f, 10.0f)]
    public float speed = 5;
    [Range(0.5f, 2.5f)]
    public float staytime = 1;

    /// <summary>
    /// 对话状态   
    /// <para> 0-未开始  1-文字打印中  2-文字完成  3-对话即将关闭 </para>
    /// </summary>
    private byte dialogState = 0;

    protected override void Awake()
    {
        base.Awake(); //查询控件的逻辑都在这里，不能删

        //绑定控件
        txtName = GetControl<TMP_Text>(nameof(txtName));
        txtContent = GetControl<TMP_Text>(nameof(txtContent));
        imgRole = GetControl<Image>(nameof(imgRole));
    }

    private void OnEnable()
    {
        //设置参数
        speed = CmgmFrameSettings.Instance.TEXT_PRINTER_SPEED;
        staytime = CmgmFrameSettings.Instance.TEXT_PRINTER_STAY;
        autoTalk = CmgmFrameSettings.Instance.TEXT_PRINTER_AUTO;


        //禁止局内输入
        InputManager.Instance.Gameplay.Disable();

        //把状态设置成对话中
        dialogState = 1;
        //注册快进对话的事件
        InputManager.Instance.UI.Confirmed.started += OnConfirmed;
    }

    /// <summary>
    /// 执行对话
    /// </summary>
    /// <param name="roleId">说话角色ID</param>
    /// <param name="content">文本内容</param>
    /// <param name="imgId">立绘差分编号(0为无立绘)</param>
    public async UniTask PrintContent(int roleId, string content, int imgId)
    {
        if (content == null)
        {
            CmgmLog.FrameLog_Negative("发现了空的对话内容，放弃执行该对话指令。");
            return;
        }

        //在内存里的配置表中，根据roleId，得到角色的其他一切数据
        string roleName = GameConfigManager.Instance.GetTable<RoleInfoContainer>().dataDic[roleId].name;
        //TODO: 根据角色ID和立绘编号，加载立绘
        if (imgId == 0)  imgRole.gameObject.SetActive(false); 
        else 
        {
            //imgRole.sprite = ResManager.Instance.LoadAsync<Texture2D>(Consts.Paths.AbResPath)
            imgRole.gameObject.SetActive(true); 
        }

        //将角色数据赋值给各个控件并显示
        txtName.text = roleName;
        txtContent.text = content;
        txtContent.maxVisibleCharacters = 0;
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);//txt的赋值，到帧末尾才会应用

        Debug.Log($"[角色说话]{roleName}:{content}");
        //content的内容逐字显示（可以被确认键或交互键打断）
        for (int i = 1; i <= txtContent.textInfo.characterCount; ++i)
        {
            if(dialogState == 2)//状态变成了文字打印完
            {
                txtContent.maxVisibleCharacters = txtContent.textInfo.characterCount;
                break; //完全显示并打断循环
            }

            await UniTask.WaitForSeconds(1 / speed);
            txtContent.maxVisibleCharacters = i;
        }

        //把状态设置成对话完成
        dialogState = 2;

        //自动对话则等待固定时长，非自动对话则需要手动触发结束
        if (autoTalk)
        {
            await UniTask.WaitForSeconds(staytime);
            //对话即将关闭
            dialogState = 3;
        }
        else
            await UniTask.WaitUntil(() => dialogState == 3);
    }

    private void OnDisable()
    {
        //隐藏面板上时，将面板上的显示内容置空
        txtName.text = string.Empty;
        txtContent.text = string.Empty;
        imgRole.sprite = null;

        //取消注册快进事件
        InputManager.Instance.UI.Confirmed.started -= OnConfirmed;
        //设置对话状态
        dialogState = 0;

        //释放局内输入
        InputManager.Instance.Gameplay.Enable();
    }


    private void OnConfirmed(InputAction.CallbackContext context)
    {
        if (dialogState == 1) dialogState = 2;
        else if (dialogState == 2) dialogState = 3;
    }
}