using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Level_BagPanel : BasePanel
{
    private TMP_Text txtBagContent;
    protected override void Awake()
    {
        base.Awake();

        txtBagContent = GetControl<TMP_Text>(nameof(txtBagContent));
    }


    private void OnEnable()
    {
        //显示背包道具
        string bagItems = "";
        foreach (var itemId in GameRuntimeData.Instance.BagDatas.Keys)
        {
            string itemName = GameConfigManager.Instance.GetTable<ItemInfoContainer>().dataDic[itemId].name;
            int itemCount = GameRuntimeData.Instance.BagDatas[itemId];

            bagItems += itemName + "------" + itemCount + "\n";
        }
        txtBagContent.text = bagItems;
    }

    protected override void OnButtonClick(string btnName)
    {
        switch (btnName)
        {
            default:
                break;
        }
    }
}