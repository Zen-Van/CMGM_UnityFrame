using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPickup : MapInteractable
{
    //地图拾取物
    [Header("该拾取物捡起的道具列表，每个项的输入格式为‘物品ID/物品数量’")]
    public List<string> items = new List<string>();


    //分为有动画表现的宝箱和无动画表现的拾取物
    private bool isChest;

    private void OnEnable()
    {
        OnInteracted += OnPick;
    }
    private void OnDisable()
    {
        OnInteracted -= OnPick;
    }


    //根据列表获得道具
    private void OnPick()
    {
        GameRuntimeData.Instance.PickUps[UUID] = false; //将存档数据改为道具未激活

        //TODO:从数据模块给角色新增道具
        foreach (var item in items)
        {
            string[] itemAndCount = item.Split('/');
            string itemId = itemAndCount[0];
            int count = int.Parse(itemAndCount[1]);

            if (GameRuntimeData.Instance.BagDatas.ContainsKey(itemId))
            {
                GameRuntimeData.Instance.BagDatas[itemId] += count;//有就加数量
            }
            else
            {
                GameRuntimeData.Instance.BagDatas.Add(itemId, count);//没就新增
            }
        }

        //显示层变化
        SetPickActive(false);
    }

    /// <summary>
    /// 根据是否还可拾取设置物体状态
    /// </summary>
    public void SetPickActive(bool isActive)
    {
        if (isChest)
        {

        }
        else
        {
            gameObject.SetActive(isActive);
        }
    }

}
