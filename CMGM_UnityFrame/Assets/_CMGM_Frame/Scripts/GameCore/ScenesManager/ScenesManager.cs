using Cysharp.Threading.Tasks;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


//按这个写法估计就快要重构一下了
public class ScenesManager : Singleton<ScenesManager>
{
    private ScenesManager() { }

    public async UniTask LoadSceneAsync(string name, Vector3 playerPos = default)
    {
        await SceneManager.LoadSceneAsync(name);


        //若加载场景时还没载入存档数据（加载的是非Gameplay场景，如主界面）
        if (GameRuntimeData.Instance == null)
            return;


        //得到需要初始化的各个对象
        var mapTriggerRoot = GameObject.FindWithTag(E_Tag.MapTriggerRoot.ToString());

        //如果没有初始化过该场景数据，尝试初始化该场景数据
        if (!GameRuntimeData.Instance.SavedScenes.Contains(name))
            GameRuntimeData.Instance.SavedScenes.Add(name);
        else
            Debug.Log("[持久化测试]载入已存在的场景：" + name);

        //如果没有初始化过拾取物数据，尝试初始化拾取物数据
        UpdatePickups(mapTriggerRoot);

        CmgmLog.TODO("后续玩家操控的角色动态生成了之后，要在这下面初始化角色的位置和状态");
        if (GameSystem.curPlayer != null)
            GameSystem.curPlayer.transform.position = playerPos;
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
}
