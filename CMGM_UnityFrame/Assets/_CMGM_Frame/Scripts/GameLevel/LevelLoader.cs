using Cysharp.Threading.Tasks;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 加载普通地图关卡，要先加载场景，再根据存档数据初始化所有的触发器和NPC
/// </summary>
public class LevelLoader:Singleton<LevelLoader> 
{
    private LevelLoader() { }

    /// <summary>
    /// 载入普通世界场景
    /// <para>（之后还可能有特殊场景）</para>
    /// </summary>
    /// <param name="name">场景名</param>
    /// <param name="playerPos">玩家生成点</param>
    /// <returns></returns>
    public async UniTask LoadMapSceneAsync(string name, Vector3 playerPos = default)
    {
        //记录上一个场景
        Scene lastScene = SceneManager.GetActiveScene();

        //全屏显示加载面板

        //加载场景
        AsyncOperation operation = SceneManager.LoadSceneAsync(name,LoadSceneMode.Additive);
        await operation;


        //若加载场景时未找到存档数据
        if (GameRuntimeData.Instance == null)
            return;


        //如果没有初始化过该场景数据，尝试初始化该场景数据
        if (!GameRuntimeData.Instance.SavedScenes.Contains(name))
            GameRuntimeData.Instance.SavedScenes.Add(name);
        else
            Debug.Log("[持久化测试]载入已存在的场景：" + name);


        //得到场景触发器的根节点，准备初始化各个触发器
        var mapTriggerRoot = GameObject.FindWithTag(E_Tag.MapTriggerRoot.ToString());

        //如果没有初始化过拾取物数据，尝试初始化拾取物数据
        UpdatePickups(mapTriggerRoot);
        //如果没有初始化过场景事件数据，尝试初始化场景事件数据
        UpdateMapGameEvents(mapTriggerRoot);

        //初始化新场景的player
        if (GameSystem.curPlayer != null)
            GameSystem.curPlayer.transform.position = playerPos;

        CmgmLog.FrameLogPositive("此处会短暂的出现两个audio listener，因为音频系统还没有写。\n" +
            "后续音频监听器将设计成DontDestroyOnLoad的单例，这样就不会受到场景加载卸载的影响。");
        //关闭旧场景
        await SceneManager.UnloadSceneAsync(lastScene);
        //刷新场景UI
        UIManager.Instance.ClearPanel();
        UIManager.Instance.SetUICameraToMain();
        UIManager.Instance.ShowPanel<LevelPanel>().Forget();
    }



    private void UpdatePickups(GameObject mapTriggerRoot)
    {
        MapPickup[] pickups = mapTriggerRoot?.GetComponentsInChildren<MapPickup>(true); //包括未激活的对象

        if (pickups.IsNullOrEmpty()) return;

        string pickupLog = "";
        //所有pickup
        for (int i = 0; i < pickups.Length; i++)
        {
            //没有 pickup 的ID 就初始化，有的话 就 状态赋值
            if (!GameRuntimeData.Instance.PickUps.ContainsKey(pickups[i].UUID))
            {
                GameRuntimeData.Instance.PickUps[pickups[i].UUID] = pickups[i].gameObject.activeSelf;
                pickupLog += $"DontHas:{pickups[i].UUID}  ————  {GameRuntimeData.Instance.PickUps[pickups[i].UUID]}\n";
            }
            else
            {
                pickups[i].gameObject.SetActive(GameRuntimeData.Instance.PickUps[pickups[i].UUID]);
                pickupLog += $"Has:{pickups[i].UUID}  ————  {GameRuntimeData.Instance.PickUps[pickups[i].UUID]}\n";
            }
        }
        foreach (var pickup in GameRuntimeData.Instance.PickUps.Keys)
        {
            pickupLog += $"\nMyStoreID:{pickup}";
        }

        Debug.Log("[持久化测试]:所有拾取物信息\n" + pickupLog);
    }



    private void UpdateMapGameEvents(GameObject mapTriggerRoot)
    {

    }

}
