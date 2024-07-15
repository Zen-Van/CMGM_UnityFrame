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
        LoadingPanel loadingPanel = await UIManager.Instance.ShowPanel<LoadingPanel>();
        //加载期间，UICamera独立显示
        UIManager.Instance.SetUICameraOverlap(null);
        //加载场景
        AsyncOperation operation = SceneManager.LoadSceneAsync(name);
        loadingPanel.StartLoading(operation);

        //加载场景进度暂时是这样算的：
        //1、场景加载的operation占80%
        //2、载入场景数据并找到各个根节点2%
        //3、初始化角色位置2%
        //4、设置拾取物数据8%
        //5、设置场景事件数据8%
        CmgmLog.FrameLog_Normal("这里进行了部分延时模拟加载，后续可视情况删除或改变其时长");

        #region 加载阶段1：场景加载的operation占80%
        //等待场景加载完毕
        await operation;

        //若加载场景时未找到存档数据
        if (GameRuntimeData.Instance == null)
            return;
        #endregion

        #region 加载阶段2：载入场景数据并找到各个根节点2%
        await UniTask.WaitForSeconds(0.02f);

        //如果没有初始化过该场景数据，尝试初始化该场景数据
        if (!GameRuntimeData.Instance.SavedScenes.Contains(name))
            GameRuntimeData.Instance.SavedScenes.Add(name);
        else
            Debug.Log("[持久化测试]载入已存在的场景：" + name);

        //得到场景触发器的根节点，准备初始化各个触发器
        var mapTriggerRoot = GameObject.FindWithTag(E_Tag.MapTriggerRoot.ToString());
        
        loadingPanel.slider.value += 0.02f;
        #endregion

        #region 加载阶段3：初始化角色位置2%
        await UniTask.WaitForSeconds(0.02f);

        //初始化新场景的player
        if (GameSystem.curPlayer != null)
            GameSystem.curPlayer.transform.position = playerPos;

        loadingPanel.slider.value += 0.02f;
        #endregion

        #region 加载阶段4：设置拾取物数据8%
        await UniTask.WaitForSeconds(0.05f);

        //如果没有初始化过拾取物数据，尝试初始化拾取物数据
        UpdatePickups(mapTriggerRoot);

        loadingPanel.slider.value += 0.08f;
        #endregion

        #region 加载阶段5：设置场景事件数据8%
        await UniTask.WaitForSeconds(0.05f);

        //如果没有初始化过场景事件数据，尝试初始化场景事件数据
        UpdateMapGameEvents(mapTriggerRoot);

        loadingPanel.slider.value += 0.08f;
        #endregion

        //刷新场景UI
        UIManager.Instance.ClearPanel();
        UIManager.Instance.SetUICameraOverlap(Camera.main);//加载场景完毕，将ui相机叠加回主相机
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
