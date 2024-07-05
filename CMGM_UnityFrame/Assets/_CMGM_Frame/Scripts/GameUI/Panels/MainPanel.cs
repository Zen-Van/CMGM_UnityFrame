using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : BasePanel
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnButtonClick(string btnName)
    {
        switch (btnName)
        {
            case "btnStart":
                //新建存档数据
                GameArchiveManager.Instance.NewRuntimeData();
                //根据Runtime数据安装Gameplay运行环境（基础的需要载入内存的东西都载入内存）
                RuntimeEnvSetup.SetupBasic();

                //异步进入游戏
                ScenesManager.Instance.LoadSceneAsync(GameRuntimeData.Instance.Player_curScene,
                    new Vector3(GameRuntimeData.Instance.Player_Pos_X, GameRuntimeData.Instance.Player_Pos_Y, GameRuntimeData.Instance.Player_Pos_Z)).Forget();
                //切换UI
                UIManager.Instance.HidePanel<MainPanel>(true);
                UIManager.Instance.ShowPanel<LevelPanel>().Forget();
                break;
            case "btnLoad":
                //读取存档，将数据写入运行时数据
                //TODO:存档页面显示
                //UIManager.Instance.ShowPanel("Main_ArchivesPanel").Forget();
                GameArchiveManager.Instance.LoadRuntimeData(1);

                //根据Runtime数据安装Gameplay运行环境（基础的需要载入内存的东西都载入内存）
                RuntimeEnvSetup.SetupBasic();
                //根据存档数据，安装其他附加的Gameplay运行环境
                //TODO

                //进入游戏
                ScenesManager.Instance.LoadSceneAsync(GameRuntimeData.Instance.Player_curScene,
                    new Vector3(GameRuntimeData.Instance.Player_Pos_X, GameRuntimeData.Instance.Player_Pos_Y, GameRuntimeData.Instance.Player_Pos_Z)).Forget();

                //切换UI
                UIManager.Instance.HidePanel<MainPanel>(true);
                UIManager.Instance.ShowPanel<LevelPanel>().Forget();
                break;
            case "btnQuit":
                UIManager.Instance.HidePanel(nameof(MainPanel));
                break;
            default:
                break;
        }
    }



}